/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/3 12:04:25
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace FH.UI
{
    /// <summary>
    /// 左下为 0,0
    /// </summary>
    [Serializable]
    public struct UISafeAreaOffset
    {
        public float Left;
        public float Right;
        public float Bottom;
        public float Top;

        public UISafeAreaOffset(float left, float right, float bottom, float top)
        {
            Left = left;
            Right = right;
            Bottom = bottom;
            Top = top;
        }

        public override string ToString()
        {
            return $"Left: {Left}, Right: {Right},Bottom: {Bottom}, Top: {Top}";
        }
    }

    [Serializable]
    public struct UISafeAreaInfo
    {
        public Vector2 ScreenSize;
        public Vector2 UIResolutionSize;

        public Rect ScreenSafeArea;
        public UISafeAreaOffset UISafeAreaOffset;
    }

    [Serializable]
    public class UISafeAreaData
    {
        public Vector2 AnchorMin = Vector2.zero;
        public Vector2 AnchorMax = Vector2.one;
        public Vector2 Pivot = Vector2.one * 0.5f;

        public Vector2 AnchoredPos = Vector2.zero;
        public Vector2 SizeDelta = Vector2.zero;
    }

    /// <summary>
    /// 挂在根节点上
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class UISafeAreaRoot : MonoBehaviour
    {
        public UISafeAreaData _SafeAreaData = new UISafeAreaData();
        public UISafeAreaInfo _SafeAreaInfo = new UISafeAreaInfo();

        public bool _Manual = false;
        public UISafeAreaOffset _ManualScreenOffset = new UISafeAreaOffset();

        public ScreenSafeAreaCalculator _Calculator;

        public void Start()
        {
            _Calculator = new ScreenSafeAreaCalculator(GetComponent<CanvasScaler>());
        }

        public void Update()
        {
            _Calculator.EdSetManual(_Manual, _ManualScreenOffset);

            if (!_Calculator.CalcSafeArea(out _SafeAreaInfo))
                return;

            var offset = _SafeAreaInfo.UISafeAreaOffset;

            _SafeAreaData.AnchoredPos.x = offset.Left * 0.5f - offset.Right * 0.5f;
            _SafeAreaData.AnchoredPos.y = offset.Bottom * 0.5f - offset.Top * 0.5f;
            _SafeAreaData.SizeDelta.x = -offset.Left - offset.Right;
            _SafeAreaData.SizeDelta.y = -offset.Top - offset.Bottom;


            var list = this.ExtGetCompsInChildren<UISafeAreaPanel>(false);
            foreach (var p in list)
            {
                p.Adjust(_SafeAreaData);
            }
        }
    }


    public struct ScreenSafeAreaCalculator
    {
        private const float CFloat2Int = 100;

        private RectTransform _RootRectTransform;

        private Vector2Int _LastUIResolution;
        private Vector2Int _LastScreenSize;
        private RectInt _LastScreenSafeArea;

        private UISafeAreaInfo _LastSafeAreaInfo;
#if UNITY_EDITOR
        private bool _Manual;
        private UISafeAreaOffset _ManualScreenOffset;
#endif

        public ScreenSafeAreaCalculator(CanvasScaler canvas_scaler)
        {
            _RootRectTransform = canvas_scaler.GetComponent<RectTransform>();
            _LastSafeAreaInfo = new UISafeAreaInfo();
            _LastScreenSize = new Vector2Int();
            _LastScreenSafeArea = new RectInt();
            _LastUIResolution = new Vector2Int();

#if UNITY_EDITOR
            _Manual = false;
            _ManualScreenOffset = new UISafeAreaOffset();
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public void EdSetManual(bool manual, UISafeAreaOffset screen_safe_are_offset)
        {
#if UNITY_EDITOR
            _Manual = manual;
            _ManualScreenOffset = screen_safe_are_offset;
#endif
        }

        /// <summary>
        /// 返回是否发生变化, 比如屏幕旋转
        /// </summary>
        public bool CalcSafeArea(out UISafeAreaInfo out_safe_area_info)
        {
            Vector2 screen_size = new Vector2(Screen.width, Screen.height);
            Rect screen_safe_area = Screen.safeArea;
            Vector2 ui_resolution = _RootRectTransform.rect.size;

#if UNITY_EDITOR
            if (_Manual)
            {
                float left = Mathf.Clamp(_ManualScreenOffset.Left, 0, screen_size.x * 0.5f - 5);
                float right = Mathf.Clamp(_ManualScreenOffset.Right, 0, screen_size.x * 0.5f - 5);
                float top = Mathf.Clamp(_ManualScreenOffset.Top, 0, screen_size.y * 0.5f - 5);
                float bottom = Mathf.Clamp(_ManualScreenOffset.Bottom, 0, screen_size.y * 0.5f - 5);

                screen_safe_area = new Rect(left, bottom, screen_size.x - left - right, screen_size.y - top - bottom);
            }
#endif

            Vector2Int screen_size_int = new Vector2Int((int)(screen_size.x * CFloat2Int), (int)(screen_size.y * CFloat2Int));
            RectInt screen_safe_area_int = new RectInt(
                (int)(screen_safe_area.x * CFloat2Int),
                (int)(screen_safe_area.y * CFloat2Int),
                (int)(screen_safe_area.width * CFloat2Int),
                (int)(screen_safe_area.height * CFloat2Int));
            Vector2Int ui_resolution_int = new Vector2Int((int)(ui_resolution.x * CFloat2Int), (int)(ui_resolution.y * CFloat2Int));

            if (_LastScreenSafeArea.Equals(screen_safe_area_int)
                && _LastScreenSize.Equals(screen_size_int)
                && _LastUIResolution.Equals(ui_resolution_int))
            {
                out_safe_area_info = _LastSafeAreaInfo;
                return false;
            }

            if(screen_size_int.x == 0 || screen_size_int.y==0||ui_resolution_int.x == 0 || ui_resolution_int.y==0 )
            {
                out_safe_area_info = _LastSafeAreaInfo;
                return false;
            }


            //3. 计算
            _LastScreenSafeArea = screen_safe_area_int;
            _LastScreenSize = screen_size_int;
            _LastUIResolution = ui_resolution_int;

            float width_ratio = ui_resolution.x / screen_size.x;
            float height_ratio = ui_resolution.y / screen_size.y;

            float x = screen_safe_area.x * width_ratio;
            float y = screen_safe_area.y * height_ratio;
            float width = screen_safe_area.width * width_ratio;
            float height = screen_safe_area.height * height_ratio;


            _LastSafeAreaInfo.ScreenSize = screen_size;
            _LastSafeAreaInfo.UIResolutionSize = ui_resolution;
            _LastSafeAreaInfo.ScreenSafeArea = screen_safe_area;
            _LastSafeAreaInfo.UISafeAreaOffset = new UISafeAreaOffset(
                    x,
                    ui_resolution.x - x - width,
                    y,
                    ui_resolution.y - y - height);

            out_safe_area_info = _LastSafeAreaInfo;

            //Debug.Log($"UIResolution:{_LastUIResolution} \nScreenSize:{new Vector2(Screen.width, Screen.height)}, \nSafeArea:{_LastScreenSafeArea}\n UISafeArea:{_LastUIResolutionSafeArea}");
            return true;
        }
    }
}

