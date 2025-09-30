/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/28
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using FH.UI;

namespace FH
{
    [RequireComponent(typeof(RectTransform))]
    public class UIWebView : UnityEngine.EventSystems.UIBehaviour
    {
        private int _WebViewId;
        public string Url;

        public bool EnableWebViewParameters = false;
        public WebViewParameters WebViewParameters;

        private RectTransform _RectTransform;
        private RectTransform GetRectTran()
        {
            if (_RectTransform == null)
                _RectTransform = GetComponent<RectTransform>();
            return _RectTransform;
        }

        protected override void OnEnable()
        {
            if(!GetRectTran().ExtToScreenNormalize(RectTransformExt.EModeY.Top_Zero, out var size, out var _))
                return;

            _WebViewId = UnityWebView.Open(Url, size, EnableWebViewParameters? WebViewParameters:null);
        }

        protected override void OnDisable()
        {
            UnityWebView.Close(ref _WebViewId);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (!GetRectTran().ExtToScreenNormalize(RectTransformExt.EModeY.Top_Zero, out var size, out var _))
                return;
            UnityWebView.Resize(_WebViewId, size);
        }
    }
}
