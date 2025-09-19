/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;

namespace FH
{
    public static class LockKeyExt
    {
        public static string ExtGet(this LocKeyInt key)
        {
            LocMgr.TryGet(key, out var tran);
            return tran;
        }

        public static string ExtGet(this LocKeyStr key)
        {
            LocMgr.TryGet(key, out var tran);
            return tran;
        }

        public static bool ExtTryGet(this LocKeyInt key,out string tran)
        {
            return LocMgr.TryGet(key, out  tran);
        }

        public static bool ExtTryGet(this LocKeyStr key, out string tran)
        {
            return LocMgr.TryGet(key, out tran);
        }
    }
}