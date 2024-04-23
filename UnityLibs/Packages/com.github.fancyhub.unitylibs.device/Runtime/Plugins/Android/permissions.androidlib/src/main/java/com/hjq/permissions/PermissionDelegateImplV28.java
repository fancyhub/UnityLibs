package com.hjq.permissions;

import android.app.Activity;
import android.content.Context;
import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;

/**
 *    author : Android 轮子哥
 *    github : https://github.com/getActivity/XXPermissions
 *    time   : 2022/07/03
 *    desc   : Android 9.0 权限委托实现
 */
@RequiresApi(api = AndroidVersion.ANDROID_9)
class PermissionDelegateImplV28 extends PermissionDelegateImplV26 {

   @Override
   public boolean isGrantedPermission(@NonNull Context context, @NonNull String permission) {
      if (PermissionUtils.equalsPermission(permission, Permission.ACCEPT_HANDOVER)) {
         return PermissionUtils.checkSelfPermission(context, permission);
      }
      return super.isGrantedPermission(context, permission);
   }

   @Override
   public boolean isDoNotAskAgainPermission(@NonNull Activity activity, @NonNull String permission) {
      if (PermissionUtils.equalsPermission(permission, Permission.ACCEPT_HANDOVER)) {
         return !PermissionUtils.checkSelfPermission(activity, permission) &&
                 !PermissionUtils.shouldShowRequestPermissionRationale(activity, permission);
      }
      return super.isDoNotAskAgainPermission(activity, permission);
   }
}