/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
//using FH.UI;

#if UNITY_EDITOR
using PlatformShare = FH.PlatformShare_Empty;
#elif UNITY_ANDROID
    using PlatformShare = FH.PlatformShare_Android;
#elif UNITY_IOS
    using PlatformShare = FH.PlatformShare_IOS;
#else
    using PlatformShare = FH.PlatformShare_Empty;
#endif

namespace FH
{
    public enum EShareCopyImageResult
    {
        OK,
        NoPermission,
        Unkown,
    }

    internal interface IPlatformShareUtil
    {
        public void StartScreenshotListen(System.Action callBack);
        public void StopScreenshotListen();

        public EShareCopyImageResult CopyLocalImage2Gallery(string srcFillePath, string destFileName);

        public void Share(string title, string text, string imageFilePath);
    }

    public static class ShareUtil
    {
        private static IPlatformShareUtil _Share;

        private static IPlatformShareUtil GetShare()
        {
            if (_Share == null)
            {
                _Share = new PlatformShare();
            }
            return _Share;
        }

        public static void StartScreenshotListen(System.Action callBack)
        {
            GetShare()?.StartScreenshotListen(callBack);
        }

        public static void StopScreenshotListen()
        {
            GetShare()?.StopScreenshotListen();
        }


        public static EShareCopyImageResult CopyLocalImage2Gallery(string srcFillePath, string destFileName)
        {
            var inst = GetShare();
            if (inst == null)
                return EShareCopyImageResult.Unkown;

            return inst.CopyLocalImage2Gallery(srcFillePath, destFileName);
        }

        public static void ShareImage(string imageFilePath)
        {
            if (string.IsNullOrEmpty(imageFilePath))
                return;

            GetShare()?.Share(null, null, imageFilePath);
        }

        public static void ShareText(string title, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            GetShare()?.Share(title, text, null);
        }

        public static void ShareImageWithText(string title, string text, string imageFilePath)
        {
            if (string.IsNullOrEmpty(text) && string.IsNullOrEmpty(imageFilePath))
                return;

            GetShare()?.Share(title, text, imageFilePath);
        }

        public static void Capture(Action<Texture2D> callBack,
            Action<List<GameObject>> funcGetHideNodeList = null,
            RectTransform targetArea = null)
        {
            GlobalCoroutine.StartCoroutine(_CaptureRoutine(callBack, funcGetHideNodeList, targetArea));
        }

        private static List<GameObject> _TempList = new();
        private static System.Collections.IEnumerator _CaptureRoutine(
            Action<Texture2D> callBack,
            Action<List<GameObject>> funcGetHideNodeList,
            RectTransform targetArea)
        {
            //1. check
            if (callBack == null)
                yield break;

            //2. get hide node list
            _TempList.Clear();
            if (funcGetHideNodeList != null)
            {
                try { funcGetHideNodeList?.Invoke(_TempList); }
                catch (Exception e) { }
            }

            //3. calc texture size and crop area

            Vector2Int textureSize = new Vector2Int(Screen.width, Screen.height);
            Rect cropArea = new Rect(0, 0, textureSize.x, textureSize.y);
            if (targetArea != null)
            {
                RectTransformExt.EModeY mode = RectTransformExt.EModeY.Botton_Zero;
                if (!Application.isEditor)
                {
                    switch (Application.platform)
                    {
                        case RuntimePlatform.Android:
                            mode = RectTransformExt.EModeY.Top_Zero;
                            break;
                    }
                }

                targetArea.ExtToScreenNormalize(mode, out var normalized, out var _);
                int width = (int)(Screen.width * normalized.width);
                int height = (int)(Screen.height * normalized.height);
                textureSize = new Vector2Int(width, height);

                int posX = (int)Mathf.RoundToInt(Screen.width * normalized.x);
                int posY = (int)Mathf.RoundToInt(Screen.height * normalized.y);

                cropArea = new Rect(posX, posY, width, height);
            }


            //4. hide node
            if (_TempList.Count > 0)
            {
                for (int i = 0; i < _TempList.Count; i++)
                {
                    if (_TempList[i] == null)
                        continue;
                    if (!_TempList[i].activeSelf)
                    {
                        _TempList[i] = null;
                        continue;
                    }

                    _TempList[i].SetActive(false);
                }
            }

            //5. wait end of frame
            yield return new WaitForEndOfFrame();


            //6. capture
            Texture2D screenShotTexture = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGB24, false);
            screenShotTexture.ReadPixels(cropArea, 0, 0);
            screenShotTexture.Apply();



            //6. show the node
            if (_TempList.Count > 0)
            {
                for (int i = 0; i < _TempList.Count; i++)
                {
                    if (_TempList[i] == null)
                        continue;
                    _TempList[i].SetActive(true);
                }

                _TempList.Clear();
            }


            //7. 
            callBack(screenShotTexture);
        }
    }
}