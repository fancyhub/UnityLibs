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

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWebViewData
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@interface MyWebViewData : NSObject
@property NSNumber * _Nonnull WebViewId;
@property UIColor* BGColor;

@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWKNavigationDelegate
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@interface MyWKNavigationDelegate: NSObject<WKNavigationDelegate>
@property  MyWebViewData* _Nonnull _Data;
-(instancetype)initWithData:(MyWebViewData*)data;
-(void)webView:(WKWebView *)webView decidePolicyForNavigationAction:(WKNavigationAction *)navigationAction decisionHandler:(void (^)(WKNavigationActionPolicy))decisionHandler;
-(void)webView:(WKWebView *)webView didFinishNavigation:(WKNavigation *)navigation;
-(void)webView:(WKWebView *)webView didFailNavigation:(WKNavigation *)navigation withError:(NSError *)error;
@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWKUIDelegate
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@interface MyWKUIDelegate: NSObject<WKUIDelegate>
@property  MyWebViewData* _Nonnull _Data;
-(instancetype)initWithData:(MyWebViewData*)data;
- (WKWebView *)webView:(WKWebView *)webView createWebViewWithConfiguration:(WKWebViewConfiguration *)configuration forNavigationAction:(WKNavigationAction *)navigationAction windowFeatures:(WKWindowFeatures *)windowFeatures;
@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWKScriptMessageHandler
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@interface MyWKScriptMessageHandler: NSObject<WKScriptMessageHandler>
@property  MyWebViewData* _Nonnull _Data;
-(instancetype)initWithData:(MyWebViewData*)data;
-(void)userContentController:(WKUserContentController *)userContentController didReceiveScriptMessage:(WKScriptMessage *)message;
@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWKScriptMessageHandler_JsConsole - 监听 JS console.log/warn/error
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@interface MyWKScriptMessageHandler_JsConsole: NSObject<WKScriptMessageHandler>
@property  MyWebViewData* _Nonnull _Data;
-(instancetype)initWithData:(MyWebViewData*)data;
-(void)userContentController:(WKUserContentController *)userContentController didReceiveScriptMessage:(WKScriptMessage *)message;
@end


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWebView
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@interface MyWebView: NSObject
@property (nonatomic, strong) MyWebViewData* _Data;
@property (nonatomic, strong) WKWebView* _WebView;
@property (nonatomic, strong) MyWKNavigationDelegate* _NavigationDelegate;
@property (nonatomic, strong) MyWKUIDelegate* _UIDelegate;
@property (nonatomic, strong) MyWKScriptMessageHandler* _ScriptMessageHandler;
@property (nonatomic, strong) MyWKScriptMessageHandler_JsConsole* _ScriptMessageHandler_JsConsole;

+(MyWebView*) Get:(int)webViewId;
+(MyWebView*) GetByNsWebView:(NSNumber*)webViewId;
+(int) Create:(NSURL * _Nonnull)url WithArea:(CGRect)area;
+(void) CloseWebView:(int)webViewId;
+(void) CloseAll;
+ (WKUserScript*)GetJsConsoleScript;

-(void)Navigate:(NSURL * _Nonnull)url;  
-(void)GoBack;
-(void)GoForward;
-(void)Reload;
-(void)Resize:(CGRect)area;
-(void)SetVisible:(bool)visible;
-(void)SetBGColor:(UIColor*)color;
-(void)RunJsCode:(NSString*)jsCode;
-(void)Close;
-(NSString*)GetUrl;
-(NSString*)GetTitle;
-(bool)IsLoading;

@end

static NSString* JavaScriptHostName = nil;
static NSString* GlobalUserAgent = nil;
static UIColor* GlobalBGColor = nil; 


