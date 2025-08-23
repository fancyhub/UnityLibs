package com.fancyhub;

import android.app.Activity;

import androidx.appcompat.app.AlertDialog;

import com.unity3d.player.UnityPlayer;

public class PermissionUtil {
    public static boolean HasPermission(String permissionName) {

        return com.hjq.permissions.XXPermissions.isGranted(UnityPlayer.currentActivity, permissionName);
    }

    public static void Request(
            IPermissionResult permissionResult,
            String permisionName,
            String title,
            String msg,
            String okBtn,
            String cancelBtn,
            String title2,
            String msg2,
            String okBtn2,
            String cancelBtn2) {
        Activity active = UnityPlayer.currentActivity;
        active.runOnUiThread(() -> {
            _Request(permissionResult,active, permisionName, title, msg, okBtn, cancelBtn, title2, msg2, okBtn2, cancelBtn2);
        });
    }

    private static void _Request(
            IPermissionResult permissionResult,
            Activity activity,
            String permisionName,
            String title,
            String msg,
            String okBtn,
            String cancelBtn,
            String title2,
            String msg2,
            String okBtn2,
            String cancelBtn2) {
        XXPermissionsHelper.with(activity)
                .permissions(permisionName)
                // 申请位置权限
                // .permissions(Permission.ACCESS_FINE_LOCATION,
                // Permission.ACCESS_COARSE_LOCATION)
                // 申请相机权限
                // .permissions(Permission.CAMERA)
                // 申请BLE权限
                // .permissions(Permission.BLUETOOTH_CONNECT)
                // 如果申请权限之前需要向用户展示权限申请理由，则走此回调
                .onShouldShowRationale((shouldShowRationaleList, onUserResult) -> {
                    // 这里的 Dialog 只是示例，没有用 DialogFragment 来处理 Dialog 生命周期
                    new AlertDialog.Builder(activity, androidx.appcompat.R.style.Theme_AppCompat_Dialog_Alert)
                            .setTitle(title)
                            // 根据 rationalePermissions 进行适当的提示
                            .setMessage(msg)
                            .setNegativeButton(cancelBtn, (dialog, which) -> dialog.cancel())
                            .setPositiveButton(okBtn, (dialog, which) -> {
                                dialog.dismiss();
                                // 用户同意，通过此回调通知框架
                                onUserResult.onResult(true);
                            })
                            .setOnCancelListener(dialog -> {
                                // 用户不同意，通过此回调通知框架
                                onUserResult.onResult(false);
                            })
                            .show();
                })
                .onDoNotAskAgain((doNotAskAgainList, onUserResult) -> {
                    // 这里的 Dialog 只是示例，没有用 DialogFragment 来处理 Dialog 生命周期
                    new AlertDialog.Builder(activity, androidx.appcompat.R.style.Theme_AppCompat_Dialog_Alert)
                            .setTitle(title2)
                            // 根据被拒绝的权限列表进行适当的提示
                            .setMessage(msg2)
                            .setNegativeButton(cancelBtn2, (dialog, which) -> dialog.cancel())
                            .setPositiveButton(okBtn2, (dialog, which) -> {
                                dialog.dismiss();
                                // 用户同意，通过此回调通知框架
                                onUserResult.onResult(true);
                            })
                            .setOnCancelListener(dialog -> {
                                // 用户不同意，通过此回调通知框架
                                onUserResult.onResult(false);
                            })
                            .show();
                })
                .onResult((allGranted, grantedList,deniedList)->
                {
                    PermissionResult result = new  PermissionResult();
                    result.allGranted=allGranted;
                    result.grantedList = grantedList;
                    result.deniedList = deniedList;

                    permissionResult.onResult(result.ToJsonString());
                })
                .request();
    }
}
