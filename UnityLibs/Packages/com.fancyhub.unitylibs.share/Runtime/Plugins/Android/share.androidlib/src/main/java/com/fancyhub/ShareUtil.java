/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/2/27
 * Title   :
 * Desc    :
 *************************************************************************************/

package com.fancyhub;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.ContentValues;
import android.content.Intent;
import android.content.IntentFilter;
import android.net.Uri;
import android.os.Build;
import android.util.Log;
import java.io.File;
import androidx.core.content.FileProvider;
import com.unity3d.player.UnityPlayer;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.InputStream;
import java.io.OutputStream;
import android.content.pm.PackageManager;
import android.os.Environment;
import android.provider.MediaStore;

public class ShareUtil {
    private static final String TAG = "FHShare";

    private static Activity.ScreenCaptureCallback sScreenshotCallBack;
    
    // 注册截图广播监听
    public static void RegisterScreenshotReceiver(IScreenshotCallBack callBack) {
        if(Build.VERSION.SDK_INT < 34)
            return;

        UnregisterReceiver();

        if(callBack==null)
            return;

        Activity activity = UnityPlayer.currentActivity;
        if(activity==null)
            return;

        sScreenshotCallBack = new Activity.ScreenCaptureCallback () {
            @Override
            public void onScreenCaptured() {
                Log.d(TAG, "检测到系统截图事件");
                callBack.OnScreenshot();
            }
        };
        activity.registerScreenCaptureCallback(activity.getMainExecutor(), sScreenshotCallBack);
    }

    public static void UnregisterReceiver() {
        if(Build.VERSION.SDK_INT < 34)
            return;

        Activity activity = UnityPlayer.currentActivity;
        if(activity==null)
            return;

        if(sScreenshotCallBack!=null)
        {
            Activity.ScreenCaptureCallback temp = sScreenshotCallBack;
            sScreenshotCallBack=null;
            activity.unregisterScreenCaptureCallback(temp);            
        }
    }

    public static void ShareImage(String title, String imagePath) {
        Activity activity = UnityPlayer.currentActivity;
        if(activity==null)
            return;


        File imageFile = new File(imagePath);
        if (!imageFile.exists()) {
            Log.e(TAG, "截图文件不存在：" + imagePath);
            return;
        }

        // 获取文件Uri（Android 7.0+必须使用FileProvider）
        Uri imageUri = FileProvider.getUriForFile(
            activity,
            activity.getPackageName() + ".fileprovider",
            imageFile
        );

        // 构建分享Intent
        Intent shareIntent = new Intent(Intent.ACTION_SEND);
        shareIntent.setType("image/png");
        shareIntent.putExtra(Intent.EXTRA_STREAM, imageUri);
        // 授予临时访问权限
        shareIntent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
        // 启动分享选择器
        Intent chooser = Intent.createChooser(shareIntent, title);
        chooser.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
        activity.startActivity(chooser);
    }

    private static final int SaveImageError_OK=0;
    private static final int SaveImageError_Permission=1;
    private static final int SaveImageError_Unkown=2;

    public static int CopyImage2Gallery(String srcFilePath, String destFileName)
    {
        Activity activity = UnityPlayer.currentActivity;
        if (activity == null) {
            return SaveImageError_Unkown;
        }

        File srcFile = new File(srcFilePath);
        if (!srcFile.exists()) {
            return SaveImageError_Unkown;
        }

         // 判断 Android 版本
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            // --- Android 10+ (API 29+) : 使用 MediaStore (免权限) ---
            return saveViaMediaStore(activity, srcFilePath, destFileName);
        } else {
            // --- Android 9 及以下 (API 28-) : 需要 WRITE_EXTERNAL_STORAGE 权限 ---
            if (hasWritePermission(activity)) {
                return saveViaLegacyFileCopy(activity, srcFilePath, destFileName);
            } else {
                return SaveImageError_Permission; //权限不足
            }
        }
    }

    private static int saveViaMediaStore(Activity activity, String sourcePath, String fileName) {
        try {
            Context context = activity.getApplicationContext();
            ContentValues values = new ContentValues();
            values.put(MediaStore.Images.Media.DISPLAY_NAME, fileName);
            values.put(MediaStore.Images.Media.MIME_TYPE, "image/*");
            // 关键：指定相对路径，系统会自动创建文件夹
            values.put(MediaStore.Images.Media.RELATIVE_PATH, Environment.DIRECTORY_PICTURES + "/MyUnityGame"); 
            values.put(MediaStore.Images.Media.IS_PENDING, 1); // 标记为正在写入

            Uri uri = context.getContentResolver().insert(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, values);
            
            if (uri != null) {
                OutputStream outputStream = context.getContentResolver().openOutputStream(uri);
                if (outputStream != null) {
                    InputStream inputStream = new FileInputStream(sourcePath);
                    byte[] buffer = new byte[1024];
                    int len;
                    while ((len = inputStream.read(buffer)) > 0) {
                        outputStream.write(buffer, 0, len);
                    }
                    inputStream.close();
                    outputStream.close();

                    // 写入完成，更新状态
                    values.clear();
                    values.put(MediaStore.Images.Media.IS_PENDING, 0);
                    context.getContentResolver().update(uri, values, null, null);
                    return SaveImageError_OK;

                    // sendResultToUnity("Success: Saved to Pictures/MyUnityGame/" + fileName);
                } else {
                    // sendResultToUnity("Error: Failed to open output stream");
                    return SaveImageError_Unkown;
                }
            } else {
                // sendResultToUnity("Error: Failed to create MediaStore record");
                return SaveImageError_Unkown;
            }
        } catch (Exception e) {
            Log.e(TAG, "MediaStore Error", e);
            // sendResultToUnity("Error: " + e.getMessage());
            return SaveImageError_Unkown;
        }
    }

    private static boolean hasWritePermission(Activity activity) {
        return activity.checkSelfPermission(android.Manifest.permission.WRITE_EXTERNAL_STORAGE)== PackageManager.PERMISSION_GRANTED;
    }

    private static int saveViaLegacyFileCopy(Activity activity, String sourcePath, String fileName) {
        try {
            // 目标目录：/sdcard/Pictures/MyUnityGame/
            File picturesDir = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_PICTURES);
            File albumDir = new File(picturesDir, "MyUnityGame");
            if (!albumDir.exists()) {
                albumDir.mkdirs();
            }

            File destFile = new File(albumDir, fileName);
            
            // 执行复制
            copyFile(new File(sourcePath), destFile);

            // 通知媒体库刷新 (Android 9- 必须手动扫描)
            scanFileLegacy(activity, destFile.getAbsolutePath());

            return SaveImageError_OK;
            // sendResultToUnity("Success: Saved to " + destFile.getAbsolutePath());
        } catch (Exception e) {
            Log.e(TAG, "Legacy Copy Error", e);
            return SaveImageError_Unkown;
            // sendResultToUnity("Error: " + e.getMessage());
        }
    }

    private static void copyFile(File src, File dst) throws Exception {
        InputStream in = new FileInputStream(src);
        OutputStream out = new FileOutputStream(dst);
        byte[] buf = new byte[4096];
        int len;
        while ((len = in.read(buf)) > 0) {
            out.write(buf, 0, len);
        }
        in.close();
        out.close();
    }

    private static void scanFileLegacy(Context context, String filePath) {
        android.media.MediaScannerConnection.scanFile(
            context,
            new String[]{filePath},
            new String[]{"image/*"},
            null
        );
    }
}