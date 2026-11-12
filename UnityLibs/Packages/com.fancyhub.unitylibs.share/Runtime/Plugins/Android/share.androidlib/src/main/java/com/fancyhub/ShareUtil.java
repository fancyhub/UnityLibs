/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/2/27
 * Title   :
 * Desc    :
 *************************************************************************************/

package com.fancyhub;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.ClipData;
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
import android.os.Environment;
import android.provider.MediaStore;
import android.widget.Toast;
import com.fancyhub.share.R;

public class ShareUtil {
    private static final String TAG = "FHShare";

    // 指定 targetPackageId 时：true=直接打开目标 App，false=继续显示系统 chooser。
    private static final boolean USE_DIRECT_TARGET_SHARE = true;

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

    public static void Share(String choserTitle, String contentSubject, String contentText, String contentImageFilePath, String targetAppPackageId) {
        if(targetAppPackageId!=null && targetAppPackageId.length()>0 && USE_DIRECT_TARGET_SHARE)
        {
            ShareToTarget(contentSubject, contentText, contentImageFilePath, targetAppPackageId);
        }
        else
        {
            ShareWithChooser(choserTitle, contentSubject, contentText, contentImageFilePath, targetAppPackageId);
        }
    }

    // 使用系统分享选择器，可选地限制到指定 package。
    private static void ShareWithChooser(String choserTitle, String contentSubject, String contentText, String contentImageFilePath, String targetAppPackageId) {
        Activity activity = UnityPlayer.currentActivity;
        if(activity==null)
            return;

        Intent shareIntent = BuildShareIntent(
            activity,
            contentSubject,
            contentText,
            contentImageFilePath
        );
        if(shareIntent==null)
            return;

        if(targetAppPackageId!=null && targetAppPackageId.length()>0)
        {
            shareIntent.setPackage(targetAppPackageId);

            // API 30+ 可能受 package visibility（<queries>）影响，因此只在 API 29
            // 及以下通过 resolveActivity 判断；找不到时提示，但仍继续启动 chooser。
            if(Build.VERSION.SDK_INT < Build.VERSION_CODES.R &&
               shareIntent.resolveActivity(activity.getPackageManager()) == null)
            {
                Log.w(TAG, "Target app is unavailable for sharing: " + targetAppPackageId);
                Toast.makeText(
                    activity,
                    activity.getString(R.string.fh_share_target_unavailable),
                    Toast.LENGTH_SHORT
                ).show();
            }
        }

        // 使用系统分享选择器
        Intent chooser = Intent.createChooser(shareIntent, choserTitle);
        chooser.addFlags(
            Intent.FLAG_ACTIVITY_NEW_TASK |
            Intent.FLAG_GRANT_READ_URI_PERMISSION
        );
        activity.startActivity(chooser);
    }

    // 直接打开指定目标 App，不显示系统分享选择器。
    private static void ShareToTarget(String contentSubject, String contentText, String contentImageFilePath, String targetAppPackageId) {
        Activity activity = UnityPlayer.currentActivity;
        if(activity==null || targetAppPackageId==null || targetAppPackageId.length()==0)
            return;

        Intent shareIntent = BuildShareIntent(
            activity,
            contentSubject,
            contentText,
            contentImageFilePath
        );
        if(shareIntent==null)
            return;

        shareIntent.setPackage(targetAppPackageId);
        shareIntent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);

        try
        {
            activity.startActivity(shareIntent);
        }
        catch (android.content.ActivityNotFoundException e)
        {
            Log.w(TAG, "Target app is unavailable for sharing: " + targetAppPackageId, e);
            Toast.makeText(
                activity,
                activity.getString(R.string.fh_share_target_unavailable),
                Toast.LENGTH_SHORT
            ).show();
        }
    }

    private static Intent BuildShareIntent(Activity activity, String contentSubject, String contentText, String contentImageFilePath) {
        Intent shareIntent = new Intent(Intent.ACTION_SEND);
        boolean hasContent = false;

        if(contentText!=null && contentText.length()>0)
        {
            hasContent=true;
            shareIntent.setType("text/plain");
            shareIntent.putExtra(Intent.EXTRA_TEXT, contentText);
        }

        if(contentImageFilePath!=null && contentImageFilePath.length()>0)
        {
            File imageFile = new File(contentImageFilePath);
            if(imageFile.exists())
            {
                hasContent=true;
                shareIntent.setType("image/*");

                // Android 7.0+ 通过 FileProvider 分享本地文件。
                Uri imageUri = FileProvider.getUriForFile(
                    activity,
                    activity.getPackageName() + ".fileprovider",
                    imageFile
                );

                shareIntent.putExtra(Intent.EXTRA_STREAM, imageUri);
                shareIntent.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
                shareIntent.setClipData(ClipData.newRawUri("share_image", imageUri));
            }
        }

        if(!hasContent)
            return null;

        if(contentSubject!=null && contentSubject.length()>0)
        {
            shareIntent.putExtra(Intent.EXTRA_SUBJECT, contentSubject);
        }

        return shareIntent;
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
            // --- Android 9 及以下 (API 28-) : 写入应用外部图片目录，避免请求 WRITE_EXTERNAL_STORAGE ---
            return saveViaLegacyFileCopy(activity, srcFilePath, destFileName);
        }
    }

    private static int saveViaMediaStore(Activity activity, String sourcePath, String fileName) {
        try {
            Context context = activity.getApplicationContext();
            ContentValues values = new ContentValues();
            values.put(MediaStore.Images.Media.DISPLAY_NAME, fileName);
            values.put(MediaStore.Images.Media.MIME_TYPE, "image/*");
            // 关键：指定相对路径，系统会自动创建文件夹
            values.put(MediaStore.Images.Media.RELATIVE_PATH, Environment.DIRECTORY_PICTURES + "/"+activity.getPackageName()); 
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
                    
                } else {
                    return SaveImageError_Unkown;
                }
            } else {
                return SaveImageError_Unkown;
            }
        } catch (Exception e) {
            Log.e(TAG, "MediaStore Error", e);
            return SaveImageError_Unkown;
        }
    }

    private static int saveViaLegacyFileCopy(Activity activity, String sourcePath, String fileName) {
        try {
            File picturesDir = activity.getExternalFilesDir(Environment.DIRECTORY_PICTURES);
            if (picturesDir == null) {
                return SaveImageError_Unkown;
            }
            File albumDir = new File(picturesDir, activity.getPackageName());
            if (!albumDir.exists()) {
                albumDir.mkdirs();
            }

            File destFile = new File(albumDir, fileName);
            
            // 执行复制
            copyFile(new File(sourcePath), destFile);

            // 通知媒体库刷新 (Android 9- 必须手动扫描)
            scanFileLegacy(activity, destFile.getAbsolutePath());

            return SaveImageError_OK;
        } catch (Exception e) {
            Log.e(TAG, "Legacy Copy Error", e);
            return SaveImageError_Unkown;
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
