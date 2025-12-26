/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/09/29
 * Title   :
 * Desc    :
*************************************************************************************/


#include "pch.h"
#include <wrl.h>
#include "WebView2UnityPlugin.h"
#include <string>
#include <map>
#include <mutex>
#include <vector>

using namespace Microsoft::WRL;

std::wstring Utf8ToWide(const char* utf8) {
	if (!utf8) return L"";
	int len = MultiByteToWideChar(CP_UTF8, 0, utf8, -1, nullptr, 0);
	wchar_t* buf = new wchar_t[len];
	MultiByteToWideChar(CP_UTF8, 0, utf8, -1, buf, len);
	std::wstring result(buf);
	delete[] buf;
	return result;
}

// 将宽字符转换为UTF-8
std::string WideToUtf8(LPCWSTR wide) {
	int len = WideCharToMultiByte(CP_UTF8, 0, wide, -1, nullptr, 0, nullptr, nullptr);
	char* buf = new char[len + 1];
	WideCharToMultiByte(CP_UTF8, 0, wide, -1, buf, len, nullptr, nullptr);
	buf[len] = '\0';
	std::string result(buf);
	delete[] buf;
	return result;
}

static ComPtr<ICoreWebView2Environment> globalEnv;
class MyWebView;
static std::map<INT32, MyWebView*> globalWebViewMap;
static std::mutex globalWebViewMutex;

static std::wstring globalUserAgent;
static std::wstring globalHostObjName;
static WebViewMessageCallback globalMessageCallback = nullptr;
static WebViewJsLogCallBack globalJsLogCallback = nullptr;
static WebViewInnerLogCallBack globalInnerLogCallback = nullptr;
static WebViewEventCallBack globalEventCallBack = nullptr;
static ELogLevel globalInnerLogLevel = ELogLevel::ELogLevel_Debug;


WEBVIEW2UNITYPLUGIN_API void  WebViewSetUserAgent(const wchar_t* userAgent)
{
	globalUserAgent = userAgent;
}

// 设置消息回调
WEBVIEW2UNITYPLUGIN_API void  WebViewSetMessageCallback(WebViewMessageCallback callback)
{
	globalMessageCallback = callback;
}

WEBVIEW2UNITYPLUGIN_API void  WebViewSetEventCallBack(WebViewEventCallBack callback)
{
	globalEventCallBack = callback;
}

WEBVIEW2UNITYPLUGIN_API void  WebViewSetJsLogCallBack(WebViewJsLogCallBack callback)
{
	globalJsLogCallback = callback;
}

WEBVIEW2UNITYPLUGIN_API void  WebViewSetInnerLogCallBack(WebViewInnerLogCallBack callback, ELogLevel maxLogLvl)
{
	globalInnerLogCallback = callback;
	globalInnerLogLevel = maxLogLvl;
}

void Log(ELogLevel logLvl, const wchar_t* fmt, ...)
{
	auto t = globalInnerLogCallback;
	if (t == nullptr)
		return;
	if (logLvl < globalInnerLogLevel)
		return;

	const int MAXCOUNT = 2048;
	wchar_t msgbuf[MAXCOUNT];
	va_list args;
	va_start(args, fmt);

	vswprintf(msgbuf, MAXCOUNT, fmt, args);
	va_end(args);

	t(logLvl, SysAllocString(msgbuf));
}

#define LOG_DEBUG(...) Log(ELogLevel_Debug, __VA_ARGS__)
#define LOG_INFO(...)  Log(ELogLevel_Info,  __VA_ARGS__)
#define LOG_WARN(...)  Log(ELogLevel_Warning,  __VA_ARGS__)
#define LOG_ERROR(...) Log(ELogLevel_Error, __VA_ARGS__)

WEBVIEW2UNITYPLUGIN_API void  WebViewSetHostObjName(const wchar_t* hostObjName)
{
	globalHostObjName = hostObjName;
}

struct WebViewHostObject : public RuntimeClass<RuntimeClassFlags<ClassicCom>, IDispatch >
{
private:
	static const DISPID Method_postMessage = 1;
	static const WCHAR* MethodName_postMessage;
	LONG m_lRefCount = 1;

public:
	INT32 WebViewId;
	std::wstring HostObjName;

public:
	WebViewHostObject() = default;
	~WebViewHostObject() override = default;

