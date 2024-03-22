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
		private static Vector3[] _SRectWorldCorners = new Vector3[4];
        /// <summary>
        /// 获取屏幕对应的坐标, Normalized, 屏幕左下角(0,0),右上角(1,1)
        /// 同时返回对应的 屏幕大小
        /// </summary>
        public static bool ExtToScreenNormalize(this UnityEngine.RectTransform self, out Rect screenRectNormalize, out Vector2 screenSize)
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

            float x = root_left_bottom.x - left_bottom.x;
            float y = root_left_bottom.y - left_bottom.y;
            float width = tar_width / root_width;
            float height = tar_height / root_height;
            x = x / root_width;
            y = y / root_height;

            screenSize = new Vector2(root_width, root_height);
            screenRectNormalize = new Rect(x, y, width, height);
            return true;
        }
	}	 
}
