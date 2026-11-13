/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/2/27
 * Title   :
 * Desc    :
 *************************************************************************************/

package com.fancyhub;

import android.app.Activity;
import android.app.Dialog;
import android.content.res.Configuration;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.graphics.drawable.GradientDrawable;
import android.text.TextUtils;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;
import android.widget.HorizontalScrollView;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.content.ComponentName;
import android.content.pm.PackageManager;
import android.content.pm.ResolveInfo;
import java.util.List;
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
import com.fancyhub.share.BuildConfig;

public class ShareUtil {
    private static final String TAG = "FHShare";

    // 与 Manifest 查询声明使用同一份 Gradle 配置，构建时生成。
    private static final String[] TARGET_QUERY_PACKAGES =
        BuildConfig.FH_SHARE_TARGET_PACKAGES.split(",");

    private static boolean SupportsTargetQuery(String packageId) {
        if(packageId==null)
            return false;
        for(String enabledPackage : TARGET_QUERY_PACKAGES)
        {
            if(!enabledPackage.isEmpty() && packageId.equals(enabledPackage))
                return true;
        }
        return false;
    }

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
        if(SupportsTargetQuery(targetAppPackageId))
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

    // 查询目标 App 的分享入口，每次自行显示选择框，不保存上次选择。
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

        // 未适配的包名不依赖查询结果，交给系统分享面板处理。
        if(!SupportsTargetQuery(targetAppPackageId))
        {
            ShareWithChooser(null, contentSubject, contentText, contentImageFilePath, targetAppPackageId);
            return;
        }

