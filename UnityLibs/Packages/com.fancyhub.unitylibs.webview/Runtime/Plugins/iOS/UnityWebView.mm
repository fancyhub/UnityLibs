/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/1/9
 * Title   : 
 * Desc    : 
*************************************************************************************/

#import <Foundation/Foundation.h>
#import <WebKit/WebKit.h>
#import <SafariServices/SafariServices.h>
#import "UnityAppController.h"


@interface MyWebView: NSObject

@property NSNumber * _Nonnull WebViewId;
@property (nonatomic, strong) WKWebView* _WebView;

+(MyWebView*) Get:(int)webViewId;

@end


static NSString* JavaScriptHostName = nil;
static NSString* GlobalUserAgent = nil;
static UIColor* GlobalBGColor = nil; 

static bool GlobalScaling = true;
static bool HasGlobalScaling=false;

typedef void (*WebViewInternalLogCallBack)(int logLvl, const char* msg);
static WebViewInternalLogCallBack s_WebViewInternalLogCallBack = NULL;

typedef void (*WebViewEventCallBack)(int webViewId, int eventType);
static WebViewEventCallBack s_WebViewEventCallBack = NULL;

typedef void (*WebViewJsLogCallBack)(int webViewId, int logLvl, const char* msg);
static WebViewJsLogCallBack s_WebViewJsLogCallBack = NULL;

typedef void (*WebViewJsMessageCallBack)(int webViewId, const char* msg);
static WebViewJsMessageCallBack s_WebViewJsMessageCallBack = NULL;


extern "C"
{
    void Init(const char* _Nonnull jsHostName)
    {
        JavaScriptHostName = [NSString stringWithUTF8String:jsHostName];
    }

    void SetGlobalBGColor(float r, float g, float b, float a)
    {
        GlobalBGColor = [UIColor colorWithRed:r green:g blue:b alpha:a];        
    }

    void SetGlobalUserAgent(const char* userAgent)
    {
        if(userAgent == NULL)
        {
            GlobalUserAgent = nil;
        }
        else
        {
            GlobalUserAgent = [NSString stringWithUTF8String:userAgent];
        }
    }

    void SetGlobalScaling(bool scaling)
    {
        HasGlobalScaling = true;
        GlobalScaling = scaling;
    }

    void SetWebViewInternalLogCallBack(WebViewInternalLogCallBack logCallBack)
    {
        s_WebViewInternalLogCallBack = logCallBack;
    }
    void SetWebViewEventCallBack(WebViewEventCallBack eventCallBack)
    {
        s_WebViewEventCallBack = eventCallBack;
    }

    void SetWebViewJsLogCallBack(WebViewJsLogCallBack logCallBack)
    {
        s_WebViewJsLogCallBack = logCallBack;
    }

    void SetWebViewJsMessageCallBack(WebViewJsMessageCallBack messageCallBack)
    {
        s_WebViewJsMessageCallBack = messageCallBack;
    }

    int Create(const char *url, float x, float y, float width, float height)
    {
        return 0;
    }

    void Close(int webViewId)
    {
    }

    void CloseAll()
    {
    }

    const char *GetUrl(int webViewId)
    {
         return nil;
    }

    void GoBack(int webViewId)
    {
    }

    void GoForward(int webViewId)
    {
    }

    void Navigate(int webViewId, const char *url)
    {
    }

    bool IsLoading(int webViewId)
    {
        return false;
    }

    void Reload(int webViewId)
    {
    }    

    void Resize(int webViewId, float x, float y, float width, float height)
    {
    }

    void SetVisible(int webViewId, bool visible)
    {
    }

    bool IsVisible(int webViewId)
    {
        return false;
    }

    void SetBGColor(int webViewId, float r, float g, float b, float a)
    {
    }

    void RunJsCode(int webViewId, const char* jsCode)
    {
    }
}


@implementation MyWebView
static NSMutableDictionary<NSNumber *, MyWebView *> *s_WebViews =[[NSMutableDictionary alloc] init];
+(MyWebView*) Get:(int)webViewId
{
    NSNumber *nsWebViewId = [NSNumber numberWithInt:webViewId];
    return [s_WebViews objectForKey:nsWebViewId];
}

@end // MyWebView