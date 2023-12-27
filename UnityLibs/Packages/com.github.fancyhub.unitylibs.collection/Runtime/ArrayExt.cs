/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/8 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    public static class ArrayExt
    {
        /// <summary>
        /// 检查 数组的长度和 offset ,count是否一致
        /// </summary>        
        public static bool ExtCheckOffsetCount(this Array self, int offset, int count)
        {
            if (self == null)
                return false;
            if (offset < 0)
                return false;
            if (count <= 0)
                return false;

            int len = self.Length;
            if ((len - offset) < count)
                return false;
            return true;
        }
    }
}