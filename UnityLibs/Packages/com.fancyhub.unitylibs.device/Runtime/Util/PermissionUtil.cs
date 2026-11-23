using System;
using UnityEngine;

namespace FH
{
    public static class PermissionUtil
    {
        public static void RequestWriteStorage(Action<bool> callBack)
        {
            if (Application.isEditor)
            {
                callBack?.Invoke(false);
                return;
            }

            switch (Application.platform)
            {
                default:
                    callBack?.Invoke(false);
                    break;

                case RuntimePlatform.Android:
                    {
                        string permission = UnityEngine.Android.Permission.ExternalStorageWrite;
                        if (FH.DeviceInfoAndroid.BuildVersion_SDK_INT >= 30)
                        {
                            permission = "android.permission.READ_MEDIA_IMAGES";
                        }

                        Debug.Log($"<Permission> {FH.DeviceInfoAndroid.BuildVersion_SDK_INT} {permission}");

                        if (FH.PermissionUtilAndroid.HasPermission(permission))
                        {
                            callBack?.Invoke(true);
                            return;
                        }

                        var dialog1 = new FH.PermissionUtilAndroid.AlertDialog()
                        {
                            Title = "Title",
                            Message = "Message",
                            ButtonOK = "OK",
                            ButtonCancel = "Cancel",
                        };

                        var dialog2 = new FH.PermissionUtilAndroid.AlertDialog()
                        {
                            Title = "Title",
                            Message = "Message 2",
                            ButtonOK = "OK",
                            ButtonCancel = "Cancel",
                        };

                        FH.PermissionUtilAndroid.RequestPermission(permission, (bool allGranted, string[] grantedList, string[] deniedList) =>
                        {
                            callBack.Invoke(allGranted);
                        }, dialog1, dialog2);

                    }
                    break;
            }
        }
    }
}
