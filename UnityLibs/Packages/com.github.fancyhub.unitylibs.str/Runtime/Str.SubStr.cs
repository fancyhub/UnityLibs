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
        public Str Substr(int start_index)
        {
            return Substr(start_index, _len - start_index);
        }

        public Str Substr(int start_index, int length)
        {
            if (start_index < 0 || (start_index + length) > Length)
                return string.Empty;
            start_index += _offset;
            return new Str(_str, start_index, length);
        }

        public Str this[int start, int len] { get { return Substr(start, len); } }        
    }
}