extern "C"
{
    static const int LogLevel_Debug = 0;
    static const int LogLevel_Info = 1;
    static const int LogLevel_Warning = 2;
    static const int LogLevel_Error = 3;

    static const int EventType_DocumentReady = 1;
    static const int EventType_Destroyed = 2;
    

    static bool GlobalScaling = true;
    static bool HasGlobalScaling=false;

    typedef void (*WebViewInternalLogCallBack)(int logLvl, const char* msg);
    static WebViewInternalLogCallBack s_WebViewInternalLogCallBack = NULL;

    typedef void (*WebViewEventCallBack)(int webViewId, int eventType);
    static WebViewEventCallBack s_WebViewEventCallBack = NULL;

    typedef void (*WebViewJsLogCallBack)(int webViewId, const char* msg);
    static WebViewJsLogCallBack s_WebViewJsLogCallBack = NULL;

    typedef void (*WebViewJsMessageCallBack)(int webViewId, const char* msg);
    static WebViewJsMessageCallBack s_WebViewJsMessageCallBack = NULL;


    static void _InnerLog(int webViewId, int logLvl, const char* msg)
    {
         if(s_WebViewInternalLogCallBack == NULL)
            return;

        if(webViewId == 0)
        {
            s_WebViewInternalLogCallBack(LogLevel_Debug, msg);
            return;
        }

        const int MAX_MSG_LEN = 1024;
        char buf[MAX_MSG_LEN];
        snprintf(buf, MAX_MSG_LEN, "WebViewId: %d %s", webViewId, msg);
        s_WebViewInternalLogCallBack(logLvl, buf);      
    }

    static void Log_Debug(int webViewId, const char* msg)
    {
        _InnerLog(webViewId, LogLevel_Debug, msg);          
    }

    static void Log_Info(int webViewId, const char* msg)
    {
        _InnerLog(webViewId, LogLevel_Info, msg);          
    }

    static void Log_Warning(int webViewId, const char* msg)
    {
        _InnerLog(webViewId, LogLevel_Warning, msg);          
    }

    static void Log_Error(int webViewId, const char* msg)
    {
        _InnerLog(webViewId, LogLevel_Error, msg); 
    }


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
        UIViewController *unityViewController =[(UnityAppController*)[UIApplication sharedApplication].delegate rootViewController];
        
        float parentWidth = unityViewController.view.frame.size.width;
        float parentHeight = unityViewController.view.frame.size.height;
        x *= parentWidth;
        y *= parentHeight;
        width *= parentWidth;
        height *= parentHeight;
        CGRect area = CGRectMake(x, y, width, height);
        
        NSURL *nsurl = [NSURL URLWithString:[NSString stringWithUTF8String:url]];
        return [MyWebView Create:nsurl WithArea:area];
        
    }

    void Close(int webViewId)
    {
        [MyWebView CloseWebView:webViewId];
    }

    void CloseAll()
    {
        [MyWebView CloseAll];
    }

    const char *GetUrl(int webViewId)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil)
        {
            Log_Error(webViewId, "GetUrl: webView is nil");
            return NULL;
        } 

        const char *url = [[webView GetUrl] UTF8String];
        if (url == NULL) return NULL;
        return strdup(url);
    }

    const char *GetTitle(int webViewId)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil)
        {
            Log_Error(webViewId, "GetTitle: webView is nil");
            return NULL;
        }

        NSString *title = [webView GetTitle];
        if(title == nil) return NULL;
        const char *titleCStr = [title UTF8String];
        if(titleCStr == NULL) return NULL;
        return strdup(titleCStr);
    }

    bool IsLoading(int webViewId)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil) return false;
        return [webView IsLoading];
    }

    void GoBack(int webViewId)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil) return;
        [webView GoBack];
    }

    void GoForward(int webViewId)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil) return;
        [webView GoForward];
    }

    void Navigate(int webViewId, const char *url)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil) return;
        [webView Navigate:[NSURL URLWithString:[NSString stringWithUTF8String:url]]];
    } 

    void Reload(int webViewId)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil) return;
        [webView Reload];
    }    

    void Resize(int webViewId, float x, float y, float width, float height)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil) return;

        UIViewController *unityViewController =[(UnityAppController*)[UIApplication sharedApplication].delegate rootViewController];
        
        float parentWidth = unityViewController.view.frame.size.width;
        float parentHeight = unityViewController.view.frame.size.height;
        x *= parentWidth;
        y *= parentHeight;
        width *= parentWidth;
        height *= parentHeight;
        CGRect area = CGRectMake(x, y, width, height);

        [webView Resize:area];
    }

    void SetVisible(int webViewId, bool visible)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil) return;
        [webView SetVisible:visible];
    }

    bool IsVisible(int webViewId)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil) return false;
        if(webView._WebView == nil) return false;
        return ![webView._WebView isHidden];
    }

    void SetBGColor(int webViewId, float r, float g, float b, float a)
    {
    }

    void RunJsCode(int webViewId, const char* jsCode)
    {
        MyWebView *webView = [MyWebView Get:webViewId];
        if(webView == nil) return;
        [webView RunJsCode:[NSString stringWithUTF8String:jsCode]];
    }
}


 static void _InnerLog2(int webViewId, int logLvl, NSString* msg)
    {
         if(s_WebViewInternalLogCallBack == NULL)
            return;

        if(webViewId == 0)
        {
            s_WebViewInternalLogCallBack(LogLevel_Debug, [msg UTF8String]);
            return;
        }

        s_WebViewInternalLogCallBack(logLvl, [[NSString stringWithFormat:@"WebViewId: %d, %@", webViewId, msg] UTF8String]);
    }

    static void Log_Debug2(int webViewId, NSString* msg)
    {
        _InnerLog2(webViewId, LogLevel_Debug, msg);          
    }

    static void Log_Info2(int webViewId, NSString* msg)
    {
        _InnerLog2(webViewId, LogLevel_Info, msg);          
    }

    static void Log_Warning2(int webViewId, NSString* msg)
    {
        _InnerLog2(webViewId, LogLevel_Warning, msg);          
    }

    static void Log_Error2(int webViewId, NSString* msg)
    {
        _InnerLog2(webViewId, LogLevel_Error, msg); 
    }


