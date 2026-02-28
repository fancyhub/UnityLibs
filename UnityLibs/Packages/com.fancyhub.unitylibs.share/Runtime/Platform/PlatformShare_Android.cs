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

        void IPlatformShareUtil.Share(string title, string text, string imageFilePath)
        {
            _Share(null, title, text, imageFilePath, null);
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