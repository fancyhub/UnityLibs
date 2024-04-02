package com.fancyhub;

import android.app.Activity;
import com.google.common.util.concurrent.ListenableFuture;
import android.util.Log;

public class DeviceInfo {

    static class AdvertisingIdResult {
        public String AdvertisingId;
        public String ProviderPackageName;
    }

    //0: Not Request, 1: Requesting 2: RequestDone
    private static int _AdvertisingIdStatus = 0;
    private static AdvertisingIdResult _AdvertisingIdResult = null;

    private final static String TAG = "AndroidDeviceInfo";

    public static String GetAdvertisingId() {
        _ReqAdvertisingId();
        if (_AdvertisingIdResult == null)
            return null;
        return _AdvertisingIdResult.AdvertisingId;
    }

    public static boolean IsAdvertisingIdReady() {
        _ReqAdvertisingId();
        return _AdvertisingIdStatus == 2;
    }

    private static void _ReqAdvertisingId() {
        //1. 检查状态
        if (_AdvertisingIdStatus != 0)
            return;
        _AdvertisingIdStatus = 1;
        Log.i(TAG, "request advertising id");

        //2. 获取Context
        Activity activity = com.unity3d.player.UnityPlayer.currentActivity;
        if (activity == null) {
            Log.e(TAG, "Can't find com.unity3d.player.UnityPlayer.currentActivity");
            _AdvertisingIdStatus = 2;
            return;
        }

        //3. 先获取Google的
        /** build.gradle
        dependencies {
        implementation("com.google.android.gms:play-services-ads-identifier:18.0.1")
        }
         */
        try {
            AdvertisingIdResult result = new AdvertisingIdResult();
            com.google.android.gms.ads.identifier.AdvertisingIdClient.Info info = com.google.android.gms.ads.identifier.AdvertisingIdClient.getAdvertisingIdInfo(activity);
            result.AdvertisingId = info.getId();
            if (!info.isLimitAdTrackingEnabled()) {
                _AdvertisingIdResult = result;
                _AdvertisingIdStatus = 2;
                Log.i(TAG, "got google advertising id");
                return;
            }
        } catch (Exception e) {
            Log.e(TAG, "Error", e);
        }

        //4. 获取其他Provider,
        /**build.gradle
        dependencies {
        implementation 'androidx.ads:ads-identifier:1.0.0-alpha04'
        implementation 'com.google.guava:guava:28.0-android'
        }
         */
        try {
            if (!androidx.ads.identifier.AdvertisingIdClient.isAdvertisingIdProviderAvailable(activity)) {
                Log.i(TAG, "androidx.ads.identifier.AdvertisingIdClient.isAdvertisingIdProviderAvailable");
                _AdvertisingIdStatus = 2;
                return;
            }

            final ListenableFuture < androidx.ads.identifier.AdvertisingIdInfo > listenableFuture = androidx.ads.identifier.AdvertisingIdClient.getAdvertisingIdInfo(activity);
            listenableFuture.addListener(() -> {
               try {
                   androidx.ads.identifier.AdvertisingIdInfo advertisingIdInfo = listenableFuture.get();

                   AdvertisingIdResult result = new AdvertisingIdResult();
                   result.AdvertisingId = advertisingIdInfo.getId();
                   result.ProviderPackageName = advertisingIdInfo.getProviderPackageName();
                   if (!advertisingIdInfo.isLimitAdTrackingEnabled()) {
                       _AdvertisingIdResult = result;
                   }
               } catch (Exception e) {
                   Log.e(TAG, "Error", e);
               } finally {
                   _AdvertisingIdStatus = 2;
               }
           }, androidx.core.content.ContextCompat.getMainExecutor(activity));
        } catch (Exception e) {
            Log.e(TAG, "Error", e);
            _AdvertisingIdStatus = 2;
        }
    }
}
