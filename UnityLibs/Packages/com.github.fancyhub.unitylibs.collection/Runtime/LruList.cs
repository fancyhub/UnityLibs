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
    public class LruList<TKey, TVal>
    {
        //key +缓存数据
        private Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TVal>>> _dict;
        private LinkedList<KeyValuePair<TKey, TVal>> _list; //最新修改的在前面，最旧的在后面

        public LruList()
        {
            _dict = new(MyEqualityComparer<TKey>.Default);
            _list = new();
        }

        public LruList(int capacity)
        {
            _dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TVal>>>(capacity);
            _list = new LinkedList<KeyValuePair<TKey, TVal>>();
        }

        public void Clear()
        {
            _list.ExtClear();
            _dict.Clear();
        }

        public bool TryGetVal(TKey key, out TVal v)
        {
            LinkedListNode<KeyValuePair<TKey, TVal>> node = null;
            _dict.TryGetValue(key, out node);
            if (node == null)
            {
                v = default(TVal);
                return false;
            }

            v = node.Value.Value;
            return true;
        }

        public int Count { get { return _list.Count; } }

        /// <summary>
        /// ascend: true 从 旧 -> 新 （旧的数值比较小)
        /// ascend: false 从 新 -> 旧
        /// </summary>
        public void GetSortedList(List<KeyValuePair<TKey, TVal>> out_list, bool ascend, int max_count)
        {
            out_list.Clear();
            if (_list.Count == 0) return;

            if (!ascend)
            {
                LinkedListNode<KeyValuePair<TKey, TVal>> node = _list.First;
                for (; ; )
                {
                    if (null == node || out_list.Count >= max_count)
                        break;
                    out_list.Add(node.Value);
                    node = node.Next;
                }
            }
            else
            {
                LinkedListNode<KeyValuePair<TKey, TVal>> node = _list.Last;
                for (; ; )
                {
                    if (null == node || out_list.Count >= max_count)
                        break;
                    out_list.Add(node.Value);
                    node = node.Previous;
                }
            }
        }

        /// <summary>
        /// 添加/修改
        /// </summary>        
        public void Set(TKey key, TVal value)
        {
            LinkedListNode<KeyValuePair<TKey, TVal>> node;
            _dict.TryGetValue(key, out node);
            if (null == node)
            {
                node = _list.ExtAddFirst(new KeyValuePair<TKey, TVal>(key, value));
                _dict.Add(key, node);
                return;
            }

            //把节点移到前面去
            node.Value = new KeyValuePair<TKey, TVal>(node.Value.Key, value);
            _list.ExtMoveFirst(node);
        }

        public bool Remove(TKey key, out TVal v)
        {
            LinkedListNode<KeyValuePair<TKey, TVal>> node;
            _dict.TryGetValue(key, out node);
            if (node == null)
            {
                v = default(TVal);
                return false;
            }

            v = node.Value.Value;
            _dict.Remove(key);
            _list.ExtRemove(node);
            return true;
        }
    }
}
