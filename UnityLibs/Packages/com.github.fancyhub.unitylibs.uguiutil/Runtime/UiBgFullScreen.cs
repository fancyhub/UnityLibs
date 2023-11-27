using UnityEngine;
using UnityEngine.UI;

namespace FH.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class UiBgFullScreen : UnityEngine.EventSystems.UIBehaviour
    {
        public void Adjust()
        {
            //1.先获取根节点 & size
            RectTransform root_rect = _GetRoot();
            if (root_rect == null)
                return;
            Vector2 size = root_rect.rect.size + Vector2.one; //加1, 防止有边缘缝隙                                                              

            //2. 调整自身的大小
            {
                RectTransform rect = GetComponent<RectTransform>();
                rect.anchorMin = Vector2.one * 0.5f;
                rect.anchorMax = Vector2.one * 0.5f;
                rect.pivot = Vector2.one * 0.5f;
                rect.sizeDelta = size;
                rect.position = root_rect.TransformPoint(Vector3.zero);
            }

            //3. 如果有RawImg,调整RawImg的scale            
            GetComponent<RawImage>().ExtScaleUV(ScaleMode.ScaleAndCrop);
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            Adjust();
        }
#endif

        protected override void OnRectTransformDimensionsChange()
        {
            Adjust();
        }

        protected override void OnTransformParentChanged()
        {
            Adjust();
        }

        protected override void OnEnable()
        {
            Adjust();
        }

        private RectTransform _GetRoot()
        {
            if (Application.isPlaying)
            {
                Canvas rootCanvas = _GetRootCanvas();
                if (rootCanvas != null)
                    return rootCanvas.transform as RectTransform;
            }

            return _GetRootTransform(transform);
        }

        private Canvas _GetRootCanvas()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                return null;
            if (canvas.isRootCanvas)
                return canvas;
            return canvas.rootCanvas;
        }

        private RectTransform _GetRootTransform(Transform tran)
        {
            if (tran == null)
                return null;

            RectTransform ret = null;
            tran = tran.parent;
            for (; ; )
            {
                if (tran == null)
                    break;
                RectTransform temp_rect = tran.GetComponent<RectTransform>();
                if (temp_rect != null)
                    ret = temp_rect;

                tran = tran.parent;
            }
            return ret;
        }
    }
}