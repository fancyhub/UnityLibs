/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/11/6
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    public sealed class UnityObjDict<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TValue : UnityEngine.Object
    {
        private static List<TValue> _SharedList;
        private Dictionary<TKey, TValue> _Data = new Dictionary<TKey, TValue>();

        public TValue Find(TKey key)
        {
            if (!_Data.TryGetValue(key, out var ret))
                return null;

            if (ret == null)
            {
                _Data.Remove(key);
                Log.E("object has been destroyed {0}", key);
                return null;
            }
            return ret;
        }


        public TValue this[TKey key]
        {
            get
            {
                return Find(key);
            }
            set
            {
                Add(key, value);
            }
        }

        public void GetAllValues(List<TValue> out_list)
        {
            if (out_list == null)
            {
                Log.E("param out_list is null ");
                return;
            }

            out_list.Clear();
            if (out_list.Capacity < _Data.Count)
                out_list.Capacity = _Data.Count;

            foreach (var p in _Data)
            {
                if (p.Value == null)
                    continue;
                out_list.Add(p.Value);
            }
        }

        public int Count => _Data.Count;

        public void Clear()
        {
            _Data.Clear();
        }

        public bool Remove(TKey key)
        {
            return _Data.Remove(key);
        }

        public bool Remove(TKey key, out TValue value)
        {
            value = Find(key);
            _Data.Remove(key);
            return value != null;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            value = Find(key);
            return value != null;
        }

        public bool ContainsKey(TKey key)
        {
            return Find(key) != null;
        }

        public List<TValue> GetSharedValueList()
        {
            if (_SharedList == null)
                _SharedList = new List<TValue>(_Data.Count);

            GetAllValues(_SharedList);
            return _SharedList;
        }

        public bool Add(TKey key, TValue value)
        {
            if (value == null)
            {
                Log.E("param value is null {0}", key);
                return false;
            }

            if (Find(key) != null)
            {
                Log.E("{0} is exist", key);
                return false;
            }

            _Data[key] = value;
            return true;
        }

        #region IEnumerable

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private readonly Dictionary<TKey, TValue> _Dict;
            private Dictionary<TKey, TValue>.Enumerator _Orig;

            public Enumerator(Dictionary<TKey, TValue> dict)
            {
                _Dict = dict;
                _Orig = _Dict.GetEnumerator();
            }

            public KeyValuePair<TKey, TValue> Current => _Orig.Current;

            object System.Collections.IEnumerator.Current => _Orig.Current;

            public void Dispose()
            {
                _Orig.Dispose();
            }

            public bool MoveNext()
            {
                for (; ; )
                {
                    if (!_Orig.MoveNext())
                        return false;

                    if (_Orig.Current.Value != null)
                        return true;
                }
            }

            public void Reset()
            {
                _Orig = _Dict.GetEnumerator();
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_Data);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return new Enumerator(_Data);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(_Data);
        }
        #endregion
    }
}