        shareIntent.setPackage(targetAppPackageId);
        activity.runOnUiThread(() -> {
            if(activity.isFinishing() || activity.isDestroyed())
                return;

            PackageManager pm = activity.getPackageManager();
            List<ResolveInfo> targets = pm.queryIntentActivities(
                shareIntent, PackageManager.MATCH_DEFAULT_ONLY);
            if(targets.isEmpty())
            {
                Toast.makeText(activity, R.string.fh_share_target_unavailable,
                    Toast.LENGTH_SHORT).show();
                return;
            }

            if(targets.size() == 1)
            {
                StartTargetActivity(activity, shareIntent, targets.get(0));
                return;
            }

            ShowTargetPicker(activity, shareIntent, targets);
        });
    }

    // 底部横向分享面板：Activity 图标在上、名称在下，入口较多时横向滚动。
    private static void ShowTargetPicker(Activity activity, Intent shareIntent, List<ResolveInfo> targets) {
        boolean dark = (activity.getResources().getConfiguration().uiMode &
            Configuration.UI_MODE_NIGHT_MASK) == Configuration.UI_MODE_NIGHT_YES;
        Dialog dialog = new Dialog(activity, R.style.FHSharePickerTheme);
        Context context = dialog.getContext();
        PackageManager pm = activity.getPackageManager();

        LinearLayout panel = new LinearLayout(context);
        panel.setOrientation(LinearLayout.VERTICAL);
        panel.setPadding(0, Dp(context, 12), 0, Dp(context, 20));
        GradientDrawable background = new GradientDrawable();
        background.setColor(dark ? Color.rgb(35, 35, 38) : Color.rgb(250, 250, 252));
        float radius = Dp(context, 24);
        background.setCornerRadii(new float[] {radius, radius, radius, radius, 0, 0, 0, 0});
        panel.setBackground(background);

        View handle = new View(context);
        GradientDrawable handleBackground = new GradientDrawable();
        handleBackground.setColor(dark ? Color.rgb(100, 100, 105) : Color.rgb(190, 190, 195));
        handleBackground.setCornerRadius(Dp(context, 2));
        handle.setBackground(handleBackground);
        handle.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
        LinearLayout.LayoutParams handleParams = new LinearLayout.LayoutParams(
            Dp(context, 36), Dp(context, 4));
        handleParams.gravity = Gravity.CENTER_HORIZONTAL;
        handleParams.bottomMargin = Dp(context, 16);
        panel.addView(handle, handleParams);

        HorizontalScrollView scroll = new HorizontalScrollView(context);
        scroll.setHorizontalScrollBarEnabled(false);
        scroll.setFillViewport(true);
        LinearLayout row = new LinearLayout(context);
        row.setOrientation(LinearLayout.HORIZONTAL);
        // 内容不足一屏时居中；超过一屏时仍保持横向滚动。
        row.setGravity(Gravity.CENTER);
        row.setPadding(Dp(context, 12), 0, Dp(context, 12), 0);

        TypedValue selectableBackground = new TypedValue();
        context.getTheme().resolveAttribute(android.R.attr.selectableItemBackground,
            selectableBackground, true);
        for(ResolveInfo target : targets)
        {
            CharSequence label = target.loadLabel(pm);
            LinearLayout item = new LinearLayout(context);
            item.setOrientation(LinearLayout.VERTICAL);
            item.setGravity(Gravity.TOP | Gravity.CENTER_HORIZONTAL);
            item.setPadding(Dp(context, 6), Dp(context, 12), Dp(context, 6), Dp(context, 8));
            if(selectableBackground.resourceId != 0)
                item.setBackgroundResource(selectableBackground.resourceId);
            item.setFocusable(true);
            item.setContentDescription(label);

            ImageView icon = new ImageView(context);
            icon.setImageDrawable(target.loadIcon(pm));
            icon.setScaleType(ImageView.ScaleType.FIT_CENTER);
            icon.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
            item.addView(icon, new LinearLayout.LayoutParams(Dp(context, 48), Dp(context, 48)));

            TextView name = new TextView(context);
            name.setText(label);
            name.setTextSize(TypedValue.COMPLEX_UNIT_SP, 12);
            name.setTextColor(dark ? Color.rgb(240, 240, 245) : Color.rgb(35, 35, 40));
            name.setGravity(Gravity.TOP | Gravity.CENTER_HORIZONTAL);
            name.setMaxLines(2);
            name.setEllipsize(TextUtils.TruncateAt.END);
            name.setImportantForAccessibility(View.IMPORTANT_FOR_ACCESSIBILITY_NO);
            LinearLayout.LayoutParams nameParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
            nameParams.topMargin = Dp(context, 10);
            item.addView(name, nameParams);
            item.setOnClickListener(view -> {
                dialog.dismiss();
                StartTargetActivity(activity, shareIntent, target);
            });
            row.addView(item, new LinearLayout.LayoutParams(
                Dp(context, 96), ViewGroup.LayoutParams.WRAP_CONTENT));
        }

        scroll.addView(row, new ViewGroup.LayoutParams(
            ViewGroup.LayoutParams.WRAP_CONTENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        panel.addView(scroll, new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        dialog.setContentView(panel);
        dialog.setCanceledOnTouchOutside(true);
        Window window = dialog.getWindow();
        window.setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
        window.setGravity(Gravity.BOTTOM | Gravity.CENTER_HORIZONTAL);
        window.addFlags(WindowManager.LayoutParams.FLAG_DIM_BEHIND);
        WindowManager.LayoutParams params = window.getAttributes();
        params.dimAmount = 0.35f;
        window.setAttributes(params);
        window.setWindowAnimations(R.style.FHSharePickerAnimation);
        dialog.show();
        scroll.post(() -> {
            // XML Theme 已经把窗口宽度设置为可用宽度的 75%。
            // 这里仅使用 Dialog 测量后的实际宽度，让入口保持水平居中。
            int contentWidth = scroll.getWidth();
            if(contentWidth > 0)
            {
                row.setMinimumWidth(contentWidth);
                row.requestLayout();
            }
        });
    }

    private static void StartTargetActivity(Activity activity, Intent shareIntent, ResolveInfo target) {
        Intent selectedIntent = new Intent(shareIntent);
        selectedIntent.setComponent(new ComponentName(
            target.activityInfo.packageName, target.activityInfo.name));
        try
        {
            activity.startActivity(selectedIntent);
        }
        catch(android.content.ActivityNotFoundException e)
        {
            Log.w(TAG, "Share activity is unavailable: " + selectedIntent.getComponent(), e);
            Toast.makeText(activity, R.string.fh_share_target_unavailable,
                Toast.LENGTH_SHORT).show();
        }
    }

    private static int Dp(Context context, int value) {
        return Math.round(value * context.getResources().getDisplayMetrics().density);
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
