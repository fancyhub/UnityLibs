/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;

namespace FH
{     
    public partial struct Str
    {
        public int ParseInt32()
        {
            return (int)ParseInt64();
        }

        public uint ParseUInt32()
        {
            return (uint)ParseUInt64();
        }

        public long ParseInt64()
        {
            return _ParseInt64(_str, _offset, _offset + _len);
        }

        public ulong ParseUInt64()
        {
            return _ParseUInt64(_str, _offset, _offset + _len);
        }

        public double ParseDouble()
        {
            if (_str == null || _len == 0)
                return 0;

            int start = _offset;
            int end_offset = _offset + _len;

            double s = 0.0;
            double d = 10.0;
            bool is_neg = false;
            if (_str[start] == '-' || _str[start] == '+')
            {
                is_neg = _str[start] == '-';
                start++;
            }

            bool is_dig = true;
            for (; start < end_offset; start++)
            {
                char b = _str[start];
                if (b >= '0' && b <= '9')
                {
                    if (is_dig)
                        s = s * 10.0 + b - '0';
                    else
                    {
                        s = s + (b - '0') / d;
                        d *= 10.0;
                    }
                }
                else if (b == '.')
                {
                    if (is_dig)
                        is_dig = false;
                    else
                        break;
                }
                else
                {
                    break;
                }
            }

            double v = s * (is_neg ? -1.0 : 1.0);
            if (start < end_offset && (_str[start] == 'E' || _str[start] == 'e'))
            {
                int exp = (int)_ParseInt64(_str, start + 1, end_offset);
                return Math.Pow(10, exp) * v;
            }

            return v;
        }

        public float ParseFloat()
        {
            return (float)ParseDouble();
        }

        private static long _ParseInt64(string str, int start, int end_offset)
        {
            if (str == null || start >= end_offset)
                return 0;

            long ret = 0;
            bool is_neg = false;
            if (str[start] == '-' || str[start] == '+')
            {
                is_neg = str[start] == '-';
                start++;
            }

            for (; start < end_offset; start++)
            {
                char b = str[start];
                if (b >= '0' && b <= '9')
                    ret = ret * 10 + (b - '0');
                else
                    break;
            }

            if (is_neg)
                return -ret;
            return ret;
        }

        private static ulong _ParseUInt64(string str, int start, int end_offset)
        {
            if (str == null || start >= end_offset)
                return 0;

            ulong ret = 0;
            if (str[start] == '+')
                start++;

            for (; start < end_offset; start++)
            {
                char b = str[start];
                if (b >= '0' && b <= '9')
                    ret = ret * 10 + b - '0';
                else
                    break;
            }
            return ret;
        }
    } 
}