	// 禁止拷贝构造和赋值（COM 对象通常单实例管理）
	WebViewHostObject(const WebViewHostObject&) = delete;
	WebViewHostObject& operator=(const WebViewHostObject&) = delete;

	STDMETHODIMP QueryInterface(REFIID riid, void** ppv)
	{
		if (riid == IID_IUnknown || riid == IID_IDispatch)
		{
			*ppv = static_cast<IDispatch*>(this);
			AddRef();
			return S_OK;
		}
		*ppv = nullptr;
		return E_NOINTERFACE;
	}

	STDMETHODIMP_(ULONG) AddRef()
	{
		return InterlockedIncrement(&m_lRefCount);
	}

	STDMETHODIMP_(ULONG) Release()
	{
		LONG lCount = InterlockedDecrement(&m_lRefCount);
		if (lCount == 0)
		{
			delete this;
		}
		return lCount;
	}

	STDMETHODIMP GetTypeInfoCount(UINT* pctinfo)
	{
		*pctinfo = 0;
		return S_OK;
	}

	STDMETHODIMP GetTypeInfo(UINT iTInfo, LCID lcid, ITypeInfo** ppTInfo)
	{
		*ppTInfo = nullptr;
		return E_NOTIMPL;
	}

	STDMETHODIMP GetIDsOfNames(REFIID riid, LPOLESTR* rgszNames, UINT cNames, LCID lcid, DISPID* rgDispId)
	{
		// 只处理单个方法名的情况
		if (cNames != 1 || !rgszNames || !rgDispId)
			return E_INVALIDARG;

		// 映射 "postMessage" 方法到 DISPID 1（自定义唯一标识）
		if (_wcsicmp(rgszNames[0], MethodName_postMessage) == 0)
		{
			*rgDispId = Method_postMessage;
			return S_OK;
		}

		// 方法名未找到
		return DISP_E_UNKNOWNNAME;
	}

	STDMETHOD(Invoke)(DISPID dispIdMember, REFIID riid, LCID lcid, WORD wFlags, DISPPARAMS* pDispParams, VARIANT* pVarResult, EXCEPINFO* pExcepInfo, UINT* puArgErr) override
	{
		if (!pDispParams)
			return E_INVALIDARG;

		switch (dispIdMember)
		{
		default:
			return DISP_E_MEMBERNOTFOUND;

		case Method_postMessage:
			if (pDispParams->cArgs != 1)
				return DISP_E_BADPARAMCOUNT; // 参数数量错误			

			VARIANT* pArg = &pDispParams->rgvarg[0];
			if (pArg->vt != VT_BSTR)
				return DISP_E_TYPEMISMATCH; // 参数类型错误

			if (globalMessageCallback == nullptr)
				return DISP_E_UNKNOWNNAME;

			globalMessageCallback(WebViewId, SysAllocString(pArg->bstrVal));
			return S_OK;
		}
	}

};

const WCHAR* WebViewHostObject::MethodName_postMessage = L"postMessage";

struct WebViewSize
{
public:
	float PosX;
	float PosY;
	float Width;
	float Height;

public:
	WebViewSize(float posX, float posY, float width, float height)
		:PosX(posX), PosY(posY), Width(width), Height(height)
	{
	}

	RECT CalcRect(HWND hWnd)
	{
		RECT clientRect;
		GetClientRect(hWnd, &clientRect);
		auto width = clientRect.right - clientRect.left;
		auto height = clientRect.bottom - clientRect.top;

		LOG_DEBUG(L"WebViewSize CalcRect , parentSize: %d,%d,%d,%d", clientRect.left, clientRect.right, clientRect.top, clientRect.bottom);

		INT32 posXI32 = (INT32)(PosX * width);
		INT32 posYI32 = (INT32)(PosY * height);
		INT32 widthI32 = (INT32)(Width * width);
		INT32 heightI32 = (INT32)(Height * height);
		return { posXI32,posYI32, posXI32 + widthI32,posYI32 + heightI32 };
	}
};

template<typename T>
struct MyWebViewParam
{
public:
	bool HasValue;
	T Value;

	MyWebViewParam() :HasValue(false)
	{
	}

	void SetValue(const T& value)
	{
		this->Value = value;
		this->HasValue = true;
	}
};

class MyWebView
{
private:
	static INT32 WebViewIdGen;

public:
	INT32 WebViewId;
	std::wstring URL;
	WebViewSize Size;
	HWND ParentWindow;
	RECT LastRect;
	bool Visible;
	MyWebViewParam<bool> ParamScaling;
	MyWebViewParam<COREWEBVIEW2_COLOR> ParamBGColor;

