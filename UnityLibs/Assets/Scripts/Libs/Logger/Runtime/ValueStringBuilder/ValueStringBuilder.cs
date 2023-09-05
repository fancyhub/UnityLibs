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
        private char[]? _arrayToReturnToPool;
        private bool _growable;
        private Span<char> _chars;
        private int _pos;

        public ValueStringBuilder(Span<char> initialBuffer, bool growable = false)
        {
            _arrayToReturnToPool = null;
            _chars = initialBuffer;
            _pos = 0;
            _growable = growable;
        }

        public ValueStringBuilder(int initialCapacity)
        {
            _arrayToReturnToPool = System.Buffers.ArrayPool<char>.Shared.Rent(initialCapacity);
            _chars = _arrayToReturnToPool;
            _pos = 0;
            _growable = true;
        }

        public bool Append(string s)
        {
            if (s == null)
                return true;
            return Append(s.AsSpan());
        }
        public bool Append(char value)
        {
            int pos = _pos;
            if (pos > _chars.Length - 1)
            {
                if (!_growable)
                    return false;
                Grow(1);
            }
            _chars[_pos] = value;
            _pos += 1;
            return true;
        }

        public bool Append(ReadOnlySpan<char> value)
        {
            int pos = _pos;
            if (pos > _chars.Length - value.Length)
            {
                if (!_growable)
                    return false;
                Grow(value.Length);
            }
            value.CopyTo(_chars.Slice(_pos));
            _pos += value.Length;
            return true;
        }

        private void Grow(int additionalCapacityBeyondPos)
        {
            // Make sure to let Rent throw an exception if the caller has a bug and the desired capacity is negative
            int count = (int)Math.Max((uint)(_pos + additionalCapacityBeyondPos), (uint)_chars.Length * 2);
            char[] poolArray = System.Buffers.ArrayPool<char>.Shared.Rent(count);

            _chars.Slice(0, _pos).CopyTo(poolArray);

            char[]? toReturn = _arrayToReturnToPool;
            _chars = _arrayToReturnToPool = poolArray;
            if (toReturn != null)
            {
                System.Buffers.ArrayPool<char>.Shared.Return(toReturn);
            }
        }

        public override string ToString()
        {
            string s = _chars.Slice(0, _pos).ToString();
            Dispose();
            return s;
        }

        public void Dispose()
        {
            char[]? toReturn = _arrayToReturnToPool;
            this = default; // for safety, to avoid using pooled array if this instance is erroneously appended to again
            if (toReturn != null)
            {
                System.Buffers.ArrayPool<char>.Shared.Return(toReturn);
            }
        }
    }
}