static NSString* s_jsConsoleHandlerName = @"UnityWebViewJsConsole";


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWebViewData
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@implementation MyWebViewData
@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWKScriptMessageHandler
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@implementation MyWKScriptMessageHandler
-(instancetype)initWithData:(MyWebViewData*)data
{
    self = [super init];
    if(self)
    {
        self._Data = data;
    }
    return self;
}

-(void)userContentController:(WKUserContentController *)userContentController didReceiveScriptMessage:(WKScriptMessage *)message
{
    if(message == nil) return;
    
    //判断是否已经销毁了
    MyWebView *webView = [MyWebView GetByNsWebView:self._Data.WebViewId];
    if(webView == nil) return;

    if(s_WebViewJsMessageCallBack != NULL)
    {
        s_WebViewJsMessageCallBack([self._Data.WebViewId intValue], [[message body] UTF8String]);
    }

    // NSLog(@"MyWKScriptMessageHandler.didReceiveScriptMessage() name: %@", [message name]);    
    // NSLog(@"MyWKScriptMessageHandler.didReceiveScriptMessage() body: %@", [message body]);
}
@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWKScriptMessageHandler_JsConsole
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@implementation MyWKScriptMessageHandler_JsConsole
-(instancetype)initWithData:(MyWebViewData*)data
{
    self = [super init];
    if(self)
    {
        self._Data = data;
    }
    return self;
}

-(void)userContentController:(WKUserContentController *)userContentController didReceiveScriptMessage:(WKScriptMessage *)message
{
    if(message == nil || message.body == nil || s_WebViewJsLogCallBack == NULL) return;
    
    int webViewId = [self._Data.WebViewId intValue];
    NSString *body = [message.body isKindOfClass:[NSString class]] ? (NSString *)message.body : [message.body description];
    s_WebViewJsLogCallBack(webViewId, [body UTF8String]);
}
@end


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWKUIDelegate
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@implementation MyWKUIDelegate
-(instancetype)initWithData:(MyWebViewData*)data
{
    self = [super init];
    if(self)
    {
        self._Data = data;
    }
    return self;
}

- (WKWebView *)webView:(WKWebView *)webView createWebViewWithConfiguration:(WKWebViewConfiguration *)configuration forNavigationAction:(WKNavigationAction *)navigationAction windowFeatures:(WKWindowFeatures *)windowFeatures
{    
    if(!navigationAction.targetFrame.isMainFrame)
    {
        [webView loadRequest:navigationAction.request];
    }
    return nil;
}
@end


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWKNavigationDelegate
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@implementation MyWKNavigationDelegate
-(instancetype)initWithData:(MyWebViewData*)data
{
    self = [super init];
    if(self)
    {
        self._Data = data;
    }
    return self;
}