	ComPtr<ICoreWebView2> pWebView2;
	ComPtr<ICoreWebView2Controller> pController;
	ComPtr<ICoreWebView2Environment> pEnv;
	ComPtr<WebViewHostObject> pHostObj;
	ComPtr<ICoreWebView2DevToolsProtocolEventReceiver> pDevToolReceiver;

	EventRegistrationToken Token_ConsoleObj;
	EventRegistrationToken Token_NavStarting;
	EventRegistrationToken Token_NavCompleted;
	EventRegistrationToken Token_WebMessageReceived;
	EventRegistrationToken Token_NewWindowRequested;

	bool IsLoading;

public:
	MyWebView(HWND parentWindow, const wchar_t* url, const WebViewSize& size)
		:WebViewId(WebViewIdGen++), ParentWindow(parentWindow),
		URL(url), Size(size), LastRect({ 0,0,0,0 }),
		IsLoading(false), Visible(true), ParamScaling(), ParamBGColor()
	{
		pHostObj = Make<WebViewHostObject>();
		pHostObj->HostObjName = globalHostObjName;
		pHostObj->WebViewId = WebViewId;


		Token_NavStarting.value = 0;
		Token_NavCompleted.value = 0;
		Token_WebMessageReceived.value = 0;
		Token_NewWindowRequested.value = 0;
		Token_ConsoleObj.value = 0;
	}

	virtual ~MyWebView()
	{
		Destroy();
	}

	RECT CalcRect()
	{
		return Size.CalcRect(ParentWindow);
	}

	bool SetLast(RECT rect)
	{
		if (LastRect.left == rect.left && LastRect.right == rect.right && LastRect.top == rect.top && LastRect.bottom == rect.bottom)
			return false;
		LastRect = rect;
		return true;
	}

	void Destroy()
	{
		if (pDevToolReceiver != nullptr)
		{
			if (Token_ConsoleObj.value != 0) pDevToolReceiver->remove_DevToolsProtocolEventReceived(Token_ConsoleObj);
			pDevToolReceiver = nullptr;
		}

		if (pWebView2 != nullptr)
		{
			if (Token_NavStarting.value != 0)pWebView2->remove_NavigationStarting(Token_NavStarting);
			if (Token_NavCompleted.value != 0)pWebView2->remove_NavigationCompleted(Token_NavCompleted);
			if (Token_WebMessageReceived.value != 0)pWebView2->remove_WebMessageReceived(Token_WebMessageReceived);
			if (Token_NewWindowRequested.value != 0)pWebView2->remove_NewWindowRequested(Token_NewWindowRequested);


			pWebView2 = nullptr;
		}

		if (pController != nullptr)
		{
			// 可选：清除父窗口关联
			pController->put_ParentWindow(nullptr);

			// 释放控制器接口
			pController = nullptr;
		}

		if (pEnv != nullptr)
		{
			pEnv = nullptr;
		}

		if (pHostObj != nullptr)
		{
			pHostObj = nullptr;
		}
	}
};

INT32 MyWebView::WebViewIdGen = 1;

MyWebView* _CreateViewView(HWND parentWindow, const wchar_t* url, const WebViewSize& size)
{
	MyWebView* pWebView = new MyWebView(parentWindow, url, size);
	{
		std::lock_guard<std::mutex> lock(globalWebViewMutex);
		globalWebViewMap[pWebView->WebViewId] = pWebView;
	}

	return pWebView;
}

MyWebView* _FindWebView(INT32 webViewId, const wchar_t* errorMsg = nullptr)
{
	std::lock_guard<std::mutex> lock(globalWebViewMutex);
	auto it = globalWebViewMap.find(webViewId);
	if (it != globalWebViewMap.end())
		return it->second;

	if (errorMsg != nullptr)
		LOG_WARN(L"WebView is not exist, WebViewId:%d, %s", webViewId, errorMsg);
	return nullptr;
}



void _DestroyWebView(INT32 webViewId)
{
	MyWebView* pWebView = nullptr;
	{
		std::lock_guard<std::mutex> lock(globalWebViewMutex);
		auto it = globalWebViewMap.find(webViewId);
		if (it == globalWebViewMap.end())
			return;
		pWebView = it->second;
		globalWebViewMap.erase(it);
	}

	pWebView->Destroy();
	delete pWebView;

	if (globalEventCallBack != nullptr)
	{
		globalEventCallBack(webViewId, EWebViewEvent_Destroyed);
	}
}

