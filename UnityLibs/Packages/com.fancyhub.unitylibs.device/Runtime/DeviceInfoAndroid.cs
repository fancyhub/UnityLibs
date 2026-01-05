/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/2/2
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Text;
using UnityEngine;

namespace FH
{
    public static class DeviceInfoAndroid
    {
        #region Base
        private const bool ReturnExcpetion = false;
        private static void _PrintException(System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }

        private static T _ExtCall<T>(this AndroidJavaObject self, string name)
        {
            try
            {
                if (self == null) return default(T);//for c++ assert

                return self.Call<T>(name);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default(T);
            }
        }

        private static T _ExtCall<T, TArg0>(this AndroidJavaObject self, string name, TArg0 arg0)
        {
            try
            {
                if (self == null) return default(T);//for c++ assert
                return self.Call<T>(name, arg0);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default(T);
            }
        }

        private static T _ExtCall<T, TArg0, TArg1>(this AndroidJavaObject self, string name, TArg0 arg0, TArg1 arg1)
        {
            try
            {
                if (self == null) return default(T);//for c++ assert

                return self.Call<T>(name, arg0, arg1);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default(T);
            }
        }

        private static T _ExtGet<T>(this AndroidJavaObject self, string name)
        {
            try
            {
                if (self == null) return default(T);//for c++ assert

                return self.Get<T>(name);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default(T);
            }
        }

        private static T _ExtCallStatic<T>(this AndroidJavaClass self, string name)
        {
            try
            {
                if (self == null) return default(T);//for c++ assert

                return self.CallStatic<T>(name);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default(T);
            }
        }

        private static T _ExtGetStatic<T>(this AndroidJavaClass self, string name)
        {
            try
            {
                if (self == null) return default(T);//for c++ assert

                return self.GetStatic<T>(name);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default(T);
            }
        }