- (void)webView:(WKWebView *)webView decidePolicyForNavigationAction:(WKNavigationAction *)navigationAction decisionHandler:(void (^)(WKNavigationActionPolicy))decisionHandler
{
    NSURL * url = nil;
    if(navigationAction != nil && navigationAction.request != nil && navigationAction.request.URL != nil)
    {
        url = navigationAction.request.URL;
    }
    
    if(url == nil)
    {
        decisionHandler(WKNavigationActionPolicyAllow);
        return;
    }
    
    NSString * urlString = url.absoluteString;
    if([urlString containsString:@"//itunes.apple.com/"])
    {
        [[UIApplication sharedApplication] openURL:url];
        decisionHandler(WKNavigationActionPolicyCancel);
    }
    else if(url.scheme && ![url.scheme hasPrefix:@"http"])
    {
        [[UIApplication sharedApplication] openURL:url];
        decisionHandler(WKNavigationActionPolicyCancel);
    }
    else
    {
        decisionHandler(WKNavigationActionPolicyAllow);
    }
}

- (void)webView:(WKWebView *)webView didFinishNavigation:(WKNavigation *)navigation
{
    Log_Debug([self._Data.WebViewId intValue], "didFinishNavigation");

    if (s_WebViewEventCallBack != NULL)
    {
        s_WebViewEventCallBack([self._Data.WebViewId intValue], EventType_DocumentReady);
    }
}

- (void)webView:(WKWebView *)webView didFailNavigation:(WKNavigation *)navigation withError:(NSError *)error
{
    if(error != nil)
    {
        Log_Error2([self._Data.WebViewId intValue], [NSString stringWithFormat:@"didFailNavigation error: %@ (code:%ld)", error.localizedDescription, (long)error.code]);
    }
}

- (void)webView:(WKWebView *)webView didFailProvisionalNavigation:(WKNavigation *)navigation withError:(NSError *)error
{
    if(error != nil)
    {
        Log_Error2([self._Data.WebViewId intValue], [NSString stringWithFormat:@"didFailProvisionalNavigation: %@ (code:%ld)", error.localizedDescription, (long)error.code]);
    }
}
@end

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// MyWebView
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
@implementation MyWebView
static NSMutableDictionary<NSNumber *, MyWebView *> *s_WebViews =[[NSMutableDictionary alloc] init];
static int s_NextWebViewId = 0;
static WKUserScript *s_JsConsoleScript = nil;

+(MyWebView*) GetByNsWebView:(NSNumber*)webViewId
{
    if(webViewId == nil) return nil;
    return [s_WebViews objectForKey:webViewId];
}

+(MyWebView*) Get:(int)webViewId
{
    NSNumber *nsWebViewId = [NSNumber numberWithInt:webViewId];
    return [s_WebViews objectForKey:nsWebViewId];
}


+ (WKUserScript*)GetJsConsoleScript
{
    if(s_JsConsoleScript != nil) return s_JsConsoleScript;
    NSString *consoleOverrideScript = [NSString stringWithFormat:
        @"(function(){"
        @"var h=window.webkit&&window.webkit.messageHandlers&&window.webkit.messageHandlers.%@;"
        @"if(!h)return;"
        @"function s(l,a){"
        @"try{"
        @"var m=Array.prototype.slice.call(a).map(function(x){"
        @"return typeof x==='object'?JSON.stringify(x):String(x);"
        @"}).join(' ');"
        @"h.postMessage(JSON.stringify({level:l,msg:m}));"
        @"}catch(e){}"
        @"}"
        @"var d=console.debug;"
        @"console.debug=function(){d.apply(console,arguments);s('debug',arguments);};"
        @"var o=console.log;"
        @"console.log=function(){o.apply(console,arguments);s('log',arguments);};"
        @"var i=console.info;"
        @"console.info=function(){i.apply(console,arguments);s('info',arguments);};"
        @"var w=console.warn;"
        @"console.warn=function(){w.apply(console,arguments);s('warn',arguments);};"
        @"var e=console.error;"
        @"console.error=function(){e.apply(console,arguments);s('error',arguments);};"
        @"})();",
        s_jsConsoleHandlerName];

   
    // consoleOverrideScript = [consoleOverrideScript stringByReplacingOccurrencesOfString:@"{LogHandlerName}" withString:s_jsConsoleHandlerName];
    s_JsConsoleScript = [[WKUserScript alloc] initWithSource:consoleOverrideScript injectionTime:WKUserScriptInjectionTimeAtDocumentStart forMainFrameOnly:NO];
    return s_JsConsoleScript;
}

+(int)NewWebViewId
{
    s_NextWebViewId++;
    int newId = s_NextWebViewId;
    return newId;
}

