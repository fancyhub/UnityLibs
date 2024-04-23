/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/2/2
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public static class AndroidPermissionUtil
    {
        public static bool HasPermission(string permission)
        {
            return _GetPermissionUtil().CallStatic<bool>("HasPermission", permission);
        }

        public struct AlertDialog
        {
            public string Title;
            public string Message;
            public string ButtonOK;
            public string ButtonCancel;
        }

        [System.Serializable]
        struct PermissionResult
        {
            public bool allGranted;
            public string[] grantedList;
            public string[] deniedList;
        }

        public delegate void PermissionCallBack(bool allGranted, string[] grantedList, string[] deniedList);


        private class JavaPermissionResult : UnityEngine.AndroidJavaProxy
        {
            private PermissionCallBack _CallBack;
            public JavaPermissionResult(PermissionCallBack callback) : base("com.fancyhub.IPermissionResult")
            {
                _CallBack = callback;
            }

            [UnityEngine.Scripting.Preserve]
            public void onResult(string result)
            {
                 PermissionResult jsonResult =  UnityEngine.JsonUtility.FromJson<PermissionResult>(result);
                _CallBack?.Invoke(jsonResult.allGranted, jsonResult.grantedList, jsonResult.deniedList);
            }

        }
        public static void RequestPermission(string permission, PermissionCallBack callBack, AlertDialog dialog, AlertDialog dialog2)
        {
            _GetPermissionUtil().CallStatic(
                "Request",
                new JavaPermissionResult(callBack),
                permission,
                dialog.Title, dialog.Message, dialog.ButtonOK, dialog.ButtonCancel,
                dialog2.Title, dialog2.Message, dialog2.ButtonOK, dialog2.ButtonCancel);
        }



        private const bool ReturnExcpetion = false;
        private static void _PrintException(System.Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }

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
                _PrintException(ex);
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
                _PrintException(ex);
                return default(T);
            }
        }

        private static T _ExtCallStatic<T>(this AndroidJavaClass self, string name)
        {
            try
            {
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

        private static AndroidJavaClass _PermissionUtil;
        private static AndroidJavaClass _GetPermissionUtil()
        {
            try
            {
                if (_PermissionUtil == null)
                {
                    _PermissionUtil = new AndroidJavaClass("com.fancyhub.PermissionUtil");
                }
                return _PermissionUtil;
            }
            catch (System.Exception ex)
            {
                if (ReturnExcpetion)
                    throw ex;
                _PrintException(ex);
                return default;
            }
        }
    }
}