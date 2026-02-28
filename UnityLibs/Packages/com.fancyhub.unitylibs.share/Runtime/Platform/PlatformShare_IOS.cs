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
        private static readonly _.FHPhotoDataCallback _FHPhotoDataCallback = _OnPhotoDataCallBack;

        private static Action _ScreenShotEventCallBack;

        protected class LoadEventData
        {
            public string assetId;
            public Action<byte[]> callback;
            public DateTime startTime;
        }

        private static List<LoadEventData> _LoadedEvents = new();
        private static List<LoadEventData> _Temp = new List<LoadEventData>();
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

        public void LoadPhotoAsync(string phAssetId, Action<byte[]> callBack)
        {
            if (callBack == null)
                return;
            if (string.IsNullOrEmpty(phAssetId))
            {
                callBack(null);
                return;
            }
            _LoadedEvents.Add(new LoadEventData()
            {
                assetId = phAssetId,
                callback = callBack,
                startTime = DateTime.UtcNow,
            });

            _.FHLoadPhotoByAssetID(phAssetId, _FHPhotoDataCallback);
        }

        //会在主线程调用
        [AOT.MonoPInvokeCallback(typeof(_.FHPhotoDataCallback))]
        private static void _OnPhotoDataCallBack(IntPtr assetData, int assetDataLen, IntPtr assetId)
        {
            if (assetId == IntPtr.Zero)
                return;
            string assetPath = Marshal.PtrToStringUTF8(assetId);

            _Temp.Clear();
            for (int i = _LoadedEvents.Count - 1; i >= 0; i--)
            {
                if (_LoadedEvents[i].assetId == assetPath)
                {
                    _Temp.Add(_LoadedEvents[i]);
                    _LoadedEvents.RemoveAt(i);
                }
                else if ((DateTime.UtcNow - _LoadedEvents[i].startTime) > new TimeSpan(0, 1, 0)) //超过一分钟了
                {
                    _Temp.Add(_LoadedEvents[i]);
                    _LoadedEvents.RemoveAt(i);
                }
            }

            try
            {
                if (assetData == IntPtr.Zero || assetDataLen == 0)
                {
                    foreach (var p in _Temp)
                    {
                        p.callback(null);
                    }
                }
                else
                {
                    byte[] data = new byte[assetDataLen];
                    Marshal.Copy(assetData, data, 0, assetDataLen);

                    foreach (var p in _Temp)
                    {
                        if (p.assetId == assetPath)
                        {
                            p.callback(data);
                        }
                        else
                        {
                            p.callback(null);
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                _Temp.Clear();
            }
        }


        private static partial class _
        {
            public delegate void FHScreenshotEventCallback();
            public delegate void FHPhotoDataCallback(IntPtr assetData, int assetDataLen, IntPtr assetId);

            [DllImport("__Internal")] public static extern void FHStartScreenShotListener(FHScreenshotEventCallback callBack);
            [DllImport("__Internal")] public static extern void FHStopScreenShotListener();

            [DllImport("__Internal")] public static extern string FHGetLatestPhotoAssetID();

            [DllImport("__Internal")] public static extern void FHLoadPhotoByAssetID(string assetId, FHPhotoDataCallback callBack);

            [DllImport("__Internal")] public static extern int FHSaveImageToPhotoAlbum(string imagePath);

            [DllImport("__Internal")] public static extern void FHShare(string title, string text, string imageFilePath);

        }
    }
}
#endif