+ (void)CloseWebView:(int)webViewId
{
    NSNumber *nsWebViewId = [NSNumber numberWithInt:webViewId];
    MyWebView *webView = [s_WebViews objectForKey:nsWebViewId];
    if(webView == nil)
    {
        return;
    }

    [s_WebViews removeObjectForKey:nsWebViewId];
    [webView Close];    
}

+ (void) CloseAll
{
    NSArray<MyWebView *> *webViews = [s_WebViews allValues];
    for(MyWebView *webView in webViews)
    {
        [s_WebViews removeObjectForKey:webView._Data.WebViewId];
        [webView Close];
    }
}

-(void)Close
{
    if(self._WebView != nil)
    {
        int webViewId = [self._Data.WebViewId intValue];
        // 移除 script message handler 避免 dealloc 时崩溃
        if(self._ScriptMessageHandler_JsConsole != nil && self._WebView.configuration != nil)
        {
            [self._WebView.configuration.userContentController removeScriptMessageHandlerForName:s_jsConsoleHandlerName];
        }
        [self._WebView removeFromSuperview];
        self._WebView = nil;

        if(s_WebViewEventCallBack != NULL)
        {
            s_WebViewEventCallBack(webViewId, EventType_Destroyed);
        }
    }
}


+ (int) Create:(NSURL * _Nonnull)url WithArea:(CGRect)area
{
    Boolean autoPlayMedia = false;
    Boolean scaling=false;
    Boolean useCookies=true;

    //1. create a new web view id
    int webViewId = [MyWebView NewWebViewId];    
    Log_Debug(webViewId, "Create WebView");
    NSNumber *nsWebViewId = [NSNumber numberWithInt:webViewId];
    
    MyWebViewData *data = [[MyWebViewData alloc] init];
    data.WebViewId = nsWebViewId;
    
    MyWebView* ret = [[MyWebView alloc] init];
    ret._Data = data;

    //2. get the unity view controller
    UIViewController *unityViewController =[(UnityAppController*)[UIApplication sharedApplication].delegate rootViewController];
    
    //3. create a new web view configuration
    WKWebViewConfiguration *webViewConfig = [[WKWebViewConfiguration alloc] init];
    webViewConfig.preferences = [[WKPreferences alloc] init];
    webViewConfig.preferences.javaScriptEnabled = YES;
    webViewConfig.preferences.javaScriptCanOpenWindowsAutomatically = NO;    
    webViewConfig.processPool = [[WKProcessPool alloc] init];
    webViewConfig.allowsInlineMediaPlayback = true;


    if(@available(iOS 11, *))    {                                        }
    else if(@available(iOS 8, *))    {    }
    else
    {
        webViewConfig.requiresUserActionForMediaPlayback = !autoPlayMedia;        
    }   
    
    if(@available(iOS 10, *))
    {
        webViewConfig.ignoresViewportScaleLimits = scaling;
        webViewConfig.mediaTypesRequiringUserActionForPlayback = autoPlayMedia? WKAudiovisualMediaTypeNone: WKAudiovisualMediaTypeAll;
    }
    else
    {
        webViewConfig.mediaPlaybackRequiresUserAction = !autoPlayMedia;
    }

    if(@available(iOS 9, *))
    {
        if(GlobalUserAgent != nil)
        {
            webViewConfig.applicationNameForUserAgent = GlobalUserAgent;
        }
    }

    WKUserContentController *userContentController = [WKUserContentController new];
    webViewConfig.userContentController = userContentController;

    if(JavaScriptHostName!=nil)
    {
        MyWKScriptMessageHandler *scriptMessageHandler = [[MyWKScriptMessageHandler alloc] initWithData:data];
        ret._ScriptMessageHandler = scriptMessageHandler;
        [webViewConfig.userContentController addScriptMessageHandler:scriptMessageHandler name:JavaScriptHostName];        
    }

    // 注入脚本监听 JS console.log/warn/error，通过 s_WebViewJsLogCallBack 回调，msg 格式由 C# 解析
    //不用检查回调是否存在, 真正触发的时候,再检查
    //if(s_WebViewJsLogCallBack != NULL)
    {
        
        MyWKScriptMessageHandler_JsConsole *jsConsoleHandler = [[MyWKScriptMessageHandler_JsConsole alloc] initWithData:data];
        ret._ScriptMessageHandler_JsConsole = jsConsoleHandler;
        [webViewConfig.userContentController addScriptMessageHandler:jsConsoleHandler name:s_jsConsoleHandlerName];
        
        
        [webViewConfig.userContentController addUserScript:[MyWebView GetJsConsoleScript]];
    }
    
     
    
    NSMutableURLRequest *request = [NSMutableURLRequest requestWithURL:url];
    [request setHTTPShouldHandleCookies:useCookies];
    if(useCookies)
    {
        NSArray *cookies = [[NSHTTPCookieStorage sharedHTTPCookieStorage] cookies];
        
        NSString *cookiesString = nil;
        
        if([cookies count] > 0)
        {
            for (NSHTTPCookie * cookie in cookies) {
                if(@available(iOS 11, *))
                {
                    [webViewConfig.websiteDataStore.httpCookieStore setCookie:cookie completionHandler:nil];
                }
                
                if(cookiesString == nil)
                {
                    cookiesString = [NSString stringWithFormat:@"%@=%@", [cookie name], [cookie value]];
                }
                else
                {
                    cookiesString = [NSString stringWithFormat:@"%@;%@=%@", cookiesString, [cookie name], [cookie value]];
                }
            }
        }
        
        if (cookiesString != nil && cookiesString.length > 0) {
            WKUserScript *cookieScript = [[WKUserScript alloc] initWithSource: cookiesString injectionTime:WKUserScriptInjectionTimeAtDocumentStart forMainFrameOnly:NO];
            [webViewConfig.userContentController addUserScript:cookieScript];
            [request addValue:cookiesString forHTTPHeaderField:@"Cookie"];
        }
    }   
    
    
    MyWKNavigationDelegate *navigationDelegate = [[MyWKNavigationDelegate alloc] initWithData:data];    
    ret._NavigationDelegate = navigationDelegate;
    
    MyWKUIDelegate *uiDelegate = [[MyWKUIDelegate alloc] initWithData:data];
    ret._UIDelegate = uiDelegate;


    ret._WebView = [[WKWebView alloc] initWithFrame:area configuration:webViewConfig];
    ret._WebView.navigationDelegate = navigationDelegate;
    ret._WebView.UIDelegate = uiDelegate;
    ret._WebView.opaque = false;
    ret._WebView.backgroundColor = UIColor.clearColor;    

    
    if(@available(iOS 11.0, *))
    {
        if(ret._WebView.scrollView != nil)
        {
            ret._WebView.scrollView.contentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentNever;
        }
    }
    else
    {
        unityViewController.automaticallyAdjustsScrollViewInsets = NO;
    }
    
    [ret._WebView loadRequest:request];
    [unityViewController.view addSubview:ret._WebView];
    ret._WebView.frame = area;


    [s_WebViews setObject:ret forKey:ret._Data.WebViewId];
    return webViewId;    
}

