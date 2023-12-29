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
    /// <summary>
    /// 挂在根节点上
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class UISafeAreaRoot : MonoBehaviour
    {
        public UISafeAreaData SafeAreaData = new UISafeAreaData();

        public Vector2 UIResolution;
        public Rect UIResolutionSafeArea;

        public Vector2 ScreenSize;
        public Rect ScreenSafeArea;


        public float LeftOffset = 0;
        public float RightOffset = 0;
        public float TopOffset = 0;
        public float BottomOffset = 0;

        public ScreenSafeAreaCalculator _Calculator;

        public void Start()
        {
            _Calculator = new ScreenSafeAreaCalculator(GetComponent<CanvasScaler>());
        }

        public void Update()
        {
            if (!_Calculator.CalcSafeArea(out UIResolution, out UIResolutionSafeArea))
                return;

            ScreenSize = new Vector2(Screen.width, Screen.height);
            ScreenSafeArea = Screen.safeArea;

            LeftOffset = UIResolutionSafeArea.x;
            RightOffset = UIResolution.x - UIResolutionSafeArea.xMax ;

            TopOffset = UIResolutionSafeArea.y;
            BottomOffset = UIResolution.y - UIResolutionSafeArea.yMax;

            SafeAreaData.AnchoredPos.x = LeftOffset * 0.5f - RightOffset * 0.5f;
            SafeAreaData.AnchoredPos.y = TopOffset * 0.5f - BottomOffset * 0.5f;

            SafeAreaData.SizeDelta.x = UIResolutionSafeArea.width - UIResolution.x;
            SafeAreaData.SizeDelta.y = UIResolutionSafeArea.height - UIResolution.y;


            var list = this.ExtGetCompsInChildren<UISafeAreaPanel>(false);
            foreach (var p in list)
            {
                p.Adjust(SafeAreaData);
            }
        }
    }


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

            //Debug.Log($"UIResolution:{_LastUIResolution} \nScreenSize:{new Vector2(Screen.width, Screen.height)}, \nSafeArea:{_LastScreenSafeArea}\n UISafeArea:{_LastUIResolutionSafeArea}");
            return true;
        }
    }

}

