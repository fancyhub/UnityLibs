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
        private WebView _WebView;
        public string Url;

        private RectTransform _RectTransform;
        private RectTransform GetRectTran()
        {
            if (_RectTransform == null)
                _RectTransform = GetComponent<RectTransform>();
            return _RectTransform;
        }

        protected override void OnEnable()
        {
            if (_WebView == null)            
                Open(Url);            

            _WebView?.SetVisible(true);            
        }

        protected override void OnDisable()
        {
            _WebView?.SetVisible(false);
        }

        public void Open(string url)
        {
            if (_WebView == null || !_WebView.IsValid())
            {
                if (!GetRectTran().ExtToScreenNormalize(RectTransformExt.EModeY.Top_Zero, out var size, out var _))
                    return;
                _WebView = WebViewMgr.Create(Url, size);
            }
            else
            {
                _WebView.Navigate(url);
            }
        }

        public void Close()
        {
            _WebView?.Close();
            _WebView = null;
        }

        protected override void OnRectTransformDimensionsChange()
        {
            if (!GetRectTran().ExtToScreenNormalize(RectTransformExt.EModeY.Top_Zero, out var size, out var _))
                return;
            _WebView?.Resize(size);
        }
    }
}
