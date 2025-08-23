package com.fancyhub;

import android.app.Activity;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.core.app.ActivityCompat;

import com.hjq.permissions.OnPermissionInterceptor;
import com.hjq.permissions.OnPermissionCallback;
import com.hjq.permissions.OnPermissionPageCallback;
import com.hjq.permissions.PermissionFragment;
import com.hjq.permissions.XXPermissions;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;

interface IPermissionsShouldShowRationale {
    public void onShouldShowRationale(List<String> shouldShowRationaleList, IAlertDialogResult onUserResult);
}

interface IPermissionsDoNotAskAgain {
    /**
     * Called when you should tell user to allow these permissions in settings.
     *
     * @param doNotAskAgainList Permissions that should allow in settings.
     * @param onUserResult      Call it when the user agrees or refuses to allow
     *                          these permissions in settings.
     */
    public void onDoNotAskAgain(List<String> doNotAskAgainList, IAlertDialogResult onUserResult);
}

class XXPermissionsHelper {
    private final Activity _Activity;
    private final ArrayList<String> _PermissionList;

    private IPermissionInnerResult _onResult;
    private IPermissionsShouldShowRationale _onShouldShowRationale;
    private IPermissionsDoNotAskAgain _onDoNotAskAgain;

    public XXPermissionsHelper(Activity activity) {
        _PermissionList = new ArrayList<>();
        _Activity = activity;
    }

    public static XXPermissionsHelper with(Activity activity) {
        return new XXPermissionsHelper(activity);
    }

    public XXPermissionsHelper permissions(String... permissions) {
        _PermissionList.addAll(Arrays.asList(permissions));
        return this;
    }

    public XXPermissionsHelper onShouldShowRationale(IPermissionsShouldShowRationale onShouldShowRationale) {
        this._onShouldShowRationale = onShouldShowRationale;
        return this;
    }

    public XXPermissionsHelper onDoNotAskAgain(IPermissionsDoNotAskAgain onDoNotAskAgain) {
        this._onDoNotAskAgain = onDoNotAskAgain;
        return this;
    }

    public XXPermissionsHelper onResult(IPermissionInnerResult onResult) {
        this._onResult = onResult;
        return this;
    }

    public void request() {
        XXPermissions.with(_Activity)
                .permission(_PermissionList)
                .interceptor(new PermissionInterceptor())
                .request(new PermissionCallback());
    }

    private class PermissionInterceptor implements OnPermissionInterceptor {

        // @Suppress("TooGenericExceptionCaught", "SwallowedException")
        @Override
        public void launchPermissionRequest(@NonNull Activity activity, @NonNull List<String> allList,
                OnPermissionCallback callback) {
            if (_onShouldShowRationale == null) {
                _SuperLaunchPermissionRequest(activity, allList, callback);
                return;
            }

            List<String> rationalePermissions = new ArrayList<>();
            for (String p : allList) {
                try {
                    if (XXPermissions.isGranted(activity, p))
                        continue;

                    if (ActivityCompat.shouldShowRequestPermissionRationale(activity, p)) {
                        rationalePermissions.add(p);
                    }
                } catch (Exception ignored) {
                }
            }

            if (rationalePermissions.isEmpty()) {
                // 第一次请求会进这个
                _SuperLaunchPermissionRequest(activity, allList, callback);
                return;
            }

            _onShouldShowRationale.onShouldShowRationale(rationalePermissions, new IAlertDialogResult() {
                @Override
                public void onResult(boolean isAgree) {
                    if (isAgree) {
                        _SuperLaunchPermissionRequest(activity, allList, callback);
                    } else {
                        List<String> newDenied = XXPermissions.getDenied(activity, allList);
                        if (callback == null)
                            return;
                        callback.onGranted(_ListMinus(allList, newDenied), false);
                        callback.onDenied(newDenied, false);
                    }
                }
            });
        }

