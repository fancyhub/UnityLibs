/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/2/2
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;

namespace FH
{
    public static class AndroidDeviceInfo
    {
        private const bool ReturnExcpetion = false;

        private static T _ExtCall<T>(this AndroidJavaObject self, string name)
        {
            try
            {
                return self.Call<T>(name);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                return default(T);
            }
        }

        private static T _ExtGet<T>(this AndroidJavaObject self, string name)
        {
            try
            {
                return self.Get<T>(name);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                return default(T);
            }
        }

        private static T _ExtGetStatic<T>(this AndroidJavaClass self, string name)
        {
            try
            {
                return self.GetStatic<T>(name);
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
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


        //https://developer.android.google.cn/reference/android/os/StatFs
        //https://developer.android.google.cn/reference/android/os/Environment
        #region Storage StateFS
        private static AndroidJavaObject _DataStorage;
        private static AndroidJavaObject _ExternalStorage;
        private static bool _ExternalStorageInited = false;
        private static AndroidJavaObject _GetDataStorage()
        {
            try
            {
                if (_DataStorage == null)
                {
                    var env_class = new AndroidJavaClass("android.os.Environment");
                    var data_dir = env_class.CallStatic<AndroidJavaObject>("getDataDirectory");
                    string path = data_dir.Call<string>("getPath");

                    _DataStorage = new AndroidJavaObject("android.os.StatFs", path);
                }
                return _DataStorage;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
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
                return default;
            }
        }

        public static long Storage_AvailableBytes => _GetDataStorage()._ExtCall<long>("getAvailableBytes");
        public static long Storage_TotalBytes => _GetDataStorage()._ExtCall<long>("getTotalBytes");
        public static bool ExternalStorage_Exist => _GetExternalStorage() != null;
        public static long ExternalStorage_AvailableBytes => _GetExternalStorage()._ExtCall<long>("getAvailableBytes");
        public static long ExternalStorage_TotalBytes => _GetExternalStorage()._ExtCall<long>("getTotalBytes");
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
                return default;
            }
        }
        public static string Build_BRAND => _GetBuild()._ExtGetStatic<string>("BRAND");
        public static string Build_CPU_ABI => _GetBuild()._ExtGetStatic<string>("CPU_ABI");
        public static string Build_CPU_ABI2 => _GetBuild()._ExtGetStatic<string>("CPU_ABI2");
        public static string Build_DEVICE => _GetBuild()._ExtGetStatic<string>("DEVICE");
        public static string Build_DISPLAY => _GetBuild()._ExtGetStatic<string>("DISPLAY");
        public static string Build_FINGERPRINT => _GetBuild()._ExtGetStatic<string>("FINGERPRINT");
        public static string Build_HARDWARE => _GetBuild()._ExtGetStatic<string>("HARDWARE");
        public static string Build_ID => _GetBuild()._ExtGetStatic<string>("ID");
        public static string Build_MODEL => _GetBuild()._ExtGetStatic<string>("MODEL");
        public static string Build_ODM_SKU => _GetBuild()._ExtGetStatic<string>("ODM_SKU");
        public static string Build_PRODUCT => _GetBuild()._ExtGetStatic<string>("PRODUCT");
        public static string Build_SKU => _GetBuild()._ExtGetStatic<string>("SKU");
        public static string Build_SOC_MODEL => _GetBuild()._ExtGetStatic<string>("SOC_MODEL");

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
                return default;
            }
        }

        public static float Screen_Density => _GetDefaultRealMetrics()._ExtGet<float>("density");
        public static int Screen_DensityDpi => _GetDefaultRealMetrics()._ExtGet<int>("densityDpi");
        public static int Screen_WidthPixels => _GetDefaultRealMetrics()._ExtGet<int>("widthPixels");
        public static int Screen_HeightPixels => _GetDefaultRealMetrics()._ExtGet<int>("heightPixels");
        #endregion
    }
}