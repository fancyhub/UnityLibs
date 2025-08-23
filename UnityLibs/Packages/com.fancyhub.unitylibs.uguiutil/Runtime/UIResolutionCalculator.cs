/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/12/27
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using ScreenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode;

namespace FH.UI
{
    [System.Serializable]
    public struct UIResolutionCalculator
    {
        private const float kLogBase = 2;

        public Vector2 RefResolution;
        public ScreenMatchMode MatchMode;

        [Range(0, 1.0f)]
        public float MatchWidthOrHeight; //MatchMode == ScreenMatchMode.MatchWidthOrHeight;

        private Vector2 _LastScreenSize;
        private Vector2 _LastResolution;

        public UIResolutionCalculator(Vector2 refResolution, ScreenMatchMode matchMode = ScreenMatchMode.Expand, float matchWidthOrHeight = 0)
        {
            RefResolution = refResolution;
            MatchMode = matchMode;
            MatchWidthOrHeight = matchWidthOrHeight;
            _LastScreenSize = Vector2.zero;
            _LastResolution = Vector2.zero;
        }

        public float CalcScaleFactor(Vector2 screenSize)
        {
            float scaleFactor = 1.0f;
            switch (MatchMode)
            {
                default:
                    scaleFactor = 1.0f;
                    break;

                case ScreenMatchMode.MatchWidthOrHeight:
                    float logWidth = Mathf.Log(screenSize.x / RefResolution.x, kLogBase);
                    float logHeight = Mathf.Log(screenSize.y / RefResolution.y, kLogBase);
                    float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, MatchWidthOrHeight);
                    scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);
                    break;

                case ScreenMatchMode.Expand:
                    scaleFactor = Mathf.Min(screenSize.x / RefResolution.x, screenSize.y / RefResolution.y);
                    break;

                case ScreenMatchMode.Shrink:
                    scaleFactor = Mathf.Max(screenSize.x / RefResolution.x, screenSize.y / RefResolution.y);
                    break;
            }
            return scaleFactor;
        }

        public Vector2 CalcResultion(Vector2 screenSize)
        {
            if (Mathf.Abs(_LastScreenSize.x - screenSize.x) < float.Epsilon && Mathf.Abs(_LastScreenSize.y - screenSize.y) < float.Epsilon)
                return _LastResolution;
            _LastScreenSize = screenSize;

            float scaleFactor = CalcScaleFactor(screenSize);
            _LastResolution = screenSize / scaleFactor;

            return _LastResolution;
        }

        public Vector2 CalcResultion()
        {
            return CalcResultion(new Vector2(Screen.width, Screen.height));
        }
    }
}
