/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/1/4
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace FH
{
    public enum EIOSDistributionType
    {
        Unknown = 0,
        Development = 1,
        AdHoc = 2,
        Enterprise = 3,
        TestFlight = 4,
        AppStore = 5,
    }


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

        private static TRet _Call<TRet, TP0>(Func<TP0, TRet> func, TP0 param0)
        {
            try
            {
                return func(param0);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default(TRet);
            }
        }


        private static readonly Dictionary<string, string> _InfoPlistJsonCache = new Dictionary<string, string>(StringComparer.Ordinal);

        private static readonly Dictionary<string, string> _EntitlementJsonCache = new Dictionary<string, string>(StringComparer.Ordinal);

        private static partial class _
        {
            [DllImport("__Internal")] public static extern long FH_GetFreeDiskSpace();
            [DllImport("__Internal")] public static extern long FH_GetTotalDiskSpace();
            [DllImport("__Internal")] public static extern int FH_GetIOSDistributionType();

            [DllImport("__Internal")] public static extern int FH_HasIOSInfoPlistItem([MarshalAs(UnmanagedType.LPUTF8Str)] string itemKey);
            [DllImport("__Internal")] public static extern IntPtr FH_GetIOSInfoPlistItemJson([MarshalAs(UnmanagedType.LPUTF8Str)] string itemKey);

            [DllImport("__Internal")] public static extern int FH_HasIOSEntitlement([MarshalAs(UnmanagedType.LPUTF8Str)] string entitlementKey);
            [DllImport("__Internal")] public static extern IntPtr FH_GetIOSEntitlementJson([MarshalAs(UnmanagedType.LPUTF8Str)] string entitlementKey);

            [DllImport("__Internal")] public static extern void FH_FreeString(IntPtr str);
        }

        private static string _ReadNativeString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            try
            {
                int length = 0;
                while (Marshal.ReadByte(ptr, length) != 0)
                    length++;

                byte[] bytes = new byte[length];
                Marshal.Copy(ptr, bytes, 0, length);
                return Encoding.UTF8.GetString(bytes);
            }
            finally
            {
                _.FH_FreeString(ptr);
            }
        }

        private static string _GetNativeInfoPlistItemJson(string itemKey)
        {
            if (string.IsNullOrEmpty(itemKey))
                return null;
            string json = _ReadNativeString(_.FH_GetIOSInfoPlistItemJson(itemKey));
            return json;
        }

        private static string _GetNativeEntitlementJson(string entitlementKey)
        {
            if (string.IsNullOrEmpty(entitlementKey))
                return null;
            string json = _ReadNativeString(_.FH_GetIOSEntitlementJson(entitlementKey));
            return json;
        }

        private static string _GetInfoPlistItemJson(string itemKey)
        {
            if (string.IsNullOrEmpty(itemKey))
                return null;

            lock (_InfoPlistJsonCache)
            {
                string cached;
                if (_InfoPlistJsonCache.TryGetValue(itemKey, out cached))
                    return cached;
            }

            string json = _Call(_GetNativeInfoPlistItemJson, itemKey);

            lock (_InfoPlistJsonCache)
            {
                _InfoPlistJsonCache[itemKey] = json;
                return json;
            }
        }

        private static string _GetEntitlementJson(string entitlementKey)
        {
            if (string.IsNullOrEmpty(entitlementKey))
                return null;

            lock (_EntitlementJsonCache)
            {
                string cached;
                if (_EntitlementJsonCache.TryGetValue(entitlementKey, out cached))
                    return cached;
            }

            string json = _Call(_GetNativeEntitlementJson, entitlementKey);

            lock (_EntitlementJsonCache)
            {
                _EntitlementJsonCache[entitlementKey] = json;
            }
            return json;
        }



        #endregion

        public static long FreeDiskSpace => _Call(_.FH_GetFreeDiskSpace);
        public static long TotalDiskSpace => _Call(_.FH_GetTotalDiskSpace);

        public static EIOSDistributionType IOSDistributionType => (EIOSDistributionType)_Call(_.FH_GetIOSDistributionType);




#if UNITY_EDITOR || UNITY_IOS
        public static UnityEngine.iOS.DeviceGeneration Generation => UnityEngine.iOS.Device.generation;
#endif

        [Serializable]
        public class IOSInfoPlistStringItem
        {
            public string type;
            public string value;
        }

        private static string _BuildNumber;
        public static string BuildNumber
        {
            get
            {
                if (_BuildNumber != null)
                    return _BuildNumber;

                string json = _GetInfoPlistItemJson("CFBundleVersion");
                if (json == null)
                {
                    _BuildNumber = string.Empty;
                }
                else
                {
                    IOSInfoPlistStringItem item = JsonUtility.FromJson<IOSInfoPlistStringItem>(json);
                    if (item == null || item.type != "string")
                        _BuildNumber = string.Empty;
                    else
                        _BuildNumber = item.value;
                }
                return _BuildNumber;
            }
        }

        private static string _ApplicationIdentifier;
        public static string ApplicationIdentifier
        {
            get
            {
                if (_ApplicationIdentifier != null)
                    return _ApplicationIdentifier;

                string json = _GetEntitlementJson("application-identifier");
                if (json == null)
                {
                    _ApplicationIdentifier = string.Empty;
                }
                else
                {
                    IOSInfoPlistStringItem item = JsonUtility.FromJson<IOSInfoPlistStringItem>(json);
                    if (item == null || item.type != "string")
                        _ApplicationIdentifier = string.Empty;
                    else
                        _ApplicationIdentifier = item.value;
                }
                return _ApplicationIdentifier;
            }
        }


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
