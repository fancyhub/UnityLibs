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
            if (_len == 0)
                return 0;
            int start = _offset;
            int end_offset = _offset + _len;
            long ret = 0;
            if (end_offset == start)
                return ret;
            bool is_neg = false;
            if (_str[start] == '-') //记录数字正负  
            {
                is_neg = true;
                start++;
            }

            for (; start < end_offset; start++)
            {
                char b = _str[start];
                if (b >= '0' && b <= '9')
                    ret = ret * 10 + (b - '0');
                else
                    break;
            }
            if (is_neg)
                return -ret;
            return ret;
        }

        public ulong ParseUInt64()
        {
            if (_len == 0)
                return 0;
            int start = _offset;
            int end_offset = _offset + _len;
            ulong ret = 0;
            if (end_offset == start)
                return ret;

            for (; start < end_offset; start++)
            {
                char b = _str[start];
                if (b >= '0' && b <= '9')
                    ret = ret * 10 + b - '0';
                else
                    break;
            }
            return ret;
        }

        public double ParseDouble()
        {
            if (_len == 0)
                return 0;
            int start = _offset;
            int end_offset = _offset + _len;

            double s = 0.0;
            if (end_offset == start)
                return s;

            double d = 10.0;
            bool is_neg = false;
            if (_str[start] == '-') //记录数字正负  
            {
                is_neg = true;
                start++;
            }

            bool is_dig = true; //是整数部分
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
            if (end_offset == start)
                return v;

            if (_str[start] == 'E' || _str[start] == 'e')
            {
                start++;
                Str sub = Substr(start);
                int exp = sub.ParseInt32();
                return Math.Pow(10, exp) * v;
            }
            else
                return v;
        }

        public float ParseFloat()
        {
            return (float)ParseDouble();
        }
    } 
}
