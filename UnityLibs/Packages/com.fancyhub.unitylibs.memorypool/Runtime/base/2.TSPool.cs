/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

namespace FH
{
    public sealed class SpinCriticalSection
    {
        private long _locked;
        private long _tokenGen;

        public Locker Lock()
        {
            long token = _NextToken();
            var spin = new SpinWait();
            while (Interlocked.CompareExchange(ref _locked, token, 0) != 0)
                spin.SpinOnce();

            return new Locker(this, token);
        }

        private long _NextToken()
        {
            long token = Interlocked.Increment(ref _tokenGen);
            if (token == 0)
                token = Interlocked.Increment(ref _tokenGen);
            return token;
        }

        private void Unlock(long token)
        {
            if (token == 0)
                return;
            Interlocked.CompareExchange(ref _locked, 0, token);
        }

        public struct Locker : IDisposable
        {
            private SpinCriticalSection _owner;
            private long _token;

            internal Locker(SpinCriticalSection owner, long token)
            {
                _owner = owner;
                _token = token;
            }

            public void Dispose()
            {
                var owner = _owner;
                var token = _token;
                if (owner == null)
                    return;

                _owner = null;
                _token = 0;
                owner.Unlock(token);
            }
        }
    }


    /// <summary>
    /// Thread safe Pool    
    /// </summary>
    public sealed class TSPool<T> : IPool<T> where T : class, IPoolItem, new()
    {
        internal static TSPool<T> _ = new TSPool<T>();

        public static TSPool<T> Inst => _;


        private SpinCriticalSection _CriticalSection = new SpinCriticalSection();

        //调用new的次数
        private int _count_new = 0;
        //调用 del的次数
        private int _count_del = 0;
        //同时使用的最大数量
        private int _count_using_max = 0;


        private List<object> _free_list = new List<object>();

        private TSPool()
        {
        }

        /// <summary>
        /// 当前还有多少个是 free的
        /// </summary>
        public int CountFree => _free_list.Count;

        /// <summary>
        /// 还在使用的数量，可能不准确，使用 new - del
        /// </summary>
        public int CountUsing => _count_new - _count_del;

        /// <summary>
        /// 调用了多少次 new
        /// </summary>
        public int CountNew => _count_new;

        /// <summary>
        /// 调用 del的次数
        /// </summary>
        public int CountDel => _count_del;

        /// <summary>
        /// 同时使用的最大数量
        /// </summary>
        public int CountUsingMax => _count_using_max;


        public Type GetObjType() { return typeof(T); }


        public void Clear()
        {
            using var locker = _CriticalSection.Lock();
            _free_list.Clear();
        }

        public T New()
        {
            T ret = null;
            using (var locker = _CriticalSection.Lock())
            {
                _count_new++;
                int count_using = _count_new - _count_del;
                _count_using_max = count_using > _count_using_max ? count_using : _count_using_max;

                if (_free_list.Count > 0)
                {
                    var pool_item = _free_list[_free_list.Count - 1];
                    _free_list.RemoveAt(_free_list.Count - 1);

                    ret = pool_item as T;
                }
            }


            if (ret == null)
            {
                ret = new T();
                ret.Pool = this;
                ret.InPool = true;
                Log.Assert(ret.Pool == this, "要重载该函数");
                Log.Assert(ret.InPool == true, "要重载该函数");
            }

            ret.InPool = false;
            return ret;
        }

        public bool Del(IPoolItem item)
        {
            if (null == item)
                return false;


            using var locker = _CriticalSection.Lock();

            if (item.Pool != this)
            {
                Log.Assert(false, "对象的Pool 不是自己", item.GetType().FullName);
                return false;
            }

            if (item.InPool)
            {
                Log.Assert(false, "重复回收 {0}", item.GetType().FullName);
                return false;
            }

            item.InPool = true;

            _count_del++;
            _free_list.Add(item);
            return true;
        }
    }

}