        public void grantedPermissionRequest(@NonNull Activity activity, @NonNull List<String> allPermissions,
                @NonNull List<String> grantedPermissions, boolean allGranted,
                @Nullable OnPermissionCallback callback) {
            OnPermissionInterceptor.super.grantedPermissionRequest(activity, allPermissions, grantedPermissions,
                    allGranted, callback);
        }

        @Override
        public void deniedPermissionRequest(@NonNull Activity activity, @NonNull List<String> allList,
                @NonNull List<String> deniedList, boolean doNotAskAgain, OnPermissionCallback callback) {
            if (doNotAskAgain) {
                _showPermissionSettingDialog(activity, allList, deniedList, callback);
            } else {
                List<String> newDenied = XXPermissions.getDenied(activity, allList);
                if (callback == null)
                    return;

                callback.onGranted(_ListMinus(allList, newDenied), false);
                callback.onDenied(deniedList, false);
            }
        }

        public void finishPermissionRequest(@NonNull Activity activity, @NonNull List<String> allPermissions,
                boolean skipRequest, @Nullable OnPermissionCallback callback) {
            OnPermissionInterceptor.super.finishPermissionRequest(activity, allPermissions, skipRequest, callback);
        }

        private void _showPermissionSettingDialog(Activity activity, List<String> allList, List<String> deniedList,
                OnPermissionCallback callback) {
            if (_onDoNotAskAgain == null) {
                OnPermissionInterceptor.super.deniedPermissionRequest(activity, allList, deniedList, true, callback);
                return;
            }

            ArrayList<String> doNotAskAgainList = new ArrayList<>();
            for (String p : deniedList) {
                if (XXPermissions.isDoNotAskAgainPermissions(activity, p)) {
                    doNotAskAgainList.add(p);
                }
            }

            _onDoNotAskAgain.onDoNotAskAgain(doNotAskAgainList, new IAlertDialogResult() {
                @Override
                public void onResult(boolean isAgree) {
                    if (isAgree) {
                        XXPermissions.startPermissionActivity(activity, doNotAskAgainList,
                                new OnPermissionPageCallback() {
                                    @Override
                                    public void onGranted() {
                                        if (callback == null)
                                            return;

                                        callback.onGranted(allList, true);
                                    }

                                    @Override
                                    public void onDenied() {
                                        List<String> newDenied = XXPermissions.getDenied(activity, allList);
                                        if (callback == null)
                                            return;
                                        callback.onGranted(_ListMinus(allList, newDenied), false);
                                        callback.onDenied(newDenied, true);
                                    }
                                });
                    } else {
                        List<String> newDenied = XXPermissions.getDenied(activity, allList);
                        if (callback == null)
                            return;
                        callback.onGranted(_ListMinus(allList, newDenied), false);
                        callback.onDenied(deniedList, true);
                    }
                }
            });

        }

        private void _SuperLaunchPermissionRequest(
                Activity activity,
                List<String> allPermissions,
                OnPermissionCallback callback) {

            PermissionFragment.launch(activity, new ArrayList<>(allPermissions), this, callback);
        }
    }

    private class PermissionCallback implements OnPermissionCallback {

        private List<String> grantedList = null;

        @Override
        public void onGranted(@NonNull List<String> permissions, boolean allGranted) {
            if (allGranted) {
                if (_onResult != null)
                    _onResult.onResult(true, permissions, java.util.Collections.<String>emptyList());
            } else {
                grantedList = permissions;
            }
        }

        @Override
        public void onDenied(@NonNull List<String> permissions, boolean doNotAskAgain) {
            if (_onResult != null)
                _onResult.onResult(false, grantedList == null ? java.util.Collections.<String>emptyList() : grantedList,
                        permissions);
        }
    };

    private static ArrayList<String> _ListMinus(List<String> orig, List<String> sub) {
        ArrayList<String> temp = new ArrayList<>();
        for (String p : orig) {
            if (!sub.contains(p))
                temp.add(p);
        }
        return temp;
    }
}