// WebView消息接收处理
void _OnWebMessageReceived(INT32 webViewId, ICoreWebView2* sender, ICoreWebView2WebMessageReceivedEventArgs* args)
{
	if (!globalMessageCallback) return;

	LPWSTR message = nullptr;
	if (SUCCEEDED(args->TryGetWebMessageAsString(&message)))
	{
		globalMessageCallback(webViewId, SysAllocString(message));
		CoTaskMemFree(message);
	}
}

void _OnNavStarting(INT32 webViewId, ICoreWebView2* sender, ICoreWebView2NavigationStartingEventArgs* args)
{
	auto pWebView = _FindWebView(webViewId);
	if (pWebView == nullptr)
		return;
	pWebView->IsLoading = true;
}

void _OnNavCompleted(INT32 webViewId, ICoreWebView2* sender, ICoreWebView2NavigationCompletedEventArgs* args)
{
	auto pWebView = _FindWebView(webViewId);
	if (pWebView == nullptr)
		return;
	pWebView->IsLoading = false;

	if (globalEventCallBack != nullptr)
	{
		globalEventCallBack(webViewId, EWebViewEvent_DocumentReady);
	}
}

// 新窗口请求事件处理
HRESULT CALLBACK _OnNewWindowRequested(ICoreWebView2* sender, ICoreWebView2NewWindowRequestedEventArgs* args)
{
	args->put_Handled(TRUE);

	// 在当前窗口导航到目标URL
	LPWSTR targetUrl;
	if (SUCCEEDED(args->get_Uri(&targetUrl)))
	{
		sender->Navigate(targetUrl);
		CoTaskMemFree(targetUrl);
	}
	return S_OK;
}

void _SetUserAgent(ICoreWebView2* pWebView, const std::wstring& userAgent)
{
	if (pWebView == nullptr)
		return;
	if (userAgent.length() == 0)
		return;

	ComPtr<ICoreWebView2Settings> settings;
	HRESULT hr = pWebView->get_Settings(&settings);
	if (FAILED(hr))
		return;

	ComPtr<ICoreWebView2Settings2> settings2;
	if (SUCCEEDED(settings.As(&settings2)))
	{
		settings2->put_UserAgent(userAgent.c_str());
	}
}

void _SetHostObj(ICoreWebView2* pWebView, const ComPtr<WebViewHostObject>& pHostObj)
{
	if (pHostObj->HostObjName.length() == 0)
		return;

	ComPtr<ICoreWebView2Settings> settings;
	HRESULT hrSetting = pWebView->get_Settings(&settings);
	if (FAILED(hrSetting))
		return;
	hrSetting = settings->put_AreHostObjectsAllowed(TRUE);

	VARIANT var;
	VariantInit(&var);
	var.vt = VT_DISPATCH;
	var.pdispVal = nullptr;

	IDispatch* pDisp = nullptr;
	HRESULT hrQI = pHostObj->QueryInterface(IID_PPV_ARGS(&pDisp)); // pDisp 会被 AddRef
	if (SUCCEEDED(hrQI) && pDisp)
	{
		var.pdispVal = pDisp; // pDisp 已经 AddRef 过
	}
	else
	{
		// fallback：把 IUnknown 放入 VT_UNKNOWN
		VariantClear(&var);
		VariantInit(&var);
		var.vt = VT_UNKNOWN;
		var.punkVal = pHostObj.Get();
		if (var.punkVal) var.punkVal->AddRef(); // 增加引用计数
	}

	// 调用 AddHostObjectToScript（适用于接受 VARIANT* 的签名）
	HRESULT hr = pWebView->AddHostObjectToScript(pHostObj->HostObjName.c_str(), &var);
	if (FAILED(hr))
	{
		// 处理失败
		OutputDebugString(L"AddHostObjectToScript failed\n");
	}

	// 释放本地 VARIANT（会 Release 我们为 VARIANT 添加的引用）
	VariantClear(&var);
}

