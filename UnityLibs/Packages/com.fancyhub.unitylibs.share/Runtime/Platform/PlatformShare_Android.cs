/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;

#if UNITY_ANDROID || UNITY_EDITOR

namespace FH
{
    internal class PlatformShare_Android : IPlatformShareUtil
    {
        public PlatformShare_Android()
        {
        }

        private static AndroidJavaClass _ShareUtil;
        private static AndroidJavaClass ShareUtil
        {
            get
            {
                if (_ShareUtil == null)
                    _ShareUtil = new AndroidJavaClass("com.fancyhub.ShareUtil");
                return _ShareUtil;
            }
        }

        void IPlatformShareUtil.StartScreenshotListen(Action callBack)
        {
            if (callBack == null)
                return;

            ShareUtil.CallStatic("RegisterScreenshotReceiver", new ScreenshotCallBack(callBack));
        }

        void IPlatformShareUtil.StopScreenshotListen()
        {
            ShareUtil.CallStatic("UnregisterReceiver");
        }

        EShareCopyImageResult IPlatformShareUtil.CopyLocalImage2Gallery(string srcFillePath, string destFileName)
        {
            int result = ShareUtil.CallStatic<int>("CopyImage2Gallery", srcFillePath, destFileName);

            switch (result)
            {
                case 0:
                    return EShareCopyImageResult.OK;
                case 1:
                    return EShareCopyImageResult.NoPermission;
                case 2:
                    return EShareCopyImageResult.Unkown;

                default:
                    return EShareCopyImageResult.Unkown;
            }
        }

        void IPlatformShareUtil.Share(string title, string text, string imageFilePath, EShareTarget target)
        {
            _Share(null, title, text, imageFilePath, GetPackageId(target));
        }

        private static string GetPackageId(EShareTarget target)
        {
            return target switch
            {
                // 国内
                EShareTarget.Wechat => "com.tencent.mm",
                EShareTarget.Douyin => "com.ss.android.ugc.aweme",
                EShareTarget.TikTok => "com.zhiliaoapp.musically",
                EShareTarget.QQ => "com.tencent.mobileqq",
                EShareTarget.Weibo => "com.sina.weibo",
                EShareTarget.Xiaohongshu => "com.xingin.xhs",
                EShareTarget.Kuaishou => "com.smile.gifmaker",
                EShareTarget.DingTalk => "com.alibaba.android.rimet",
                EShareTarget.Bilibili => "tv.danmaku.bili",
                EShareTarget.Alipay => "com.eg.android.AlipayGphone",

                // 海外
                EShareTarget.Facebook => "com.facebook.katana",
                EShareTarget.Messenger => "com.facebook.orca",
                EShareTarget.Instagram => "com.instagram.android",
                EShareTarget.WhatsApp => "com.whatsapp",
                EShareTarget.X => "com.twitter.android",
                EShareTarget.Telegram => "org.telegram.messenger",
                EShareTarget.Snapchat => "com.snapchat.android",
                EShareTarget.Line => "jp.naver.line.android",
                EShareTarget.Discord => "com.discord",
                EShareTarget.Pinterest => "com.pinterest",
                EShareTarget.Reddit => "com.reddit.frontpage",
                EShareTarget.Gmail => "com.google.android.gm",
                EShareTarget.GoogleDrive => "com.google.android.apps.docs",
                EShareTarget.YouTube => "com.google.android.youtube",
                _ => null,
            };
        }


        private void _Share(string choserTitle, string contentSubject, string contentText, string contentImageFilePath, string targetAppPackageId)
        {
            ShareUtil.CallStatic("Share", choserTitle, contentSubject, contentText, contentImageFilePath, targetAppPackageId);
        }

        internal class ScreenshotCallBack : AndroidJavaProxy
        {
            private Action _CallBack;
            public ScreenshotCallBack(Action callBack)
               : base("com.fancyhub.IScreenshotCallBack")
            {
                _CallBack = callBack;
            }

            public void OnScreenshot()
            {
                UnityThread.RunInUpdate(_CallBack);
            }
        }
    }
}
#endif
