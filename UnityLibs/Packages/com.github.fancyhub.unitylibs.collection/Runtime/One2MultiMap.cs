using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FH
{
    //1 对多的映射
    public class One2MultiMap<TKey, TVal>
    {
        public Dictionary<TKey, LinkedList<TVal>> _dict_key;
        public Dictionary<TVal, LinkedListNode<TVal>> _dict_val;

        public One2MultiMap()
        {
            _dict_key = new Dictionary<TKey, LinkedList<TVal>>();
            _dict_val = new Dictionary<TVal, LinkedListNode<TVal>>();
        }

        public void Clear()
        {
            foreach (var p in _dict_key)
            {
                p.Value.ExtClear();
            }
            _dict_key.Clear();
            _dict_val.Clear();
        }

        public int Count
        {
            get
            {
                return _dict_val.Count;
            }
        }

        //找到第一个
        public bool FindFirst(TKey key, out TVal val)
        {
            //1. 找到
            LinkedList<TVal> list;
            _dict_key.TryGetValue(key, out list);

            //2. 如果list 为空或者数量为0，返回false
            if (list == null || list.Count == 0)
            {
                //my.logT(ResConst.C_LOG_TAG, "One2Multi FindFirst Failed {0}", key);
                val = default(TVal);
                return false;
            }

            //3. 赋值并返回            
            val = list.First.Value;
            //my.logT(ResConst.C_LOG_TAG, "One2Multi FindFirst {0} {1}", key, val);
            return true;
        }

        public bool FindLast(TKey key, out TVal val)
        {
            //1. 找到
            LinkedList<TVal> list;
            _dict_key.TryGetValue(key, out list);

            //2. 如果list 为空或者数量为0，返回false
            if (list == null || list.Count == 0)
            {
                val = default(TVal);
                return false;
            }

            //3. 赋值并返回
            val = list.Last.Value;
            return true;
        }

        public int GetCount(TKey key)
        {
            //1. 找到
            LinkedList<TVal> list;
            _dict_key.TryGetValue(key, out list);
            if (list == null) return 0;
            return list.Count;
        }

        public bool RemoveKey(TKey key, List<TVal> out_val_list)
        {
            if (out_val_list != null)
                out_val_list.Clear();

            //1. 找到
            LinkedList<TVal> list;
            _dict_key.TryGetValue(key, out list);
            if (null == list)
                return false;

            //2. 每个val 要从 dict_val 里面移除，并且添加到 out_val_list
            LinkedListNode<TVal> node = list.First;
            for (; ; )
            {
                if (node == null)
                    break;

                TVal v = node.Value;
                _dict_val.Remove(v);

                if (out_val_list != null)
                    out_val_list.Add(v);
                node = node.Next;
            }

            //3. 移除整个list
            list.ExtClear();
            _dict_key.Remove(key);
            return true;
        }

        public bool RemoveVal(TVal val)
        {
            LinkedListNode<TVal> node;
            _dict_val.TryGetValue(val, out node);
            if (node == null)
                return false;
            node.List.ExtRemove(node);
            _dict_val.Remove(val);
            return true;
        }

        public bool Add(TKey key, TVal val)
        {
            //0. check 是否已经存在
            if (_dict_val.ContainsKey(val))
            {
                UnityEngine.Debug.AssertFormat(false, "已经存在了 {0}", key);
                return false;
            }
            //1. 找到list
            LinkedList<TVal> list = null;
            _dict_key.TryGetValue(key, out list);

            //2. 如果不存在，添加一个
            if (null == list)
            {
                list = new LinkedList<TVal>();
                _dict_key.Add(key, list);
            }

            //4. 添加
            LinkedListNode<TVal> node = list.ExtAddLast(val);
            _dict_val.Add(val, node);
            return true;
        }
    }

    //扩展类
    public static class One2MultiMapExt
    {
        public static bool ExtPop<TKey, TVal>(this One2MultiMap<TKey, TVal> target, TKey key, out TVal val)
        {
            bool ret = target.FindFirst(key, out val);
            if (ret)
                target.RemoveVal(val);
            return ret;
        }
    }
}