void _AppWebViewParams(MyWebView* pWebView)
{
	if (pWebView == nullptr || pWebView->pWebView2 == nullptr)
		return;


	ComPtr<ICoreWebView2Controller2> controller2;
	if (pWebView->ParamBGColor.HasValue && pWebView->pController != nullptr && SUCCEEDED(pWebView->pController.As(&controller2)))
	{
		controller2->put_DefaultBackgroundColor(pWebView->ParamBGColor.Value);
	}

	ComPtr<ICoreWebView2Settings> settings;
	if (pWebView->ParamScaling.HasValue && SUCCEEDED(pWebView->pWebView2->get_Settings(&settings)))
	{
		settings->put_IsZoomControlEnabled(pWebView->ParamScaling.Value);

		ComPtr<ICoreWebView2Settings5> settings5;
		if (SUCCEEDED(settings.As(&settings5)))
		{
			settings5->put_IsPinchZoomEnabled(pWebView->ParamScaling.Value);
		}
	}
}


// 控制器创建完成回调
HRESULT _OnCreateWebViewCompleted(HRESULT result, ICoreWebView2Controller* controller, INT32 webViewId)
{
	if (result == S_OK)
		LOG_DEBUG(L"OnCreateWebViewCompleted webviewId:%d, result:%d", webViewId, result);
	else
		LOG_ERROR(L"OnCreateWebViewCompleted webviewId:%d, result:%d", webViewId, result);

	auto pWebView = _FindWebView(webViewId, L"OnCreateWebViewCompleted");
	if (pWebView == nullptr)
	{
		if (controller != nullptr)
		{
			controller->Release();
		}
		return -1;
	}

	if (FAILED(result))
	{
		_DestroyWebView(webViewId);
		return result;
	}

	pWebView->pController = controller;

	ICoreWebView2* webView;
	controller->get_CoreWebView2(&webView);
	pWebView->pWebView2 = webView;

	_SetUserAgent(webView, globalUserAgent);
	_SetHostObj(webView, pWebView->pHostObj);
	_AppWebViewParams(pWebView);


	RECT rect = pWebView->CalcRect();
	LOG_DEBUG(L"OnCreateWebViewCompleted SetSize %d,%d,%d,%d", rect.left, rect.right, rect.top, rect.bottom);
	if (pWebView->SetLast(rect))
		controller->put_Bounds(rect);

	webView->add_NewWindowRequested(
		Callback<ICoreWebView2NewWindowRequestedEventHandler>(
			_OnNewWindowRequested
		).Get(),
		&pWebView->Token_NewWindowRequested
	);

	// 注册消息接收事件
	webView->add_WebMessageReceived(
		Callback<ICoreWebView2WebMessageReceivedEventHandler>(
			[webViewId](ICoreWebView2* sender, ICoreWebView2WebMessageReceivedEventArgs* args) -> HRESULT {
				_OnWebMessageReceived(webViewId, sender, args);
				return S_OK;
			}).Get(),
				&pWebView->Token_WebMessageReceived
				);

	webView->add_NavigationCompleted(
		Callback<ICoreWebView2NavigationCompletedEventHandler>(
			[webViewId](ICoreWebView2* sender, ICoreWebView2NavigationCompletedEventArgs* args) -> HRESULT {
				_OnNavCompleted(webViewId, sender, args);
				return S_OK;
			}).Get(),
				&pWebView->Token_NavCompleted
				);

	webView->add_NavigationStarting(
		Callback<ICoreWebView2NavigationStartingEventHandler>(
			[webViewId](ICoreWebView2* sender, ICoreWebView2NavigationStartingEventArgs* args) -> HRESULT {
				_OnNavStarting(webViewId, sender, args);
				return S_OK;
			}).Get(),
				&pWebView->Token_NavStarting
				);


	HRESULT hr = webView->GetDevToolsProtocolEventReceiver(L"Runtime.consoleAPICalled", &pWebView->pDevToolReceiver);
	if (SUCCEEDED(hr))
	{
		hr = pWebView->pDevToolReceiver->add_DevToolsProtocolEventReceived(Callback<ICoreWebView2DevToolsProtocolEventReceivedEventHandler>(
			[webViewId](ICoreWebView2* sender, ICoreWebView2DevToolsProtocolEventReceivedEventArgs* args) -> HRESULT
			{
				if (globalJsLogCallback == nullptr) return S_OK;

				LPWSTR message;
				if (SUCCEEDED(args->get_ParameterObjectAsJson(&message)))
				{
					globalJsLogCallback(webViewId, SysAllocString(message));
					CoTaskMemFree(message);
				}
				return S_OK;
			}
		).Get(), &pWebView->Token_ConsoleObj);

		if (SUCCEEDED(hr))
		{
			webView->CallDevToolsProtocolMethod(L"Runtime.enable", L"{}", nullptr);
		}
	}

	LOG_DEBUG(L"OnCreateWebViewCompleted Navigate %s", pWebView->URL.c_str());
	webView->Navigate(pWebView->URL.c_str());
	return S_OK;
}

