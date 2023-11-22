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
    public partial struct Str : IEquatable<Str>
    {
        public bool Equals(Str other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (obj is string str_val)
            {
                Str str = new Str(str_val);
                return this == str;
            }
            else if (obj is Str str)
            {
                return this == str;
            }
            return false;
        }

        public static bool operator ==(Str self, Str other)
        {
            if (self._len != other._len)
                return false;
            if (self._len == 0)
                return true;

            if (self._len == self._str.Length
                    && other._len == other._str.Length)
                return self._str == other._str;

            for (int i = 0, i_1 = self._offset, i_2 = other._offset
                        ; i < self._len
                        ; ++i, ++i_1, ++i_2)
            {
                if (self._str[i_1] != other._str[i_2])
                    return false;
            }
            return true;
        }

        public static bool operator !=(Str self, Str other)
        {
            if (self._len != other._len)
                return true;
            if (self._len == 0)
                return false;

            if (self._len == self._str.Length
              && other._len == other._str.Length)
                return self._str != other._str;

            for (int i = 0, i_1 = self._offset, i_2 = other._offset
                    ; i < self._len
                    ; ++i, ++i_1, ++i_2)
            {
                if (self._str[i_1] != other._str[i_2])
                    return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash1 = 5381;
            for (int i = _offset; i < _offset + _len; ++i)
            {
                hash1 = ((hash1 << 5) + hash1) ^ _str[i];
            }
            return hash1 * 1566083941;
        }
    }


    public class StrEqualityComparer : IEqualityComparer<Str>
    {
        public static StrEqualityComparer _ = new StrEqualityComparer();

        public bool Equals(Str x, Str y)
        {
            return x == y;
        }
        public int GetHashCode(Str obj)
        {
            return obj.GetHashCode();
        }
    }
}
