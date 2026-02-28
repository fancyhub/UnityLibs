/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if UNITY_IOS || UNITY_EDITOR
namespace FH
{
    internal class PlatformShare_IOS : IPlatformShareUtil
    {
        private static readonly _.FHScreenshotEventCallback _FHScreenshotEventCallback = _OnScreenShotEvent;
        private static Action _ScreenShotEventCallBack;        

        public PlatformShare_IOS()
        {
        }

        public void StartScreenshotListen(Action callBack)
        {
            _ScreenShotEventCallBack = callBack;
            _.FHStartScreenShotListener(_FHScreenshotEventCallback);
        }

        public void StopScreenshotListen()
        {
            _.FHStopScreenShotListener();
        }


        public EShareCopyImageResult CopyLocalImage2Gallery(string srcFillePath, string destFileName)
        {
            int result = _.FHSaveImageToPhotoAlbum(srcFillePath);
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

        public void Share(string title, string text, string imageFilePath)
        {
            _.FHShare(title, text, imageFilePath);
        }

        [AOT.MonoPInvokeCallback(typeof(_.FHScreenshotEventCallback))]
        private static void _OnScreenShotEvent()
        {
            _ScreenShotEventCallBack?.Invoke();
        }

        public string GetLastPhotoAssetId()
        {
            return _.FHGetLatestPhotoAssetID();
        }

        private static partial class _
        {
            public delegate void FHScreenshotEventCallback();

            [DllImport("__Internal")] public static extern void FHStartScreenShotListener(FHScreenshotEventCallback callBack);
            [DllImport("__Internal")] public static extern void FHStopScreenShotListener();

            [DllImport("__Internal")] public static extern string FHGetLatestPhotoAssetID();            

            [DllImport("__Internal")] public static extern int FHSaveImageToPhotoAlbum(string imagePath);

            [DllImport("__Internal")] public static extern void FHShare(string title, string text, string imageFilePath);
        }
    }
}
#endif