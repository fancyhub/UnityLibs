/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/29
 * Title   :
 * Desc    :
*************************************************************************************/

#pragma once

#ifdef WEBVIEW2UNITYPLUGIN_EXPORTS
#define WEBVIEW2UNITYPLUGIN_API __declspec(dllexport)
#else
#define WEBVIEW2UNITYPLUGIN_API __declspec(dllimport)
#endif

#include <Windows.h>
#include <wtypes.h>


// 导出函数声明
extern "C" {

	// 消息接收回调函数类型
	typedef WEBVIEW2UNITYPLUGIN_API void(*WebViewMessageCallback)(INT32 webViewId, BSTR message);

	enum ELogLevel
	{
		ELogLevel_Debug = 0,
		ELogLevel_Info = 1,
		ELogLevel_Warning = 2,
		ELogLevel_Error = 3,
		ELogLevel_Off = 4,
	};

	typedef WEBVIEW2UNITYPLUGIN_API void(*WebViewJsLogCallBack)(INT32 webViewId, BSTR message);

	typedef WEBVIEW2UNITYPLUGIN_API void(*WebViewInnerLogCallBack)(ELogLevel logLevel, BSTR message);

	WEBVIEW2UNITYPLUGIN_API void  WebViewSetUserAgent(const wchar_t* userAgent);
	// 设置消息回调函数
	WEBVIEW2UNITYPLUGIN_API void  WebViewSetMessageCallback(WebViewMessageCallback callback);

	WEBVIEW2UNITYPLUGIN_API void  WebViewSetJsLogCallBack(WebViewJsLogCallBack callback);

	WEBVIEW2UNITYPLUGIN_API void  WebViewSetInnerLogCallBack(WebViewInnerLogCallBack callback, ELogLevel maxLogLvl);

	WEBVIEW2UNITYPLUGIN_API void  WebViewSetHostObjName(const wchar_t* hostObjName);
	WEBVIEW2UNITYPLUGIN_API bool  WebViewIsLoading(INT32 webViewId);


	// 创建WebView实例
	WEBVIEW2UNITYPLUGIN_API INT32  WebViewCreate(HWND parentWindow, const wchar_t* url, float posX, float posY, float width, float height);

	WEBVIEW2UNITYPLUGIN_API void  WebViewSetParam(INT32 webViewId, BYTE bgR, BYTE bgG, BYTE bgB, BYTE bgA, bool scaling);

	// 销毁WebView实例
	WEBVIEW2UNITYPLUGIN_API void  WebViewClose(INT32 webViewId);
	WEBVIEW2UNITYPLUGIN_API void  WebViewCloseAll();

	// 调整WebView大小
	WEBVIEW2UNITYPLUGIN_API void  WebViewResize(INT32 webViewId, float x, float y, float width, float height);

	// 导航到指定URL
	WEBVIEW2UNITYPLUGIN_API void  WebViewNavigate(INT32 webViewId, const wchar_t* url);

	// 执行JavaScript
	WEBVIEW2UNITYPLUGIN_API void  WebViewExecuteScript(INT32 webViewId, const wchar_t* script);


	WEBVIEW2UNITYPLUGIN_API BSTR WebViewGetUrl(INT32 webViewId);
	WEBVIEW2UNITYPLUGIN_API void  WebViewReload(INT32 webViewId);

	WEBVIEW2UNITYPLUGIN_API bool WebViewCanGoBack(INT32 webViewId);
	WEBVIEW2UNITYPLUGIN_API void  WebViewGoBack(INT32 webViewId);

	WEBVIEW2UNITYPLUGIN_API bool WebViewCanGoForward(INT32 webViewId);
	WEBVIEW2UNITYPLUGIN_API void  WebViewGoForward(INT32 webViewId);

	WEBVIEW2UNITYPLUGIN_API void  WebViewSetVisible(INT32 webViewId, bool visible);
	WEBVIEW2UNITYPLUGIN_API bool WebViewIsVisible(INT32 webViewId);

	WEBVIEW2UNITYPLUGIN_API bool WebViewIsValid(INT32 webViewId);

	WEBVIEW2UNITYPLUGIN_API bool  WebViewIsLoading(INT32 webViewId);
}
