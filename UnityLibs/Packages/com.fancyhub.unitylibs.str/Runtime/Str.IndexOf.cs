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
        public int LastIndexOf(char c)
        {
            return LastIndexOf(c, _len - 1);
        }

        public int LastIndexOf(char c, int offset)
        {
            if (_str == null)
                return -1;
            if (offset < 0)
                return -1;
            if (offset >= _len)
                return -1;

            int ret = _str.LastIndexOf(c, offset + _offset, offset + 1);
            if (ret < 0)
                return -1;
            return ret - _offset;
        }

        public int IndexOf(char c, int offset)
        {
            if (_str == null)
                return -1;
            if (offset < 0)
                return -1;
            if (offset >= _len)
                return -1;

            int ret = _str.IndexOf(c, offset + _offset, _len - offset);
            if (ret < 0)
                return -1;
            return ret - _offset;
        }
    }
}
