/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/8 15:43:33
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{

    public sealed class MyEqualityComparer<T>
    {
        internal static IEqualityComparer<T> _Default;
        public static IEqualityComparer<T> Default
        {
            get
            {
#if UNITY_EDITOR
                Type t = typeof(T);
                if (_Default == null && !(t.IsEnum || t.IsPrimitive || t == typeof(string)))
                {
                    Debug.LogError($"{t} 没有 EqualityComparer, 需要注册 MyEqualityComparer.Reg");
                }                
#endif
                return _Default == null ? EqualityComparer<T>.Default : _Default;
            }
        }
    }

    public static class MyEqualityComparer
    {
        public static void Reg<T>(IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null)
                return;
            MyEqualityComparer<T>._Default = equalityComparer;
        }
    }

    /// <summary>
    /// 可以重入的Dict
    /// 最大的内存优化，一个dict 只有linklist 是属于自己的，而不会增加一个数组
    /// </summary>
    public class MyDict<TKey, TVal> : IEnumerable<KeyValuePair<TKey, TVal>>
    {
        //默认的比较器
        private static int C_CAP = 400;
        private static int _id_gen = 0;

        struct InnerKey
        {
            public int Id;
            public TKey Key;
        }

        struct InnerVal
        {
            public TKey Key;
            public TVal Val;
            public bool Empty;
        }

        //inner key的比较器
        private class InnerKeyComparer : IEqualityComparer<InnerKey>
        {
            public static InnerKeyComparer _ = new InnerKeyComparer();
            public bool Equals(InnerKey x, InnerKey y)
            {
                if (x.Id != y.Id)
                    return false;
                return MyEqualityComparer<TKey>.Default.Equals(x.Key, y.Key);
            }

            public int GetHashCode(InnerKey obj)
            {
                return System.HashCode.Combine(MyEqualityComparer<TKey>.Default.GetHashCode(obj.Key), obj.Id);
            }
        }

        //迭代器
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TVal>>
        {
            private MyDict<TKey, TVal> _dict;
            private LinkedListNode<InnerVal> _node;
            private bool _end;

            public Enumerator(MyDict<TKey, TVal> dict)
            {
                _dict = dict;
                if (_dict != null)
                    _dict._it_count++;
                _node = null;
                _end = false;
            }

            public KeyValuePair<TKey, TVal> Current
            {
                get
                {
                    if (_node == null)
                        return default;
                    InnerVal val = _node.Value;
                    return new KeyValuePair<TKey, TVal>(val.Key, val.Val);
                }
            }

            object IEnumerator.Current { get { KeyValuePair<TKey, TVal> ret = Current; return ret; } }

            public void Dispose()
            {
                if (_dict == null)
                    return;
                _dict._it_count--;
                _dict.RemoveInvalidateVal();
                _dict = null;
                _node = null;
            }

            public bool MoveNext()
            {
                if (_end || _dict == null)
                    return false;

                _node = _NextNode(_dict._child_keys, _node);
                if (_node == null)
                {
                    _end = true;
                    return false;
                }
                return true;
            }

            public void Reset()
            {
                _end = false;
                _node = null;
            }

            private LinkedListNode<InnerVal> _NextNode(LinkedList<InnerVal> list, LinkedListNode<InnerVal> node)
            {
                if (node == null)
                    node = list.First;
                else
                    node = node.Next;

                for (; ; )
                {
                    if (node == null)
                        return null;

                    if (!node.Value.Empty)
                        return node;

                    node = node.Next;
                }
            }
        }

        private static Dictionary<InnerKey, LinkedListNode<InnerVal>> _AllChildren;

        private int _id;
        private LinkedList<InnerVal> _child_keys;
        private int _it_count; //当前正在foreach的数量
        private int _count; //因为list的数量不代表当前的数量


        public MyDict()
        {
            _id_gen++;
            _id = _id_gen;
            _child_keys = new LinkedList<InnerVal>();
            if (_AllChildren == null)
                _AllChildren = new Dictionary<InnerKey, LinkedListNode<InnerVal>>(C_CAP, InnerKeyComparer._);
            _it_count = 0;
            _count = 0;
        }

        public int Count { get { return _count; } }

        public bool ContainsKey(TKey key)
        {
            //1. 生成 key
            InnerKey inner_key = new InnerKey()
            {
                Key = key,
                Id = _id,
            };

            return _AllChildren.ContainsKey(inner_key);
        }

        public bool TryGetValue(TKey key, out TVal val)
        {
            //1. 生成 key
            InnerKey inner_key = new InnerKey()
            {
                Key = key,
                Id = _id,
            };

            //2. 根据key 获取val
            _AllChildren.TryGetValue(inner_key, out LinkedListNode<InnerVal> inner_val);

            //3.  如果找不到，就返回默认值，false
            if (inner_val == null || inner_val.Value.Empty)
            {
                val = default;
                return false;
            }

            //4. 返回正确值
            val = inner_val.Value.Val;
            return true;
        }

        public bool Add(TKey key, TVal val)
        {
            return Add(key, val, false);
        }

        //是否可以覆盖
        public bool Add(TKey key, TVal val, bool can_override)
        {
            //1. 生成内部key
            InnerKey inner_key = new InnerKey()
            {
                Key = key,
                Id = _id,
            };

            //2. 获取val
            _AllChildren.ExtRemove(inner_key, out LinkedListNode<InnerVal> node_val);

            //3. 如果存在，并且不允许覆盖
            if (node_val != null && !can_override)
            {
                return false;
            }

            //4. 如果旧的节点存在，先销毁旧的节点
            if (node_val != null)
            {
                _count--;
                //如果不在迭代中
                if (_it_count == 0)
                    _child_keys.ExtRemove(node_val);
                else
                    node_val.Value = new InnerVal()
                    {
                        Empty = true,
                    };
            }

            //5. 生成内部val
            InnerVal inner_val = new InnerVal()
            {
                Key = key,
                Val = val,
                Empty = false
            };

            //6. 直接添加
            node_val = _child_keys.ExtAddLast(inner_val);
            _AllChildren.Add(inner_key, node_val);
            _count++;
            return true;
        }

        public bool Remove(TKey key, out TVal val)
        {
            //1. 生成内部key
            InnerKey inner_key = new InnerKey()
            {
                Key = key,
                Id = _id,
            };

            //2. 获取val
            _AllChildren.ExtRemove(inner_key, out LinkedListNode<InnerVal> node_val);

            //3. 如果旧的节点存在，先销毁旧的节点
            if (node_val == null)
            {
                val = default;
                return false;
            }

            _count--;
            val = node_val.Value.Val;
            //如果不在迭代中
            if (_it_count == 0)
                _child_keys.ExtRemove(node_val);
            else
                node_val.Value = new InnerVal()
                {
                    Empty = true,
                };
            return true;
        }
        public bool Remove(TKey key)
        {
            return Remove(key, out var _);
        }

        public TVal this[TKey key]
        {
            get
            {
                TryGetValue(key, out TVal val);
                return val;
            }
            set
            {
                Add(key, value, true);
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void Clear()
        {
            if (_child_keys.Count == 0)
                return;

            _count = 0;

            //不在迭代中
            if (_it_count == 0)
            {
                var node = _child_keys.First;
                for (; ; )
                {
                    if (node == null)
                        break;

                    InnerVal inner_val = node.Value;
                    if (inner_val.Empty)
                    {
                        node = node.Next;
                        continue;
                    }

                    InnerKey key = new InnerKey()
                    {
                        Id = _id,
                        Key = inner_val.Key,
                    };
                    _AllChildren.Remove(key);
                    node = node.Next;
                }
                _child_keys.ExtClear();
                return;
            }
            else
            {
                var node = _child_keys.First;
                for (; ; )
                {
                    if (node == null)
                        break;
                    node.Value = new InnerVal()
                    {
                        Empty = true,
                    };
                    node = node.Next;
                }
            }
        }

        public void RemoveInvalidateVal()
        {
            if (_it_count != 0)
                return;
            if (_count == _child_keys.Count)
                return;

            var node = _child_keys.First;
            for (; ; )
            {
                if (node == null)
                    break;
                var temp_node = node;
                node = node.Next;

                if (temp_node.Value.Empty)
                {
                    _child_keys.ExtRemove(temp_node);
                }
            }
            UnityEngine.Debug.Assert(_count == _child_keys.Count);
        }

        IEnumerator<KeyValuePair<TKey, TVal>> IEnumerable<KeyValuePair<TKey, TVal>>.GetEnumerator()
        {
            Enumerator ret = GetEnumerator();
            return ret;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            Enumerator ret = GetEnumerator();
            return ret;
        }
    }
}
