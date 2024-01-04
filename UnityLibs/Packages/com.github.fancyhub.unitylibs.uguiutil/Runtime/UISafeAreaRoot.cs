/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/3 12:04:25
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace FH.UI
{
    /// <summary>
    /// 挂在根节点上
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class UISafeAreaRoot : MonoBehaviour
    {
        [Serializable]
        public struct UISafeAreaRectInfo
        {
            public static UISafeAreaRectInfo Default = new UISafeAreaRectInfo()
            {
                AnchorMin = Vector2.zero,
                AnchorMax = Vector2.one,
                Pivot = Vector2.one * 0.5f,
                AnchoredPos = Vector2.zero,
                SizeDelta = Vector2.zero,
            };

            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 Pivot;
            public Vector2 AnchoredPos;
            public Vector2 SizeDelta;
        }
        public UISafeAreaRectInfo _SafeAreaRectInfo = UISafeAreaRectInfo.Default;

        [Serializable]
        public struct UISafeAreaOffset
        {
            public float Left;
            public float Right;
            public float Top;
            public float Bottom;
        }

        public Vector2Int _ScreenSize;
        public Rect _ScreenSafeArea;

        public bool _Manual = false;
        public UISafeAreaOffset _ManualOffset;
        public bool _ShowSafeArea = false;

        public void Awake()
        {
            _SafeAreaRectInfo = UISafeAreaRectInfo.Default;
            _ScreenSize = default;
            _ScreenSafeArea = default;
        }

        public void Start()
        {
            _Update();
        }

        public void Update()
        {
            _Update();
        }

#if UNITY_EDITOR
        public void OnGUI()
        {
            if (!_ShowSafeArea)
                return;

            //GUI 是左上角 为0,0
            //Screen 是左下角为0,0
            Rect rect = _ScreenSafeArea;
            _ScreenSafeArea.y = _ScreenSize.y - rect.yMax;
            _DrawRect(rect, Color.yellow, 10);
        }

        private static void _DrawRect(Rect rect, Color color, float line_width)
        {
            Rect left = new Rect(rect.x - line_width * 0.5f, rect.y, line_width, rect.height);
            Rect right = new Rect(rect.xMax - line_width * 0.5f, rect.y, line_width, rect.height);

            Rect top = new Rect(rect.x, rect.y - line_width * 0.5f, rect.width, line_width);
            Rect bottom = new Rect(rect.x, rect.yMax - line_width * 0.5f, rect.width, line_width);

            GUI.DrawTexture(left, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, 0);
            GUI.DrawTexture(right, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, 0);
            GUI.DrawTexture(top, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, 0);
            GUI.DrawTexture(bottom, Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, color, 0, 0);
        }
#endif

        private void _Update()
        {
            Vector2Int screen_size = new Vector2Int(Screen.width, Screen.height);
            if (screen_size.x == 0 || screen_size.y == 0)
                return;

            Rect screen_safe_area = Screen.safeArea;
#if UNITY_EDITOR
            if (_Manual)
            {
                screen_safe_area.xMin = Math.Clamp(_ManualOffset.Left, 0, screen_size.x * 0.5f - 5);
                screen_safe_area.xMax = Math.Clamp(screen_size.x - _ManualOffset.Right, screen_size.x * 0.5f + 5, screen_size.x);

                screen_safe_area.yMin = Math.Clamp(_ManualOffset.Bottom, 0, screen_size.y * 0.5f - 5);
                screen_safe_area.yMax = Math.Clamp(screen_size.y - _ManualOffset.Top, screen_size.y * 0.5f + 5, screen_size.y);
            }
#endif
            if (_ScreenSafeArea.Equals(screen_safe_area) && _ScreenSize.Equals(screen_size))
                return;

            _ScreenSize = screen_size;
            _ScreenSafeArea = screen_safe_area;

            //Mode: change anchorMin & Max
            _SafeAreaRectInfo.AnchorMin.x = _ScreenSafeArea.x / _ScreenSize.x;
            _SafeAreaRectInfo.AnchorMin.y = _ScreenSafeArea.y / _ScreenSize.y;

            _SafeAreaRectInfo.AnchorMax.x = _ScreenSafeArea.xMax / _ScreenSize.x;
            _SafeAreaRectInfo.AnchorMax.y = _ScreenSafeArea.yMax / _ScreenSize.y;


            var list = this.ExtGetCompsInChildren<UISafeAreaPanel>(false);
            foreach (var p in list)
            {
                p.Adjust(_SafeAreaRectInfo);
            }
        }
    }
}