// 环境创建完成回调
HRESULT _OnCreateEnvironmentCompleted(HRESULT result, ICoreWebView2Environment* env)
{
	if (globalEnv != nullptr)
		return result;

	if (!SUCCEEDED(result))
	{
		LOG_ERROR(L"OnCreateEnvironmentCompleted result:%d", result);
		WebViewCloseAll();
		return result;
	}
	globalEnv = env;
	LOG_DEBUG(L"OnCreateEnvironmentCompleted result:%d", result);
	std::vector<MyWebView*> all;
	{
		std::lock_guard<std::mutex> lock(globalWebViewMutex);
		for (auto it : globalWebViewMap)
		{
			all.push_back(it.second);
		}
	}

	for (auto pWebView : all)
	{
		pWebView->pEnv = env;
		INT32 webViewId = pWebView->WebViewId;

		env->CreateCoreWebView2Controller(
			pWebView->ParentWindow,
			Callback<ICoreWebView2CreateCoreWebView2ControllerCompletedHandler>(
				[webViewId](HRESULT result, ICoreWebView2Controller* controller) -> HRESULT {
					return _OnCreateWebViewCompleted(result, controller, webViewId);
				}).Get()
					);
	}
	return S_OK;
}

void _InitEnv()
{
	static bool inited = false;
	if (!inited)	// 初始化COM
	{
		inited = true;
		CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED);
	}

	if (globalEnv == nullptr)
	{
		// 创建WebView2环境
		CreateCoreWebView2EnvironmentWithOptions(
			nullptr,
			nullptr,
			nullptr,
			Callback<ICoreWebView2CreateCoreWebView2EnvironmentCompletedHandler>(
				[](HRESULT result, ICoreWebView2Environment* env) -> HRESULT {
					return _OnCreateEnvironmentCompleted(result, env);
				}).Get());
	}
}

// 创建WebView
WEBVIEW2UNITYPLUGIN_API INT32 WebViewCreate(HWND parentWindow, const wchar_t* url, float posX, float posY, float width, float height)
{
	_InitEnv();
	auto pWebView = _CreateViewView(parentWindow, url, WebViewSize(posX, posY, width, height));

	if (globalEnv != nullptr)
	{
		pWebView->pEnv = globalEnv;
		INT32 webViewId = pWebView->WebViewId;

		globalEnv->CreateCoreWebView2Controller(
			pWebView->ParentWindow,
			Callback<ICoreWebView2CreateCoreWebView2ControllerCompletedHandler>(
				[webViewId](HRESULT result, ICoreWebView2Controller* controller) -> HRESULT {
					return _OnCreateWebViewCompleted(result, controller, webViewId);
				}).Get()
					);
	}
	return  pWebView->WebViewId;
}

WEBVIEW2UNITYPLUGIN_API void  WebViewSetBGColor(INT32 webViewId, BYTE bgR, BYTE bgG, BYTE bgB, BYTE bgA)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewSetParam");
	if (pWebView == nullptr)
		return;

	COREWEBVIEW2_COLOR color;

	color.R = bgR;
	color.G = bgG;
	color.B = bgB;
	color.A = bgA;

	pWebView->ParamBGColor.SetValue(color);
	_AppWebViewParams(pWebView);
}

WEBVIEW2UNITYPLUGIN_API void WebViewSetScaling(INT32 webViewId, bool scaling)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewSetParam");
	if (pWebView == nullptr)
		return;

	pWebView->ParamScaling.SetValue(scaling);
	_AppWebViewParams(pWebView);
}

// 销毁WebView
WEBVIEW2UNITYPLUGIN_API void  WebViewClose(INT32 webViewId)
{
	_DestroyWebView(webViewId);
}

WEBVIEW2UNITYPLUGIN_API void  WebViewCloseAll()
{
	std::vector<MyWebView*> all;
	{
		std::lock_guard<std::mutex> lock(globalWebViewMutex);
		for (auto it : globalWebViewMap)
		{
			all.push_back(it.second);
		}
		globalWebViewMap.clear();
	}

	for (auto it : all)
	{
		it->Destroy();
	}
}

