using System;
using System.Collections.Generic;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14 9:28:39
 * Title   : 
 * Desc    : 
*************************************************************************************/
namespace FH
{
    //unity 里面的C#版本，不支持这个函数
    public static class DictionaryExt
    {
        public static bool ExtRemove<TKey, TVal>(this Dictionary<TKey, TVal> self, TKey key, out TVal val)
        {
            bool ret = self.TryGetValue(key, out val);
            if (ret)
                self.Remove(key);
            return ret;
        }
    }
}
