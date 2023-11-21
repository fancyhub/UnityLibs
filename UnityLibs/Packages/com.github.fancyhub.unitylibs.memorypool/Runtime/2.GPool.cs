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
    //全局的 管理Pool的, 非线程安全的
    public static class GPool
    {
        public static T New<T>() where T : class, IPoolItem, new()
        {
            //1. 获取 Pool
            IPool<T> pool = Pool<T>.Instance;
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
            IPool<T> pool = Pool<T>.Instance;
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
            IPool<T> pool = Pool<T>.Instance;
            if (pool != null)
                return pool;

            Log.Assert(typeof(T).IsSealed, "使用 Pool 最好是 sealed 类型的类 {0}", typeof(T));
            pool = new Pool<T>(create_func);
            return pool;
        }
    }
}