// 调整WebView大小
WEBVIEW2UNITYPLUGIN_API void  WebViewResize(INT32 webViewId, float posX, float posY, float width, float height)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewResize");
	if (pWebView == nullptr)
		return;

	pWebView->Size = WebViewSize(posX, posY, width, height);
	RECT rect = pWebView->CalcRect();
	if (pWebView->pController != nullptr && pWebView->SetLast(rect))
	{
		pWebView->pController->put_Bounds(rect);
	}
}

// 导航到URL
WEBVIEW2UNITYPLUGIN_API void  WebViewNavigate(INT32 webViewId, const wchar_t* url)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewNavigate");
	if (pWebView == nullptr)
		return;
	pWebView->URL = url;

	if (pWebView->pWebView2 != nullptr)
	{
		pWebView->pWebView2->Navigate(pWebView->URL.c_str());
	}
}


WEBVIEW2UNITYPLUGIN_API BSTR WebViewGetUrl(INT32 webViewId)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewGetUrl");
	if (pWebView == nullptr)
	{
		return nullptr;
	}

	if (pWebView->pWebView2 == nullptr)
	{
		return SysAllocString(pWebView->URL.c_str());
	}

	LPWSTR url;
	HRESULT result = pWebView->pWebView2->get_Source(&url);
	if (!SUCCEEDED(result))
		return  nullptr;

	pWebView->URL = url;
	CoTaskMemFree(url);
	return SysAllocString(pWebView->URL.c_str());
}


WEBVIEW2UNITYPLUGIN_API void  WebViewReload(INT32 webViewId)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewReload");
	if (pWebView == nullptr)
		return;

	pWebView->pWebView2->Reload();
}

WEBVIEW2UNITYPLUGIN_API bool WebViewCanGoBack(INT32 webViewId)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewCanGoBack");
	if (pWebView == nullptr)
		return false;

	BOOL ret;
	pWebView->pWebView2->get_CanGoBack(&ret);
	return ret;
}

WEBVIEW2UNITYPLUGIN_API void  WebViewGoBack(INT32 webViewId)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewGoBack");
	if (pWebView == nullptr)
		return;

	pWebView->pWebView2->GoBack();
}

WEBVIEW2UNITYPLUGIN_API bool WebViewCanGoForward(INT32 webViewId)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewCanGoForward");
	if (pWebView == nullptr)
		return false;

	BOOL ret;
	pWebView->pWebView2->get_CanGoForward(&ret);
	return ret;
}

WEBVIEW2UNITYPLUGIN_API void  WebViewGoForward(INT32 webViewId)
{
	MyWebView* pWebView = _FindWebView(webViewId, L"WebViewGoForward");
	if (pWebView == nullptr)
		return;

	pWebView->pWebView2->GoForward();
}

WEBVIEW2UNITYPLUGIN_API void  WebViewSetVisible(INT32 webViewId, bool visible)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewSetVisible");
	if (pWebView == nullptr)
		return;

	pWebView->Visible = visible;

	if (pWebView->pController != nullptr)
	{
		pWebView->pController->put_IsVisible(visible);
	}
}


WEBVIEW2UNITYPLUGIN_API bool WebViewIsVisible(INT32 webViewId)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewIsVisible");
	if (pWebView == nullptr)
		return false;

	if (pWebView->pController != nullptr)
	{
		BOOL ret = false;
		HRESULT hr = pWebView->pController->get_IsVisible(&ret);
		if (SUCCEEDED(hr))
			return ret;
	}
	return pWebView->Visible;
}

WEBVIEW2UNITYPLUGIN_API bool WebViewIsValid(INT32 webViewId)
{
	auto pWebView = _FindWebView(webViewId);
	return pWebView != nullptr;
}

// 执行JavaScript
WEBVIEW2UNITYPLUGIN_API void  WebViewExecuteScript(INT32 webViewId, const wchar_t* script)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewExecuteScript");
	if (pWebView == nullptr)
		return;

	if (pWebView->pWebView2 != nullptr)
	{
		pWebView->pWebView2->ExecuteScript(script, nullptr);
	}
}

WEBVIEW2UNITYPLUGIN_API bool  WebViewIsLoading(INT32 webViewId)
{
	auto pWebView = _FindWebView(webViewId, L"WebViewIsLoading");
	if (pWebView == nullptr)
		return false;
	return pWebView->IsLoading;
}

