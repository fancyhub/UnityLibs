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
    //http://adammil.net/blog/v111_Creating_High-Performance_Locks_and_Lock-free_Code_for_NET_.html
    public sealed class CasCriticalSection
    {
        private long _Value = 0;

        public bool Enter(long locker_id)
        {
            if (locker_id == 0)
                return false;
            int spinCount = 0;

            while (Interlocked.CompareExchange(ref _Value, locker_id, 0) != 0)
            {
                _SpinWait(spinCount++);
            }
            return true;
        }

        public bool Exit(long locker_id)
        {
            if (locker_id == 0)
                return false;
            return Interlocked.CompareExchange(ref _Value, 0L, locker_id) == locker_id;
        }

        private static void _SpinWait(int spinCount)
        {
            if (spinCount < 10)
                Thread.SpinWait(20 * (spinCount + 1));
            else if (spinCount < 15)
                Thread.Sleep(0); // or use Thread.Yield() in .NET 4
            else
                Thread.Sleep(1);
        }
    }

    public static class CasCriticalSectionExt
    {
        public static Locker ExtLock(this CasCriticalSection self)
        {
            return new Locker(self);
        }

        public struct Locker : IDisposable
        {
            private static long _LockerIdGen = 1;
            private long _LockerId;
            private CasCriticalSection _CriticalSection;
            internal Locker(CasCriticalSection critical_section)
            {
                _CriticalSection = critical_section;
                _LockerId = 0;
                long locker_id = Interlocked.Increment(ref _LockerIdGen);
                if (critical_section.Enter(locker_id))
                    _LockerId = locker_id;
            }

            public void Dispose()
            {
                if (_LockerId == 0 || _CriticalSection == null)
                    return;
                var value = _LockerId;
                var cs = _CriticalSection;
                _CriticalSection = null;
                _LockerId = 0;

                cs.Exit(value);
            }
        }
    }


    /// <summary>
    /// Thread safe Pool    
    /// </summary>
    public sealed class TSPool<T> : IPool<T> where T : class, IPoolItem, new()
    {
        internal static TSPool<T> _;

        public static TSPool<T> Inst
        {
            get
            {
                if (_ == null)
                    _ = new TSPool<T>();
                return _;
            }
        }

        private CasCriticalSection _CriticalSection = new CasCriticalSection();

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
            using var locker = _CriticalSection.ExtLock();
            _free_list.Clear();
        }

        public T New()
        {
            T ret = null;
            using (var locker = _CriticalSection.ExtLock())
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

            using var locker = _CriticalSection.ExtLock();
            _count_del++;
            _free_list.Add(item);
            return true;
        }
    }
      
}
