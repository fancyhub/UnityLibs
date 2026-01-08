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

        public static string SystemVersion
        {
            get
            {
#if UNITY_EDITOR || UNITY_IOS 
                return UnityEngine.iOS.Device.systemVersion;
#else
                return default;
#endif
            }
        }

#if UNITY_EDITOR || UNITY_IOS 
        public static UnityEngine.iOS.DeviceGeneration Generation => UnityEngine.iOS.Device.generation;
#endif

        public static string VendorIdentifier
        {
            get
            {
#if UNITY_EDITOR || UNITY_IOS 
                return UnityEngine.iOS.Device.vendorIdentifier;
#else
                return default;
#endif
            }
        }

        public static string AdvertisingIdentifier
        {
            get
            {
#if UNITY_EDITOR || UNITY_IOS 
                return UnityEngine.iOS.Device.advertisingIdentifier;
#else
                return default;
#endif
            }
        }
    }
}