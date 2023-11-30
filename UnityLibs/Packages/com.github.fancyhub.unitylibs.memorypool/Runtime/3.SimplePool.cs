/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/09/04
 * Title   :  SimplePool For C# Collections
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace FH
{
    public static class SimplePool
    {
        public static List<T> AllocList<T>()
        {
            return Pool<List<T>>.Inst.Alloc();
        }

        public static HashSet<T> AllocSet<T>()
        {
            return Pool<HashSet<T>>.Inst.Alloc();
        }

        public static Dictionary<K, V> AllocDict<K, V>()
        {
            return Pool<Dictionary<K, V>>.Inst.Alloc();
        }

        public static AutoList<T> AllocList<T>(out List<T> list)
        {
            list = Pool<List<T>>.Inst.Alloc();
            return new AutoList<T>(list);
        }

        public static AutoSet<T> AllocSet<T>(out HashSet<T> set)
        {
            set = Pool<HashSet<T>>.Inst.Alloc();
            return new AutoSet<T>(set);
        }

        public static AutoDict<K, V> AllocDict<K, V>(out Dictionary<K, V> dict)
        {
            dict = Pool<Dictionary<K, V>>.Inst.Alloc();
            return new AutoDict<K, V>(dict);
        }

        public static void Free<T>(List<T> list)
        {
            if (list == null)
                return;
            list.Clear();
            Pool<List<T>>.Free(list);
        }

        public static void Free<T>(HashSet<T> set)
        {
            if (set == null)
                return;
            set.Clear();
            Pool<HashSet<T>>.Free(set);
        }

        public static void Free<K, V>(Dictionary<K, V> dict)
        {
            if (dict == null)
                return;
            dict.Clear();
            Pool<Dictionary<K, V>>.Free(dict);
        }

        public struct AutoList<T> : IDisposable
        {
            public List<T> Val;
            public AutoList(List<T> v)
            {
                this.Val = v;
            }

            public void Dispose()
            {
                if (Val == null)
                    return;
                List<T> t = Val;
                Val = null;

                Free(t);
            }

            public static implicit operator List<T>(AutoList<T> v) { return v.Val; }
        }

        public struct AutoSet<T> : IDisposable
        {
            public HashSet<T> Val;
            public AutoSet(HashSet<T> v)
            {
                Val = v;
            }

            public void Dispose()
            {
                if (Val == null)
                    return;
                var t = Val;
                Val = null;
                Free(t);
            }

            public static implicit operator HashSet<T>(AutoSet<T> v) { return v.Val; }
        }

        public struct AutoDict<K, V> : IDisposable
        {
            public Dictionary<K, V> Val;
            public AutoDict(Dictionary<K, V> v)
            {
                Val = v;
            }

            public void Dispose()
            {
                if (Val == null)
                    return;
                var t = Val;
                Val = null;
                Free(t);
            }

            public static implicit operator Dictionary<K, V>(AutoDict<K, V> v) { return v.Val; }
        }

        /// <summary>
        /// 清除容器内的所有可回收元素
        /// </summary>
        public static void ExtFreeMembers<T>(this List<T> self) where T : IDestroyable
        {
            if (self == null)
                return;
            foreach (var p in self)
                p?.Destroy();
            self.Clear();
        }

        /// <summary>
        /// 清除容器内的所有可回收元素
        /// </summary>
        public static void ExtFreeMembers<T>(this HashSet<T> self) where T : IDestroyable
        {
            if (self == null)
                return;
            foreach (var p in self)
                p?.Destroy();
            self.Clear();
        }

        /// <summary>
        /// 清除容器内的所有可回收元素
        /// </summary>
        public static void ExtFreeMembers<K, T>(this Dictionary<K, T> self) where T : IDestroyable
        {
            if (self == null)
                return;
            foreach (var p in self)
                p.Value?.Destroy();
            self.Clear();
        }

        private sealed class Pool<T> where T : class, new()
        {
            internal static Pool<T> _;
            private LinkedList<T> s_pooled = new LinkedList<T>();
            private int allocatedCount = 0;

            public static Pool<T> Inst
            {
                get
                {
                    if (_ == null)
                        _ = new Pool<T>();
                    return _;
                }
            }

            public T Alloc()
            {
                if (s_pooled.ExtPopFirst(out T ret))
                    return ret;
                allocatedCount++;
                return new T();
            }

            public static bool Free(T tar)
            {
                if (_ == null)
                    return false;

                if (_.s_pooled.Count < _.allocatedCount)
                {
                    _.s_pooled.ExtAddLast(tar);
                }
                return true;
            }
        }
    }
}
