using System;
using System.Runtime.InteropServices;

namespace Microsoft.MixedReality.WebView
{
    using WebViewInstancePtr = System.IntPtr;
    using WebViewTexturePtr = System.IntPtr;

    internal static class WebViewNative
    {
        private const string DLL_NAME = "MicrosoftWebViewUnityPlugin";

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct WebViewWindowFeatures
        {
            public bool HasSize;
            public uint Height;
            public uint Width;
        };

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TextureAvailableDelegate([In] WebViewInstancePtr instanceId, [In]WebViewTexturePtr texturePtr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void UrlChangedDelegate([In] WebViewInstancePtr instanceId, [In][MarshalAs(UnmanagedType.LPWStr)] string url);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PostMessageToUnityDelegate([In] WebViewInstancePtr instanceId, [In][MarshalAs(UnmanagedType.LPWStr)] string message);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void NavigationButtonStatusUpdatedDelegate([In] WebViewInstancePtr instanceId, [In] bool enabled);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void WindowCloseRequestedDelegate([In] WebViewInstancePtr instanceId);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void NewWindowRequestedDelegate([In] WebViewInstancePtr instanceId, [In][MarshalAs(UnmanagedType.LPWStr)] string uri);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void OnReadyDelegate([In] WebViewInstancePtr instanceId);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void ActivateKeyboard();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void DeactivateKeyboard();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetWebViewUrl(WebViewInstancePtr instanceId, string url);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void GoBackOnWebView(WebViewInstancePtr instanceId);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void GoForwardOnWebView(WebViewInstancePtr instanceId);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern WebViewInstancePtr InitializeWebView(int w, int h);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern WebViewTexturePtr GetOutputTexture(WebViewInstancePtr instanceId);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void DestroyWebView(WebViewInstancePtr instanceId);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetWebViewSize(WebViewInstancePtr instanceId, int w, int h);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetWebViewContentScale(WebViewInstancePtr instanceId, double scale);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetRenderEventFunc();

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetTextureAvailableCallback(WebViewInstancePtr instanceId, TextureAvailableDelegate callback);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetUrlChangedCallback(WebViewInstancePtr instanceId, UrlChangedDelegate callback);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetCanGoBackUpdatedCallback(WebViewInstancePtr instanceId, NavigationButtonStatusUpdatedDelegate callback);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetCanGoForwardUpdatedCallback(WebViewInstancePtr instanceId, NavigationButtonStatusUpdatedDelegate callback);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetNewWindowRequestedCallback(WebViewInstancePtr instanceId, NewWindowRequestedDelegate callback);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetWindowCloseRequestedCallback(WebViewInstancePtr instanceId, WindowCloseRequestedDelegate callback);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetReadyCallback(WebViewInstancePtr instanceId, OnReadyDelegate callback);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void InvokeScript(WebViewInstancePtr instanceId, string script);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void HandlePointerInput(WebViewInstancePtr instanceId, int x, int y, int device, int pointerEvent, int pointerButton, int mouseWheel);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetPostMessageCallback(WebViewInstancePtr instanceId, PostMessageToUnityDelegate callback);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void PostWebMessage(WebViewInstancePtr instanceId, string url, bool isJSON);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void LoadHTMLContent(WebViewInstancePtr instanceId, string htmlContent);

        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetVirtualHostMapping(WebViewInstancePtr instanceId, string hostName, string folderPath);
        
        [DllImport(DLL_NAME, CallingConvention = CallingConvention.StdCall)]
        public static extern void ReleaseFocus(WebViewInstancePtr instanceId);
    }
}