-(void)Navigate:(NSURL * _Nonnull)url
{
    if(self._WebView == nil) return;
    [self._WebView loadRequest:[NSURLRequest requestWithURL:url]];
}

-(void)GoBack
{
    if(self._WebView == nil) return;
    [self._WebView goBack];
}

-(void)GoForward
{
    if(self._WebView == nil) return;
    [self._WebView goForward];
}   

-(void)Reload
{
    if(self._WebView == nil) return;
    [self._WebView reload];
}

-(void)Resize:(CGRect)area
{
    if(self._WebView == nil) return;
    self._WebView.frame = area;
}   

-(void)SetVisible:(bool)visible
{
    if(self._WebView == nil) return;
    self._WebView.hidden = !visible;
}

-(void)SetBGColor:(UIColor*)color
{
    if(self._WebView == nil) return;
    self._WebView.backgroundColor = color;
}   

-(void)RunJsCode:(NSString*)jsCode
{
    if(self._WebView == nil) return;
    //@try {
        [self._WebView evaluateJavaScript:jsCode completionHandler:nil];
    // } @catch (NSException *exception) {
    //     Log_Error2(webViewId, [exception description]);
    // } @catch (NSError *error) {
    //     Log_Error2(webViewId, [error description]);
    // }
}

-(bool)IsLoading
{
    if(self._WebView == nil) return false;
    return [self._WebView isLoading];
}

-(NSString*)GetUrl
{
    if(self._WebView == nil) return nil;
    return [self._WebView.URL absoluteString];
}

-(NSString*)GetTitle
{
    if(self._WebView == nil) return nil;
    return self._WebView.title;
}

 
@end // MyWebView
