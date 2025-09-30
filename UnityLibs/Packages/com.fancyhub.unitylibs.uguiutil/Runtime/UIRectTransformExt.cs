/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/3/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    public static class RectTransformExt
    {
        /// <summary>
        /// 反转
        /// </summary>
        /// <param name="normalizedRect"></param>
        public static Rect InverseNormalizedRectY(Rect normalizedRect)
        {
            Rect ret = normalizedRect;
            ret.y = 1 - normalizedRect.y - normalizedRect.height;
            return ret;
        }

        public enum EModeY
        {
            Top_Zero, //屏幕上方为0
            Botton_Zero,//屏幕下方为0
        }

        private static Vector3[] _SRectWorldCorners = new Vector3[4];

        /// <summary>
        /// 获取屏幕对应的坐标, Normalized,
        ///modeY: Botton_Zero 屏幕左下角(0,0),右上角(1,1)
        ///modeY: Botton_Zero 屏幕左上角(0,0),右下角(1,1)
        /// 同时返回对应的 屏幕大小
        /// </summary>
        public static bool ExtToScreenNormalize(this UnityEngine.RectTransform self, EModeY modeY, out Rect screenRectNormalize, out Vector2 screenSize)
        {
            screenSize = new Vector2(1, 1);
            screenRectNormalize = new Rect(0, 0, 1, 1);
            if (screenRectNormalize == null)
                return false;

            Canvas canvas = self.GetComponentInParent<Canvas>();
            if (canvas == null)
                return false;
            canvas = canvas.rootCanvas;
            if (canvas == null)
                return false;
            Camera cam = canvas.worldCamera;

            //ref : https://docs.unity.cn/cn/current/ScriptReference/RectTransform.GetWorldCorners.html
            self.GetWorldCorners(_SRectWorldCorners);
            Vector2 left_bottom = RectTransformUtility.WorldToScreenPoint(cam, _SRectWorldCorners[0]);
            // Vector2 left_top = RectTransformUtility.WorldToScreenPoint(cam, _SRectWorldCorners[1]);
            Vector2 right_top = RectTransformUtility.WorldToScreenPoint(cam, _SRectWorldCorners[2]);
            // Vector2 right_down = RectTransformUtility.WorldToScreenPoint(cam, _SRectWorldCorners[3]);

            canvas.GetComponent<RectTransform>().GetWorldCorners(_SRectWorldCorners);
            Vector2 root_left_bottom = RectTransformUtility.WorldToScreenPoint(cam, _SRectWorldCorners[0]);
            // Vector2 root_left_top = RectTransformUtility.WorldToScreenPoint(cam, _SRectWorldCorners[1]);
            Vector2 root_right_top = RectTransformUtility.WorldToScreenPoint(cam, _SRectWorldCorners[2]);
            // Vector2 root_right_down = RectTransformUtility.WorldToScreenPoint(cam, _SRectWorldCorners[3]);

            float root_width = root_right_top.x - root_left_bottom.x;
            float root_height = root_right_top.y - root_left_bottom.y;

            float tar_width = right_top.x - left_bottom.x;
            float tar_height = right_top.y - left_bottom.y;

            float x = left_bottom.x - root_left_bottom.x;
            float y = left_bottom.y - root_left_bottom.y;
            float width = tar_width / root_width;
            float height = tar_height / root_height;
            x = x / root_width;
            y = y / root_height;

            screenSize = new Vector2(root_width, root_height);
            screenRectNormalize = new Rect(x, y, width, height);

            if (modeY == EModeY.Top_Zero)
                screenRectNormalize = InverseNormalizedRectY(screenRectNormalize);
            return true;
        }

        /// <summary>
        /// 获取屏幕对应的坐标, Normalized, 屏幕左上角(0,0),右下角(1,1)
        /// 同时返回对应的 屏幕大小
        /// </summary>
        public static bool ExtToScreenNormalize2(this UnityEngine.RectTransform self, out Rect screenRectNormalize, out Vector2 screenSize)
        {
            if (!self.ExtToScreenNormalize(EModeY.Botton_Zero, out screenRectNormalize, out screenSize))
                return false;

            screenRectNormalize.y = 1.0f - (screenRectNormalize.y + screenRectNormalize.height);
            return true;
        }
    }
}
