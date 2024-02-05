/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/2/4
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FH
{
    [RequireComponent(typeof(RawImage))]
    public class UIWebView : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private RawImage _RawImage;
        private RectTransform _RectTransform;
        public IWebView _WebView;
        public string Uri;
        public void Awake()
        {
            _RawImage = GetComponent<RawImage>();
            _RectTransform = GetComponent<RectTransform>();
            Vector2 size = _RectTransform.rect.size;
            _WebView = WebViewWindows.Create((int)size.x, (int)size.y);
            _WebView.TextureReady = _OnTextureReady;
            _WebView.Navigated = _OnNavigated;
            _WebView.NewWindowRequested = _OnNewWindowRequested;
        }

        protected void Start()
        {
            _WebView.Navigate(Uri);
        }

        protected void OnDestroy()
        {
            _WebView?.Dispose();
            _WebView = null;
        }

        private void _OnNavigated(string uri)
        {
            Uri = uri;
        }

        private void _OnNewWindowRequested(string uri)
        {
            Uri = uri;
            _WebView.Navigate(uri);
        }

        private void _OnTextureReady(Texture2D texture)
        {
            _RawImage.texture = texture;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            return;
            _ToLocalNormalizedPos(eventData, _RectTransform, out var local_pos);


            if (_WebView is WebViewWindows web_view_windows)
            {
                WebViewWindowsEventData data = WebViewWindowsEventData.CreateMouseEvent(eventData, EventType.MouseDown);

                Vector2 size = _RectTransform.rect.size;
                data.MousePos = new Vector2Int((int)(local_pos.x * size.x), (int)(local_pos.y * size.y));

                Debug.Log("NormalPos: " + local_pos + " Pos:" + data.MousePos);

                web_view_windows.MouseEvent(data);
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            return;
            _ToLocalNormalizedPos(eventData, _RectTransform, out var local_pos);


            if (_WebView is WebViewWindows web_view_windows)
            {
                WebViewWindowsEventData data = WebViewWindowsEventData.CreateMouseEvent(eventData, EventType.MouseUp);

                Vector2 size = _RectTransform.rect.size;
                data.MousePos = new Vector2Int((int)(local_pos.x * size.x), (int)(local_pos.y * size.y));

                Debug.Log("NormalPos: " + local_pos + " Pos:" + data.MousePos);

                web_view_windows.MouseEvent(data);
            }
        }

        protected void OnGUI()
        {
            var currentEvent = Event.current;
            if (currentEvent.isMouse || currentEvent.isScrollWheel)
            {
                _ToLocalNormalizedPos(currentEvent, _RectTransform, out var local_pos);


                if (_WebView is WebViewWindows web_view_windows)
                {
                    WebViewWindowsEventData data = WebViewWindowsEventData.CreateMouseEvent(currentEvent);

                    Vector2 size = _WebView.Size;
                    Vector2Int old_pos = new Vector2Int((int)(local_pos.x * size.x), (int)(local_pos.y * size.y));
                    Vector2Int new_pos = new Vector2Int((int)(local_pos.x * size.x), (int)currentEvent.mousePosition.y);

                    data.MousePos = old_pos;

                    Debug.Log("NormalPos: " + local_pos + " Pos:" + old_pos + " : "+ new_pos);

                    web_view_windows.MouseEvent(data);
                }
            }
        }

        /// <summary>
        /// local_normalized_pos: left_up(0,0), right_down(1,1)
        /// </summary>
        private static bool _ToLocalNormalizedPos(PointerEventData eventData, RectTransform rectTran, out Vector2 local_normalized_pos)
        {
            Camera cam = eventData.pressEventCamera;
            if (cam == null)
                cam = eventData.enterEventCamera;

            Vector2 screen_pos = eventData.position;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTran, screen_pos, cam, out var local_pos))
            {
                local_normalized_pos = Vector2.zero;
                return false;
            }

            Rect rect = rectTran.rect;
            local_normalized_pos = Rect.PointToNormalized(rect, local_pos);
            local_normalized_pos.y = 1 - local_normalized_pos.y;
            return true;
        }


        /// <summary>
        /// local_normalized_pos: left_up(0,0), right_down(1,1)
        /// </summary>
        private static bool _ToLocalNormalizedPos(UnityEngine.Event eventData, RectTransform rectTran, out Vector2 local_normalized_pos)
        {
            Camera cam = null;

            Vector2 screen_pos = eventData.mousePosition;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTran, screen_pos, cam, out var local_pos))
            {
                local_normalized_pos = Vector2.zero;
                return false;
            }

            Rect rect = rectTran.rect;
            local_normalized_pos = Rect.PointToNormalized(rect, local_pos);
            return true;
        }
    }
}