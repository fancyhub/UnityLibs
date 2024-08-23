/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/8 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace FH
{
    public static class ListExt
    {
        /// <summary>
        /// 移除index的内容， 是把index的内容和最后一个交换位置， 然后移除最后一个
        /// </summary>        
        public static bool ExtFastRemoveAt<T>(this List<T> self, int index)
        {
            if (self == null)
                return false;

            if (index < 0 || index >= self.Count)
                return false;

            int count = self.Count;
            if (index != count - 1)
                self[index] = self[count - 1];

            self.RemoveAt(count - 1);
            return true;
        }

        /// <summary>
        /// FisherYatesShuffle
        /// </summary>
        public static void ExtShuffle<T>(this IList<T> self)
        {
            if (self == null || self.Count <= 1)
                return;

            for (int i = self.Count - 1; i > 0; i--)
            {
                var j = UnityEngine.Random.Range(0, i + 1);

                T temp = self[i];
                self[i] = self[j];
                self[j] = temp;
            }
        }
    }
}