        private static UnityEngine.AndroidJavaClass _UnityPlayer;
        private static UnityEngine.AndroidJavaObject _GetCurrentActivity()
        {
            if (_UnityPlayer == null)
                _UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return _UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
        #endregion

        //https://developer.android.google.cn/reference/android/telephony/TelephonyManager
        #region TelephonyManager
        private static AndroidJavaObject _TelephonyManager;
        private const string TELEPHONY_SERVICE = "phone"; //android.content.Context.TELEPHONY_SERVICE
        private static AndroidJavaObject _GetTelephonyManager()
        {
            try
            {
                if (_TelephonyManager == null)
                {
                    AndroidJavaObject currentActivity = _GetCurrentActivity();
                    _TelephonyManager = currentActivity.Call<AndroidJavaObject>("getSystemService", TELEPHONY_SERVICE);
                }
                return _TelephonyManager;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        public static int TelephonyManager_SimCarrierId => _GetTelephonyManager()._ExtCall<int>("getSimCarrierId");
        public static string TelephonyManager_SimCarrierIdName => _GetTelephonyManager()._ExtCall<AndroidJavaObject>("getSimCarrierIdName")._ExtCall<string>("toString");
        public static string TelephonyManager_SimCountryIso => _GetTelephonyManager()._ExtCall<string>("getSimCountryIso");

        public static string TelephonyManager_SimOperator => _GetTelephonyManager()._ExtCall<string>("getSimOperator");
        public static string TelephonyManager_SimOperatorName => _GetTelephonyManager()._ExtCall<string>("getSimOperatorName");
        //android.Manifest.permission.READ_PRIVILEGED_PHONE_STATE
        public static string TelephonyManager_SimSerialNumber => _GetTelephonyManager()._ExtCall<string>("getSimSerialNumber");
        public static int TelephonyManager_SimSpecificCarrierId => _GetTelephonyManager()._ExtCall<int>("getSimSpecificCarrierId");
        public static string TelephonyManager_SimSpecificCarrierIdName => _GetTelephonyManager()._ExtCall<string>("getSimSpecificCarrierIdName");
        public static int TelephonyManager_SimState => _GetTelephonyManager()._ExtCall<int>("getSimState");

        public static string TelephonyManager_NetworkCountryIso => _GetTelephonyManager()._ExtCall<string>("getNetworkCountryIso");
        public static string TelephonyManager_NetworkOperator => _GetTelephonyManager()._ExtCall<string>("getNetworkOperator");
        public static string TelephonyManager_NetworkOperatorName => _GetTelephonyManager()._ExtCall<string>("getNetworkOperatorName");
        //android.Manifest.permission.READ_PRIVILEGED_PHONE_STATE
        public static int TelephonyManager_NetworkType => _GetTelephonyManager()._ExtCall<int>("getNetworkType");

        #endregion


        #region ActivityManager
        private static AndroidJavaObject _ActivityManager;
        private const string ACTIVITY_SERVICE = "activity"; //android.content.Context.ACTIVITY_SERVICE
        private static AndroidJavaObject _GetActivityManager()
        {
            try
            {
                if (_ActivityManager == null)
                {
                    AndroidJavaObject currentActivity = _GetCurrentActivity();
                    _ActivityManager = currentActivity.Call<AndroidJavaObject>("getSystemService", ACTIVITY_SERVICE);
                }
                return _ActivityManager;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }
        #endregion


        #region  MemoryInfo
        private static AndroidJavaObject _MemoryInfo;
        private static AndroidJavaObject _GetMemoryInfo()
        {
            try
            {
                if (_MemoryInfo == null)
                {
                    _MemoryInfo = new AndroidJavaObject("android.app.ActivityManager$MemoryInfo");
                    var activeMgr = _GetActivityManager();
                    activeMgr.Call("getMemoryInfo", _MemoryInfo);
                }
                return _MemoryInfo;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        public static long MemoryInfo_TotalMem => _GetMemoryInfo()._ExtGet<long>("totalMem");
        public static long MemoryInfo_AvailMem => _GetMemoryInfo()._ExtGet<long>("availMem");
        #endregion


        //https://developer.android.google.cn/reference/android/content/pm/PackageManager
        //https://developer.android.google.cn/reference/android/content/pm/PackageManager.PackageInfoFlags
        //https://developer.android.google.cn/reference/android/content/pm/PackageInfo
        #region PackageManager
        private static AndroidJavaObject _PackageManager;
        private static AndroidJavaObject _GetPackageManager()
        {
            try
            {
                if (_PackageManager == null)
                {
                    AndroidJavaObject currentActivity = _GetCurrentActivity();
                    _PackageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
                }
                return _PackageManager;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        //ref https://developer.android.google.cn/reference/android/content/pm/PackageManager.PackageInfoFlags
        public enum EPackageInfoFlag
        {
            GET_SIGNATURES = 64,
            GET_META_DATA = 128,
            GET_PERMISSIONS = 4096,
        }
        public static AndroidJavaObject GetPackageInfo(string package_name, EPackageInfoFlag flag)
        {
            return _GetPackageManager()._ExtCall<AndroidJavaObject, string, int>("getPackageInfo", UnityEngine.Application.identifier, (int)flag);
        }

        public static AndroidJavaObject GetSelfPackageInfo(EPackageInfoFlag flag)
        {
            return GetPackageInfo(UnityEngine.Application.identifier, flag);
        }

        public static string[] GetSelfPackageInfoPermissions()
        {
            string[] signatures = GetSelfPackageInfo(EPackageInfoFlag.GET_PERMISSIONS)._ExtGet<string[]>("requestedPermissions");
            if (signatures == null || signatures.Length == 0)
                return System.Array.Empty<string>();
            return signatures;
        }

        public static bool IsPackageInstalled(string package_name)
        {
            try
            {
                var info = GetPackageInfo(package_name, EPackageInfoFlag.GET_META_DATA);
                return info != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        public sealed class ApkSignature
        {
            public readonly byte[] Data;
            public ApkSignature(byte[] data)
            {
                Data = data;
            }

            public string ToMd5()
            {
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] result = md5.ComputeHash(Data);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                {
                    sb.AppendFormat("{0:X2}", result[i]);
                }
                return sb.ToString();
            }

            public string ToSha1()
            {
                System.Security.Cryptography.SHA1 sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                byte[] result = sha1.ComputeHash(Data);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                {
                    sb.AppendFormat("{0:X2}", result[i]);
                }
                return sb.ToString();
            }

            public string ToSha256()
            {
                System.Security.Cryptography.SHA256 sha256 = new System.Security.Cryptography.SHA256CryptoServiceProvider();
                byte[] result = sha256.ComputeHash(Data);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < result.Length; i++)
                {
                    sb.AppendFormat("{0:X2}", result[i]);
                }
                return sb.ToString();
            }
        }

        public static ApkSignature GetSelfPackageInfoSignature()
        {
            AndroidJavaObject[] signatures = GetSelfPackageInfo(EPackageInfoFlag.GET_SIGNATURES)._ExtGet<AndroidJavaObject[]>("signatures");
            if (signatures == null || signatures.Length == 0)
                return null;
            byte[] bytes = signatures[0].Call<byte[]>("toByteArray");
            return new ApkSignature(bytes);
        }

        public static string ApkSignatureMd5
        {
            get
            {
                var signature = GetSelfPackageInfoSignature();
                if (signature == null)
                    return null;
                return signature.ToMd5();
            }
        }

        public static string ApkSignatureSh1
        {
            get
            {
                var signature = GetSelfPackageInfoSignature();
                if (signature == null)
                    return null;
                return signature.ToSha1();
            }
        }

        public static string ApkSignatureSh256
        {
            get
            {
                var signature = GetSelfPackageInfoSignature();
                if (signature == null)
                    return null;
                return signature.ToSha256();
            }
        }
        #endregion


        //https://developer.android.google.cn/reference/android/os/StatFs
        //https://developer.android.google.cn/reference/android/os/Environment
        #region Storage StateFS
        private static AndroidJavaObject _DataStorage; //android.os.StatFs
        private static AndroidJavaObject _ExternalStorage; //android.os.StatFs
        private static bool _ExternalStorageInited = false;
        private static AndroidJavaObject _GetDataStorage()
        {
            try
            {
                if (_DataStorage == null)
                {
                    var env_class = new AndroidJavaClass("android.os.Environment");
                    var data_dir = env_class.CallStatic<AndroidJavaObject>("getDataDirectory");
                    if (data_dir == null) return default;

                    string path = data_dir.Call<string>("getPath");

                    _DataStorage = new AndroidJavaObject("android.os.StatFs", path);
                }
                return _DataStorage;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        private static AndroidJavaObject _GetExternalStorage()
        {
            if (_ExternalStorageInited)
                return null;
            _ExternalStorageInited = true;
            try
            {
                if (_ExternalStorage == null)
                {
                    var env_class = new AndroidJavaClass("android.os.Environment");
                    var data_dir = env_class.CallStatic<AndroidJavaObject>("getExternalStorageDirectory");
                    string path = data_dir.Call<string>("getPath");
                    _ExternalStorage = new AndroidJavaObject("android.os.StatFs", path);
                }
                return _ExternalStorage;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }
 

        private static long _GetStorageAvailableBytes(string path)
        {
            try
            {
                return _GetStorageAvailableBytes(new AndroidJavaObject("android.os.StatFs", path));
            }
            catch (Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
            }
            return 0;
        }

        private static long _GetStorageAvailableBytes(AndroidJavaObject target)
        {
            if (target == null)
                return 0;

            try
            {
                int sdkVersion = BuildVersion_SDK_INT;

                // Android 8.0 (API 26) 及以上：直接获取可用字节数
                if (sdkVersion >= 26)
                    return target.Call<long>("getAvailableBytes");


                // 兼容旧版本（API 18 ~ 25）
                long blockSize;
                long availableBlocks;

                if (sdkVersion >= 18)
                {
                    blockSize = target.Call<long>("getBlockSizeLong");
                    availableBlocks = target.Call<long>("getAvailableBlocksLong");
                }
                else
                {
                    // 非常老的设备（API < 18），使用 int（最大约 2GB）
                    blockSize = target.Call<int>("getBlockSize");
                    availableBlocks = target.Call<int>("getAvailableBlocks");
                }
                return blockSize * availableBlocks;

            }
            catch (Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return 0;
            }
        }

        private static long _GetStorageTotalBytes(string path)
        {            
            try
            {
                return _GetStorageTotalBytes(new AndroidJavaObject("android.os.StatFs", path));
            }
            catch (Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
            }
            return 0;
        }
        private static long _GetStorageTotalBytes(AndroidJavaObject target)
        {
            if (target == null)
                return 0;

            try
            {
                int sdkVersion = BuildVersion_SDK_INT;

                // Android 8.0 (API 26) 及以
                if (sdkVersion >= 26)
                    return target.Call<long>("getTotalBytes");


                // 兼容旧版本（API 18 ~ 25）
                long blockSize;
                long totalBlocks;

                if (sdkVersion >= 18)
                {
                    blockSize = target.Call<long>("getBlockSizeLong");
                    totalBlocks = target.Call<long>("getBlockCountLong");
                }
                else
                {
                    // 非常老的设备（API < 18），使用 int（最大约 2GB）
                    blockSize = target.Call<int>("getBlockSize");
                    totalBlocks = target.Call<int>("getBlockCount");
                }
                return blockSize * totalBlocks;

            }
            catch (Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return 0;
            }
        }

        private static long? _Storage_TotalBytes = null;
        public static long Storage_AvailableBytes => _GetStorageAvailableBytes(_GetDataStorage());
        public static long Storage_TotalBytes
        {
            get
            {
                if (_Storage_TotalBytes == null)
                    _Storage_TotalBytes = _GetStorageTotalBytes(_GetDataStorage());
                return _Storage_TotalBytes.Value;
            }
        }

        private static long? _ExternalStorage_TotalBytes = null;
        public static bool ExternalStorage_Exist => _GetExternalStorage() != null;
        public static long ExternalStorage_AvailableBytes => _GetStorageAvailableBytes(_GetExternalStorage());
        public static long ExternalStorage_TotalBytes
        {
            get
            {
                if (_ExternalStorage_TotalBytes == null)
                    _ExternalStorage_TotalBytes = _GetStorageTotalBytes(_GetExternalStorage());
                return _ExternalStorage_TotalBytes.Value;
            }
        }

        private static long? _Persistent_TotalBytes = null;
        public static long Persistent_AvailableBytes => _GetStorageAvailableBytes(Application.persistentDataPath);
        public static long Persistent_TotalBytes
        {
            get
            {
                if (_Persistent_TotalBytes == null)
                    _Persistent_TotalBytes = _GetStorageTotalBytes(Application.persistentDataPath);
                return _Persistent_TotalBytes.Value;
            }
        }
        #endregion

        //https://developer.android.google.cn/reference/android/os/Build
        #region Build
        private static AndroidJavaClass _Build;
        private static AndroidJavaClass _GetBuild()
        {
            try
            {
                if (_Build == null)
                {
                    _Build = new AndroidJavaClass("android.os.Build");
                }
                return _Build;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        public static string Build_BOARD => _GetBuild()._ExtGetStatic<string>("BOARD");
        public static string Build_BOOTLOADER => _GetBuild()._ExtGetStatic<string>("BOOTLOADER");
        public static string Build_BRAND => _GetBuild()._ExtGetStatic<string>("BRAND");
        public static string Build_CPU_ABI => _GetBuild()._ExtGetStatic<string>("CPU_ABI");
        public static string Build_CPU_ABI2 => _GetBuild()._ExtGetStatic<string>("CPU_ABI2");
        public static string Build_DEVICE => _GetBuild()._ExtGetStatic<string>("DEVICE");
        public static string Build_DISPLAY => _GetBuild()._ExtGetStatic<string>("DISPLAY");
        public static string Build_FINGERPRINT => _GetBuild()._ExtGetStatic<string>("FINGERPRINT");
        public static string Build_HARDWARE => _GetBuild()._ExtGetStatic<string>("HARDWARE");
        public static string Build_HOST => _GetBuild()._ExtGetStatic<string>("HOST");
        public static string Build_ID => _GetBuild()._ExtGetStatic<string>("ID");
        public static string Build_MANUFACTURER => _GetBuild()._ExtGetStatic<string>("MANUFACTURER");
        public static string Build_MODEL => _GetBuild()._ExtGetStatic<string>("MODEL");
        public static string Build_ODM_SKU => _GetBuild()._ExtGetStatic<string>("ODM_SKU");
        public static string Build_PRODUCT => _GetBuild()._ExtGetStatic<string>("PRODUCT");
        public static string Build_SKU => _GetBuild()._ExtGetStatic<string>("SKU");
        public static string Build_SOC_MANUFACTURER => _GetBuild()._ExtGetStatic<string>("SOC_MANUFACTURER");
        public static string Build_SOC_MODEL => _GetBuild()._ExtGetStatic<string>("SOC_MODEL");
        public static string[] Build_SUPPORTED_ABIS => _GetBuild()._ExtGetStatic<string[]>("SUPPORTED_ABIS");
        public static string Build_TAGS => _GetBuild()._ExtGetStatic<string>("TAGS");
        public static long Build_TIME => _GetBuild()._ExtGetStatic<long>("TIME");
        public static string Build_TYPE => _GetBuild()._ExtGetStatic<string>("TYPE");

        #endregion


        //https://developer.android.google.cn/reference/android/os/Build.VERSION
        #region BuildVersion
        private static AndroidJavaClass _BuildVersion;
        private static AndroidJavaClass _GetBuildVersion()
        {
            try
            {
                if (_BuildVersion == null)
                {
                    _BuildVersion = new AndroidJavaClass("android.os.Build$VERSION");
                }
                return _BuildVersion;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        private static int? _BuildVersion_SDK_INT = null;
        public static int BuildVersion_SDK_INT
        {
            get
            {
                if (_BuildVersion_SDK_INT == null)
                    _BuildVersion_SDK_INT = _GetBuildVersion()._ExtGetStatic<int>("SDK_INT");
                return _BuildVersion_SDK_INT.Value;
            }
        }
        public static string BuildVersion_Release => _GetBuildVersion()._ExtGetStatic<string>("RELEASE");
        #endregion



        //https://developer.android.google.cn/reference/android/view/WindowManager
        //https://developer.android.google.cn/reference/android/view/Display
        //https://developer.android.google.cn/reference/android/util/DisplayMetrics
        #region WindowManager
        //private static AndroidJavaObject _WindowManager;
        //private static AndroidJavaObject _DefaultDisplay;
        private static AndroidJavaObject _DefaultRealMetrics;

        private static AndroidJavaObject _GetDefaultRealMetrics()
        {
            try
            {
                if (_DefaultRealMetrics == null)
                {
                    AndroidJavaObject currentActivity = _GetCurrentActivity();
                    AndroidJavaObject windowManager = currentActivity.Call<AndroidJavaObject>("getSystemService", "window");
                    AndroidJavaObject defaultDisplay = windowManager.Call<AndroidJavaObject>("getDefaultDisplay");
                    _DefaultRealMetrics = new AndroidJavaObject("android.util.DisplayMetrics");
                    defaultDisplay.Call("getRealMetrics", _DefaultRealMetrics);
                }
                return _DefaultRealMetrics;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        public static float Screen_Density => _GetDefaultRealMetrics()._ExtGet<float>("density");
        public static int Screen_DensityDpi => _GetDefaultRealMetrics()._ExtGet<int>("densityDpi");
        public static int Screen_WidthPixels => _GetDefaultRealMetrics()._ExtGet<int>("widthPixels");
        public static int Screen_HeightPixels => _GetDefaultRealMetrics()._ExtGet<int>("heightPixels");
        #endregion



        #region AndroidDeviceInfo
        private static AndroidJavaClass _AndroidDeviceInfo;
        private static bool _AndroidDeviceInfoInited = false;
        private static AndroidJavaClass _GetAndroidDeviceInfo()
        {
            if (_AndroidDeviceInfoInited)
                return _AndroidDeviceInfo;

            _AndroidDeviceInfoInited = true;
            try
            {
                _AndroidDeviceInfo = new AndroidJavaClass("com.fancyhub.DeviceInfo");
                return _AndroidDeviceInfo;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }


        public static bool AdvertisingIdReady
        {
            get
            {
                var info = _GetAndroidDeviceInfo();
                return info._ExtCallStatic<bool>("IsAdvertisingIdReady");
            }
        }

        public static string AdvertisingId
        {
            get
            {
                var info = _GetAndroidDeviceInfo();
                return info._ExtCallStatic<string>("GetAdvertisingId");
            }
        }
        #endregion


        //adb shell getprop
        #region  SystemProperties
        private static AndroidJavaClass _SystemProperties;
        private static AndroidJavaClass _GetSystemProperties()
        {
            try
            {
                if (_SystemProperties == null)
                {
                    _SystemProperties = new AndroidJavaClass("android.os.SystemProperties");
                }
                return _SystemProperties;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        public static string SystemProperties_GetString(string key)
        {
            try
            {
                var obj = _GetSystemProperties();
                return obj.CallStatic<string>("get", key);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        public static string SystemProperties_GetString(string key, string def)
        {
            try
            {
                var obj = _GetSystemProperties();
                return obj.CallStatic<string>("get", key, def);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        public static int SystemProperties_GetInt(string key, int def = 0)
        {
            try
            {
                var obj = _GetSystemProperties();
                return obj.CallStatic<int>("getInt", key, def);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        public static long SystemProperties_GetLong(string key, long def = 0)
        {
            try
            {
                var obj = _GetSystemProperties();
                return obj.CallStatic<long>("getLong", key, def);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        public static bool SystemProperties_GetBool(string key, bool def = false)
        {
            try
            {
                var obj = _GetSystemProperties();
                return obj.CallStatic<bool>("getBoolean", key, def);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }

        public static string GetAllSystemProperties()
        {

            try
            {
                return _GetAndroidDeviceInfo().CallStatic<string>("ExeCmd", "/system/bin/getprop");
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }

        }
        #endregion
    }
}