/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace FH
{
    public partial struct Str
    {
        public StrEnumerable Split(char split_char, bool remove_empty)
        {
            return new StrEnumerable(this, split_char, remove_empty);
        }

        public StrEnumerable Split(char split_char)
        {
            return new StrEnumerable(this, split_char, false);
        }

        public void Split(char split_char, List<Str> out_list)
        {
            Split(split_char, out_list, false);
        }

        public void Split(char split_char, List<Str> out_list, bool remove_empty)
        {
            out_list.Clear();
            if (_str == null)
                return;
            if (_len == 0)
            {
                if (!remove_empty)
                    out_list.Add(this);
                return;
            }

            int start = _offset;
            int count = _len;
            for (; ; )
            {
                int index = _str.IndexOf(split_char, start, count);
                if (index < 0)
                {
                    Str f1 = new Str(_str, start, count);
                    if (remove_empty && f1.Length == 0)
                        break;

                    out_list.Add(f1);
                    break;
                }

                int count2 = index - start;
                Str f2 = new Str(_str, start, count2);
                if (!(remove_empty && f2.Length == 0))
                    out_list.Add(f2);

                count = count - count2 - 1;
                start = index + 1;
            }
        }

    }

    public struct StrEnumerator : IEnumerator<Str>
    {
        public Str _val;
        public char _split;
        public int _start_index;
        public int _length;
        public bool _remove_empty;

        public StrEnumerator(ref Str str, char split, bool remove_empty)
        {
            _val = str;
            _split = split;
            _start_index = 0;
            _length = -1;
            _remove_empty = remove_empty;
        }

        public Str Current
        {
            get
            {
                if (_length == -1)
                    return string.Empty;

                return _val.Substr(_start_index, _length);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                Str ret = Current;
                return ret;
            }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            for (; ; )
            {
                _start_index = _start_index + _length + 1;
                _length = -1;

                if (_start_index > _val.Length)
                    return false;

                int index = _val.IndexOf(_split, _start_index);
                if (index < 0)
                {
                    _length = _val.Length - _start_index;
                }
                else
                    _length = index - _start_index;

                if (_remove_empty && _length == 0)
                    continue;

                return true;
            }
        }

        public void Reset()
        {
            _start_index = 0;
            _length = -1;
        }
    }

    public struct StrEnumerable : IEnumerable<Str>
    {
        public char _split;
        public Str _str;
        public bool _remove_empty;
        public StrEnumerable(Str str, char split, bool remove_empty)
        {
            _str = str;
            _split = split;
            _remove_empty = remove_empty;
        }
        public StrEnumerator GetEnumerator()
        {
            return new StrEnumerator(ref _str, _split, _remove_empty);
        }
        IEnumerator<Str> IEnumerable<Str>.GetEnumerator()
        {
            return new StrEnumerator(ref _str, _split, _remove_empty);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new StrEnumerator(ref _str, _split, _remove_empty);
        }
    }

    public static class StringExt
    {
        public static StrEnumerable ExtSplit(this string self, char split)
        {
            return new StrEnumerable(self, split, false);
        }
    }     
}
