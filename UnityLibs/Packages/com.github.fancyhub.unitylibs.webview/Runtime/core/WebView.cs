/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/2/4
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public interface IWebView : IDisposable
    {
        public delegate void OnNavigated(string uri);
        public delegate void OnCanGoForwardUpdated(bool value);
        public delegate void OnCanGoBackUpdated(bool value);
        public delegate void OnNewWindowRequested(string uri);
        public delegate void OnCloseRequested();
        public delegate void OnPostMessage(string message);
        public delegate void OnTextureReady(Texture2D texture);
        public delegate void OnReady();

        public OnNavigated Navigated { get; set; }
        public OnCanGoForwardUpdated CanGoForwardUpdated { get; set; }
        public OnCanGoBackUpdated CanGoBackUpdated { get; set; }
        public OnPostMessage MessageReceived { get; set; }
        public OnNewWindowRequested NewWindowRequested { get; set; }
        public OnCloseRequested WindowCloseRequested { get; set; }
        public OnReady Ready { get; set; }
        public OnTextureReady TextureReady { get; set; }

        public Vector2Int Size { get; }
        public void Resize(int width, int height);
        public void Navigate(string url);
        public void LoadHTMLContent(string htmlContent);
        public void GoBack();
        public void GoForward();
        public void PostMessage(string message, bool isJSON = false);
        public void InvokeScript(string script);
    }    
}