/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    /// <summary>
    /// Global Pool
    /// 全局的 管理Pool的, 非线程安全的
    /// </summary>
    public static class GPool
    {
        private const int C_CAPCITY = 500;
        private static Dictionary<Type, IPool> Dict = new Dictionary<Type, IPool>(C_CAPCITY);

        public static Dictionary<Type, IPool> AllPools { get { return Dict; } }

        public static T New<T>() where T : class, IPoolItem, new()
        {
            //1. 获取 Pool
            IPool<T> pool = Pool<T>.Inst;
            if (pool == null)
                pool = CreatePool<T>();

            //2. 检查
            if (pool == null)
                return null;

            //3. new 一个
            return pool.New();
        }

        public static T New<T>(Func<T> create_func) where T : class, IPoolItem
        {
            //1. 获取 Pool
            IPool<T> pool = Pool<T>.Inst;
            if (pool == null)
                pool = CreatePool<T>(create_func);

            //2. 检查
            if (pool == null)
                return null;

            //3. new 一个
            return pool.New();

        }

        public static IPool<T> CreatePool<T>() where T : class, IPoolItem, new()
        {
            return CreatePool<T>(() => { return new T(); });
        }

        public static IPool<T> CreatePool<T>(int cap) where T : class, IPoolItem, new()
        {
            return CreatePool<T>(cap, () => { return new T(); });
        }

        public static IPool<T> CreatePool<T>(int cap, Func<T> create_func) where T : class, IPoolItem
        {
            return CreatePool<T>(cap, create_func);
        }

        public static IPool<T> CreatePool<T>(Func<T> create_func) where T : class, IPoolItem
        {
            IPool<T> pool = Pool<T>.Inst;
            if (pool != null)
                return pool;

            Log.Assert(typeof(T).IsSealed, "使用 Pool 最好是 sealed 类型的类 {0}", typeof(T));
            pool = new Pool<T>(create_func);
            Dict[typeof(T)] = pool;
            return pool;
        }

        #region Implement        

        //	- using, remove, free  
        internal sealed class Pool<T> : IPool<T> where T : class, IPoolItem
        {
            internal static Pool<T> Inst;

            //调用new的次数
            private int _count_new = 0;
            //调用 del的次数
            private int _count_del = 0;
            //同时使用的最大数量
            private int _count_using_max = 0;

            private int _init_cap = int.MaxValue;

            private LinkedList<object> _free_list = new LinkedList<object>();
            private System.Func<T> _create_func;

            public Pool(System.Func<T> create_func)
            {
                _create_func = create_func;
                Inst = this;
            }

            public Pool(int capcity, System.Func<T> create_func)
            {
                _create_func = create_func;
                _InitCap(capcity);
                Inst = this;
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
                _free_list.ExtClear();
            }

            public T New()
            {
                _count_new++;
                int count_using = _count_new - _count_del;
                _count_using_max = count_using > _count_using_max ? count_using : _count_using_max;

                if (_free_list.Count == 0)
                    _CreateFreeItem(1);

                bool succ = _free_list.ExtPopFirst(out object pool_item);
                Log.Assert(succ);

                T ret = pool_item as T;
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

                _count_del++;
                item.InPool = true;
                _free_list.ExtAddLast(item);
                return true;
            }

            /// <summary>
            /// 只能设置一次
            /// </summary>        
            private void _InitCap(int cap)
            {
                if (cap <= 0)
                {
                    Log.Assert(false, "必须大于0 {0}", cap);
                    return;
                }
                if (_init_cap != int.MaxValue)
                {
                    Log.Assert(false, "只能设置一次");
                    return;
                }
                _init_cap = cap;
                int count_using = _count_new - _count_del;
                int count_free = _free_list.Count;
                int dt = _init_cap - count_using - count_free;

                _CreateFreeItem(dt);
            }


            private void _CreateFreeItem(int count)
            {
                for (int i = 0; i < count; ++i)
                {
                    T new_item = _create_func();
                    new_item.Pool = this;
                    new_item.InPool = true;
                    Log.Assert(new_item.Pool == this, "要重载该函数");
                    Log.Assert(new_item.InPool == true, "要重载该函数");
                    _free_list.ExtAddLast(new_item);

                    Log.Assert(_count_using_max <= _init_cap
                       , "PoolItem 的数量已经超过了 最大上限 {0}  Cap:{1}, usingMax:{2}"
                       , new_item.GetType().FullName
                       , _init_cap
                       , _count_using_max);
                }
            }
        }
        #endregion
    }
}
