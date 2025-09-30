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
        public enum EUISafeAreaOffset
        {
            Canvas,
            Screen,
            Ratio,
        }

        [Serializable]
        public struct UISize
        {
            public Vector2 Screen;
            public Vector2 Canvas;

            public static UISize Create(RectTransform rect_transform)
            {
                return new UISize()
                {
                    Screen = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height),
                    Canvas = rect_transform.rect.size
                };
            }

            public bool IsValid()
            {
                return (Screen.x > 0 && Screen.y > 0 && Canvas.x > 0 && Canvas.y > 0) ;
            }
        }

        [Serializable]
        public struct UISafeAreaRect
        {
            public EUISafeAreaOffset Mode;
            public float Left;
            public float Right;
            public float Top;
            public float Bottom;
             
            public UISafeAreaRect(EUISafeAreaOffset mode)
            {
                Mode = mode;
                Left = Right = Top = Bottom = 0;
            }
            public UISafeAreaRect Clamp(float min, float max)
            {
                return new UISafeAreaRect(Mode)
                {
                    Left = Mathf.Clamp(this.Left, min, max),
                    Right = Mathf.Clamp(this.Right, min, max),
                    Top = Mathf.Clamp(this.Top, min, max),
                    Bottom = Mathf.Clamp(this.Bottom, min, max),
                };
            }

            public static UISafeAreaRect Max(UISafeAreaRect a, UISafeAreaRect b)
            {
                return new UISafeAreaRect(a.Mode)
                {
                    Left = Mathf.Max(a.Left, b.Left),
                    Right = Mathf.Max(a.Right, b.Right),
                    Top = Mathf.Max(a.Top, b.Top),
                    Bottom = Mathf.Max(a.Bottom, b.Bottom),
                };
            }

            public bool IsEqual(UISafeAreaRect other)
            {
                if (other.Mode != Mode)
                    return false;

                const float epsilon = 0.01f;
                return Mathf.Abs(other.Left - Left) < epsilon && Mathf.Abs(other.Right - Right) < epsilon &&
                    Mathf.Abs(other.Top - Top) < epsilon && Mathf.Abs(other.Bottom - Bottom) < epsilon;
            }


            public UISafeAreaRect ToScreen(UISize size)
            {
                var ratio = this;
                if(Mode != EUISafeAreaOffset.Ratio)
                {
                    ratio = this.ToRatio(size);
                }

                return new UISafeAreaRect(EUISafeAreaOffset.Screen)
                {
                    Left = Left* size.Screen.x,
                    Right = Right * size.Screen.x,
                    Top = Top * size.Screen.y,
                    Bottom = Bottom * size.Screen.y,
                };
            }

            public UISafeAreaRect ToCanvas(UISize size)
            {
                var ratio = this;
                if (Mode != EUISafeAreaOffset.Ratio)
                {
                    ratio = this.ToRatio(size);
                }

                return new UISafeAreaRect(EUISafeAreaOffset.Canvas)
                {
                    Left = Left * size.Canvas.x,
                    Right = Right * size.Canvas.x,
                    Top = Top * size.Canvas.y,
                    Bottom = Bottom * size.Canvas.y,
                };
            }

            public UISafeAreaRect ToRatio(UISize size)
            {
                switch(Mode)
                {
                    case EUISafeAreaOffset.Screen:
                        return new UISafeAreaRect(EUISafeAreaOffset.Ratio)
                        {
                            Left = this.Left / size.Screen.x,
                            Right = this.Right / size.Screen.x,
                            Top = this.Top / size.Screen.y,
                            Bottom = this.Bottom / size.Screen.y,
                        };

                    case EUISafeAreaOffset.Canvas:
                        return new UISafeAreaRect(EUISafeAreaOffset.Ratio)
                        {
                            Left = this.Left / size.Canvas.x,
                            Right = this.Right / size.Canvas.x,
                            Top = this.Top / size.Canvas.y,
                            Bottom = this.Bottom / size.Canvas.y,
                        };

                    case EUISafeAreaOffset.Ratio:
                        return this;

                    default:
                        return new UISafeAreaRect(EUISafeAreaOffset.Ratio);
                }
            }

            public static UISafeAreaRect CreateFromScreenSafeArea(UISize size)
            {
                var ret = new UISafeAreaRect(EUISafeAreaOffset.Screen);
                var safe_area = Screen.safeArea;
                ret.Left = safe_area.xMin;
                ret.Right = size.Screen.x - safe_area.xMax;
                ret.Top = size.Screen.y-safe_area.yMax;
                ret.Bottom =  safe_area.yMin;

                return ret;
            }           
        }

        public UISafeAreaRect _Offset;

        private RectTransform _RectTransform;
        public UISize _Size;
        public UISafeAreaRect _FinalScreenSafeArea;
        public UISafeAreaRect _FinalCanvasSafeArea;
        public UISafeAreaRect _FinalRatioSafeArea;

        [Serializable]
        public struct UISafeAreaRectTranInfo
        {
            public static UISafeAreaRectTranInfo Default = new UISafeAreaRectTranInfo()
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

        public UISafeAreaRectTranInfo _ResultRectTranInfo = UISafeAreaRectTranInfo.Default;

        public UISafeAreaRectTranInfo ResultRectTranInfo => _ResultRectTranInfo;


        public bool _ShowSafeArea = false;

        public void Awake()
        {
            _ResultRectTranInfo = UISafeAreaRectTranInfo.Default;
            _FinalScreenSafeArea = new UISafeAreaRect(EUISafeAreaOffset.Screen);
            _FinalCanvasSafeArea = new UISafeAreaRect(EUISafeAreaOffset.Canvas);
            _FinalRatioSafeArea = new UISafeAreaRect(EUISafeAreaOffset.Ratio);
            _RectTransform = GetComponent<RectTransform>();
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
            Rect rect = new Rect();
            rect.x = _FinalScreenSafeArea.Left;
            rect.y = _FinalScreenSafeArea.Top;
            rect.width = _Size.Screen.x - _FinalScreenSafeArea.Left - _FinalScreenSafeArea.Right;
            rect.height = _Size.Screen.y - _FinalScreenSafeArea.Top - _FinalScreenSafeArea.Bottom;
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
            //1. 获取size
            UISize size = UISize.Create(_RectTransform);
            if (!size.IsValid())
                return;
            _Size = size;

            //2. 计算
            UISafeAreaRect screen_safe_area = UISafeAreaRect.CreateFromScreenSafeArea(size);
            UISafeAreaRect screen_safe_area_ratio= screen_safe_area.ToRatio(size);
            UISafeAreaRect manual_offset_ratio = _Offset.ToRatio(size);
            UISafeAreaRect final_ratio = UISafeAreaRect.Max(screen_safe_area_ratio, manual_offset_ratio);
            final_ratio = final_ratio.Clamp(0, 0.4f);

            //3. 判断变化
            bool is_changed = !final_ratio.IsEqual(_FinalRatioSafeArea);


            //4. 赋值
            _FinalRatioSafeArea = final_ratio;
            _FinalCanvasSafeArea = final_ratio.ToCanvas(_Size);
            _FinalScreenSafeArea = final_ratio.ToScreen(_Size);

            //Mode: change anchorMin & Max
            _ResultRectTranInfo.AnchorMin.x = _FinalRatioSafeArea.Left;
            _ResultRectTranInfo.AnchorMin.y = _FinalRatioSafeArea.Bottom;

            _ResultRectTranInfo.AnchorMax.x = 1-_FinalRatioSafeArea.Right;
            _ResultRectTranInfo.AnchorMax.y = 1-_FinalRatioSafeArea.Top;


            //5. 通知
            if (!is_changed)
                return;

            var list = this.ExtGetCompsInChildren<UISafeAreaPanel>(false);
            foreach (var p in list)
            {
                p.Adjust(_ResultRectTranInfo);
            }
            BroadcastMessage("OnRectTransformDimensionsChange", SendMessageOptions.DontRequireReceiver);
        }
    }
}

