/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/5/14 
 * Title   : 
 * Desc    : ref C# ValueStringBuilder
*************************************************************************************/

using System;

namespace FH
{
#pragma warning disable CS8632
    public ref struct ValueStringBuilder
    {
        private Span<char> _Buff;
        private int _Pos;

        public ValueStringBuilder(Span<char> initialBuffer)
        {
            _Buff = initialBuffer;
            _Pos = 0;
        }

        public void Clear()
        {
            _Pos = 0;
        }

        public int Length => _Pos;

        public bool Append(string s)
        {
            if (s == null || s.Length == 0)
                return true;
            return Append(s.AsSpan());
        }

        public bool Append(char value)
        {
            if (_Pos > _Buff.Length - 1)
                return false;
            _Buff[_Pos] = value;
            _Pos += 1;
            return true;
        }

        public bool Append(long value)
        {
            if (_Pos >= _Buff.Length)
                return false;
            if (!value.TryFormat(_Buff.Slice(_Pos), out var count))
                return false;
            _Pos += count;
            return true;
        }

        public bool Append(ulong value)
        {
            if (_Pos >= _Buff.Length)
                return false;
            if (!value.TryFormat(_Buff.Slice(_Pos), out var count))
                return false;
            _Pos += count;
            return true;
        }

        public bool Append(double value)
        {
            if (_Pos >= _Buff.Length)
                return false;
            if (!value.TryFormat(_Buff.Slice(_Pos), out var count))
                return false;
            _Pos += count;
            return true;
        }

        public bool Append(float value)
        {
            if (_Pos >= _Buff.Length)
                return false;
            if (!value.TryFormat(_Buff.Slice(_Pos), out var count))
                return false;
            _Pos += count;
            return true;
        }

        public bool Append(uint value)
        {
            if (_Pos >= _Buff.Length)
                return false;
            if (!value.TryFormat(_Buff.Slice(_Pos), out var count))
                return false;
            _Pos += count;
            return true;
        }

        public bool Append(int value)
        {
            if (_Pos >= _Buff.Length)
                return false;
            if (!value.TryFormat(_Buff.Slice(_Pos), out var count))
                return false;
            _Pos += count;
            return true;
        }

        public bool Append(ReadOnlySpan<char> value)
        {
            if (value.Length == 0)
                return true;

            int copy_count = Math.Min(_Buff.Length - _Pos, value.Length);
            if (copy_count <= 0)
                return false;
            value.Slice(0, copy_count).CopyTo(_Buff.Slice(_Pos));
            _Pos += copy_count;
            return true;
        }

        public override string ToString()
        {
            if (_Pos == 0)
                return string.Empty;

            string s = _Buff.Slice(0, _Pos).ToString();
            _Pos = 0;
            return s;
        }

        public void Dispose()
        {
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again            
        }
    }
}
