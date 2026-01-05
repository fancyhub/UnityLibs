/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/1/4
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FH
{
    public static partial class DeviceInfoIOS
    {
        #region Base
        private const bool ReturnExcpetion = false;
        private static void _PrintException(System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
        private static T _Call<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default(T);
            }
        }
        #endregion

        private static partial class _
        {
            [DllImport("__Internal")] public static extern long FH_GetFreeDiskSpace();
            [DllImport("__Internal")] public static extern long FH_GetTotalDiskSpace();    
        }

        public static long FreeDiskSpace => _Call(_.FH_GetFreeDiskSpace);
        public static long TotalDiskSpace => _Call(_.FH_GetTotalDiskSpace);
       
    }
}