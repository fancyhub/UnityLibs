/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/3 12:04:25
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FH.UI
{
    public struct ScreenSafeAreaCalculator
    {
        private RectTransform _RootRectTransform;
        private CanvasScaler _CanvasScaler;

        private Vector2 _LastUIResolution;
        private Rect _LastScreenSafeArea;

        private Rect _LastUIResolutionSafeArea;

        public ScreenSafeAreaCalculator(CanvasScaler canvas_scaler)
        {
            _CanvasScaler = canvas_scaler;
            _RootRectTransform = _CanvasScaler.GetComponent<RectTransform>();
            _LastUIResolution = new Vector2();
            _LastScreenSafeArea = new Rect();
            _LastUIResolutionSafeArea = new Rect();
        }

        /// <summary>
        /// 返回是否发生变化, 比如屏幕旋转
        /// </summary>
        public bool CalcSafeArea(out Vector2 ui_resolution, out Rect ui_resolution_safe_area)
        {
            //1. 比较
            bool changed = false;
            if (Screen.safeArea != _LastScreenSafeArea)
            {
                changed = true;
            }
            if (_RootRectTransform.rect.size != _LastUIResolution)
            {
                changed = true;
            }
            if (!changed)
            {
                ui_resolution = _LastUIResolution;
                ui_resolution_safe_area = _LastUIResolutionSafeArea;
                return false;
            }

            //2. 记录
            _LastUIResolution = _RootRectTransform.rect.size;
            _LastScreenSafeArea = Screen.safeArea;



            //3. 计算
            float width_ratio = _LastUIResolution.x / Screen.width;
            float height_ratio = _LastUIResolution.y / Screen.height;

            float x = _LastScreenSafeArea.x * width_ratio;
            float y = _LastScreenSafeArea.y * height_ratio;
            float width = _LastScreenSafeArea.width * width_ratio;
            float height = _LastScreenSafeArea.height * height_ratio;
            _LastUIResolutionSafeArea = new Rect(x, y, width, height);

            ui_resolution = _LastUIResolution;
            ui_resolution_safe_area = _LastUIResolutionSafeArea;

            Debug.Log($"UIResolution:{_LastUIResolution} \nScreenSize:{new Vector2(Screen.width, Screen.height)}, \nSafeArea:{_LastScreenSafeArea}\n UISafeArea:{_LastUIResolutionSafeArea}");
            return true;
        }
    }

    /// <summary>
    /// 不能和 UIBgFullScreen 在一起
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UISafeAreaPanel : UIBehaviour
    {
        private static Rect _UIResolutionSafeArea = new Rect();
        private static Vector2 _UIResolution = new Vector2();

        private static Vector2 _SafeAreaPos = Vector2.zero;
        private static Vector2 _SafeAreaSize = Vector2.zero;

        public static void ChangeSafeArea(Vector2 ui_resolution, Rect ui_resolution_safe_area, Transform root)
        {
            if (_UIResolutionSafeArea == ui_resolution_safe_area && _UIResolution == ui_resolution)
                return;
            _UIResolutionSafeArea = ui_resolution_safe_area;
            _UIResolution = ui_resolution;

            float left_offset = ui_resolution_safe_area.x;
            float right_offset = ui_resolution.x - ui_resolution_safe_area.xMax;

            float top_offset = ui_resolution_safe_area.y;
            float bottom_offset = ui_resolution.y - ui_resolution_safe_area.yMax;

            _SafeAreaPos = new Vector2(left_offset * 0.5f - right_offset * 0.5f, top_offset*0.5f-bottom_offset*0.5f);
            _SafeAreaSize = new Vector2(-left_offset- right_offset,-top_offset-bottom_offset);


            var comps = root.ExtGetCompsInChildren<UISafeAreaPanel>(true);
            foreach (var p in comps)
            {
                p._UpdateResultion(_SafeAreaPos, _SafeAreaSize);
            }
            comps.Clear();
        }

        protected override void Start()
        {
            ResetRect();
            _UpdateResultion(_SafeAreaPos, _SafeAreaSize);
        }

        [ContextMenu("ResetRect")]
        public void ResetRect()
        {
            RectTransform rect = this.GetComponent<RectTransform>();
            if (transform.parent != null)
                _ResetRectTransform(transform.parent.GetComponent<RectTransform>());
            _ResetRectTransform(rect);
        }

        private void _UpdateResultion(Vector2 anchor_pos, Vector2 size_delta)
        {
            RectTransform rect = this.GetComponent<RectTransform>();
            rect.anchoredPosition = anchor_pos;
            rect.sizeDelta = size_delta;
        }

        private void _ResetRectTransform(RectTransform rect)
        {
            if (rect == null)
                return;

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one * 0.5f;
        }
    }
        
}

