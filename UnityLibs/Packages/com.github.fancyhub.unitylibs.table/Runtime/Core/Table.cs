using System;
using System.Collections;
using System.Collections.Generic;


namespace FH
{
    public sealed class Table
    {
        public readonly IList List;
        public readonly Type DataType;

        public readonly Type KeyType;
        public readonly IDictionary Dict;

        public static Table Create<T>(List<T> list) where T : class
        {
            if (list == null)
                return null;
            return new Table(list, typeof(T));
        }

        public static Table Create<TKey, TVal>(List<TVal> list, Dictionary<TKey, TVal> dict) where TVal : class
        {
            if (list == null || dict == null)
                return null;

            return new Table(list, typeof(TVal), dict, typeof(TKey));
        }

        private Table(IList list, Type data_type)
        {
            this.List = list;
            this.DataType = data_type;
        }

        private Table(IList list, Type data_type, IDictionary dict, Type key_type)
        {
            this.List = list;
            this.DataType = data_type;
            this.Dict = dict;
            this.KeyType = key_type;
        }

        public Dictionary<TKey, TItem> GetDict<TKey, TItem>() where TItem : class
        {            
            if (Dict == null)
            {
                Log.E("Table {0} 不存在 Dict<{1},{0}>", typeof(TItem), typeof(TKey));
                return null;
            }

            Dictionary<TKey, TItem> dict_t = Dict as Dictionary<TKey, TItem>;
            if (dict_t == null)
            {
                Log.E("Table {1} 转换失败, Dict<{2},{3}> -> Dict<{0},{1}>", typeof(TKey), typeof(TItem), KeyType, DataType);
                return null;
            }
            return dict_t;
        }

        public List<TItem> GetList<TItem>() where TItem : class
        {            
            if (List == null)
            {
                Log.E("Table {0} 不存在", typeof(TItem));
                return null;
            }
            List<TItem> list_t = List as List<TItem>;
            if (list_t == null)
            {
                Log.E("Table {0} 不存在2", typeof(TItem));
                return null;
            }
            return list_t;
        }

        public TItem Get<TKey, TItem>(TKey id) where TItem : class
        {
            Dictionary<TKey, TItem> dict = GetDict<TKey, TItem>();
            if (dict == null)
                return null;
            bool succ = dict.TryGetValue(id, out TItem ret);
            Log.Assert(succ, "Table {0}, 不存在Key {1}", typeof(TItem), id);
            return ret;
        }

        public bool Get<TKey, TItem>(TKey id, out TItem v) where TItem : class
        {
            v = Get<TKey, TItem>(id);
            return v != null;
        }
    }
}
