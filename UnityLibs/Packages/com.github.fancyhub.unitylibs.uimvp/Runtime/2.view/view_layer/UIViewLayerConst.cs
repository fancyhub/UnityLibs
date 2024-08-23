/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/31 17:09:02
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;


namespace FH.UI
{
    public static class UIViewLayerConst
    {
        public const int OrderMin = 100;
        public const int OrderMax = 32767;

        //层和层之间的间隔
        //每个view之间的 order 间隔
        public const int ViewOrderInterval = 100;

        //最多多少个view
        public const int ViewMaxCount = (OrderMax - OrderMin) / ViewOrderInterval;

        //BG 的order 比目标的小5
        public const int BgOrderInterval = 5;

        public static int CalcOrder(int view_idx)
        {
            return OrderMin + view_idx * ViewOrderInterval;
        }
    }     
}
