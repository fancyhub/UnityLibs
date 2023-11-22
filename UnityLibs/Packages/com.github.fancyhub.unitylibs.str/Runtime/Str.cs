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
    /// <summary>
    /// 结构体类型的string
    /// </summary>    
    public partial struct Str
    {
        public static Str Empty = new Str(string.Empty, 0, 0);

        private readonly int _len;
        private readonly int _offset;
        private readonly string _str;

        public Str(string v)
        {
            _str = v;
            _offset = 0;
            _len = 0;
            if (_str != null)
                _len = _str.Length;
        }

        public Str(string v, int offset, int len)
        {
            _str = v;
            _offset = offset;
            _len = len;
        }

        public bool IsEmpty() { return _len == 0; }
        public int Length { get { return _len; } }

        public char this[int idx] { get { return _str[idx + _offset]; } }

        public override string ToString() { return StrVal; }

        public string StrVal
        {
            get
            {
                if (_str == null) return _str;
                if (_offset == 0 && _len == _str.Length) return _str;
                return _str.Substring(_offset, _len);
            }
        }

        public void ToStr(System.Text.StringBuilder sb)
        {
            if (_str == null)
                return;
            if (_offset == 0 && _len == _str.Length)
            {
                sb.Append(_str);
                return;
            }

            for (int i = _offset; i < _offset + _len; ++i)
            {
                sb.Append(_str[i]);
            }
        }        

        public static implicit operator Str(string v) { return new Str(v); }
        public static implicit operator string(Str v) { return v.StrVal; }       
    }
}
