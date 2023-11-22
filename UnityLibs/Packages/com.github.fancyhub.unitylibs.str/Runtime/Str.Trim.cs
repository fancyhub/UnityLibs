/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{ 
    public partial struct Str
    {
        public Str TrimStart()
        {
            int offset = _offset;
            int len = _len;
            for (int i = _offset; i < _offset + _len; ++i)
            {
                if (!char.IsWhiteSpace(_str[i]))
                {
                    offset = i;
                    break;
                }
                len--;
            }
            return new Str(_str, offset, len);
        }

        public Str TrimEnd()
        {
            int offset = _offset;
            int len = _len;
            for (int i = offset + len - 1; i >= offset; --i)
            {
                if (!char.IsWhiteSpace(_str[i]))
                    break;
                len--;
            }
            return new Str(_str, offset, len);
        }

        public Str TrimEnd(char[] trim_chars)
        {
            int offset = _offset;
            int len = _len;
            for (int i = offset + len - 1; i >= offset; --i)
            {
                if (!_Has(trim_chars, _str[i]))
                    break;
                len--;
            }
            return new Str(_str, offset, len);
        }

        public Str TrimStart(char[] trim_chars)
        {
            int offset = _offset;
            int len = _len;
            for (int i = _offset; i < _offset + _len; ++i)
            {
                if (!_Has(trim_chars, _str[i]))
                {
                    offset = i;
                    break;
                }
                len--;
            }
            return new Str(_str, offset, len);
        }


        public Str Trim(char[] trim_chars)
        {
            Str ret = TrimEnd(trim_chars);
            return ret.TrimStart(trim_chars);
        }

        public Str Trim()
        {
            Str ret = TrimEnd();
            return ret.TrimStart();
        }

        private static bool _Has(char[] trim_chars, char c)
        {
            for (int i = 0; i < trim_chars.Length; ++i)
            {
                if (trim_chars[i] == c)
                    return true;
            }
            return false;
        }
    }     
}
