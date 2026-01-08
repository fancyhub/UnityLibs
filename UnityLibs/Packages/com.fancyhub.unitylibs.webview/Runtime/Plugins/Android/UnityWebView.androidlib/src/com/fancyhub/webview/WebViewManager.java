/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/12/1
 * Title   :
 * Desc    :
 *************************************************************************************/

package com.fancyhub.webview;

import android.util.Log;

import java.util.HashMap;
import java.util.Set;



public class WebViewManager {
    public static final int WEB_VIEW_INTENT_REQUEST_CODE = 0x222;

    private static WebViewSharedData _SharedData;
    public static void Init(String javascriptName, IWebViewManagerCallBack callBack)
    {
        if(_SharedData!=null)
            return;

        Log.d(WebViewDefine.LogTag,"Init");

        _SharedData = new WebViewSharedData(javascriptName,callBack);
    }

    public static void SetGlobalBGColor(int bgColor)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"Create: sharedData null");
            return;
        }

        _SharedData.GlobalGBColor.SetValue(bgColor);
    }

    public static void SetGlobalUserAgent(String userAgent)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"Create: sharedData null");
            return;
        }

        _SharedData.GlobalUserAgent.SetValue(userAgent);
    }

    public static void SetGlobalScaling(boolean scaling)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"Create: sharedData null");
            return;
        }

        _SharedData.GlobalScaling.SetValue(scaling);
    }

    private static volatile HashMap<Integer, MyWebView> _WebViews = new HashMap<>();

    public static int Create(String url, float x, float y, float width,float height)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"Create: sharedData null");
            return -1;
        }

        _SharedData.Debug("Create WebView");


        MyWebView ret = new MyWebView(_SharedData);
        _WebViews.put(ret.GetId(),ret);

        _SharedData.RunOnUiThread(()->
        {
            ret.Create(url,x,y,width,height);
        });

        return ret.GetId();
    }

    public static void Close(int webViewId)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"Close: sharedData null");
            return;
        }
        if(webViewId==0)
        {
            android.util.Log.e(WebViewDefine.LogTag,"Close: param webviewid is 0");
            return;
        }

        _SharedData.Debug("Close " +webViewId);

        _SharedData.RunOnUiThread(()->
        {
            _SharedData.Debug("Close2 " +webViewId);
            MyWebView view= _WebViews.get(webViewId);
            if(view==null)
            {
                _SharedData.Error("Close: Can't find webview " + webViewId);
                return;
            }
            _WebViews.remove(webViewId);

            view.Destroy();
        });
    }

    public static void CloseAll()
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"CloseAll: sharedData null");
            return;
        }

        _SharedData.Debug("CloseAll");
        _SharedData.RunOnUiThread(()->
        {
            Set<Integer> webViewIds = _WebViews.keySet();
            for(Integer webViewId : webViewIds)
            {
                MyWebView view= _WebViews.get(webViewId);
                if(view==null)
                {
                    _SharedData.Error("CloseAll: Can't find webview " + webViewId);
                    return;
                }
                _WebViews.remove(webViewId);

                view.Destroy();
            }
        });
    }

    public static String GetUrl(int webViewId)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"GetUrl: sharedData null");
            return null;
        }

        _SharedData.Debug("GetUrl");
        MyWebView view= _WebViews.get(webViewId);
        if(view==null)
            return null;

        return view.GetUrl();
    }

    public static boolean IsLoading(int webViewId)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"IsLoading: sharedData null");
            return false;
        }

        _SharedData.Debug("IsLoading");
        MyWebView view= _WebViews.get(webViewId);
        if(view==null)
        {
            _SharedData.Error("IsLoading: Can't find webview " + webViewId);
            return false;
        }

        return view.IsLoading();
    }

    public static void Navigate(int webViewId,String url)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"Navigate: sharedData null");
            return;
        }

        _SharedData.Debug("Navigate");
        _SharedData.RunOnUiThread(()->
        {
            MyWebView view= _WebViews.get(webViewId);
            if(view==null)
            {
                _SharedData.Error("Navigate: Can't find webview " + webViewId);
                return;
            }

            view.Navigate(url);
        });
    }
    public static void GoForward(int webViewId)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"GoForward: sharedData null");
            return ;
        }
        _SharedData.Debug("GoForward");
        _SharedData.RunOnUiThread(()->
        {
            MyWebView view= _WebViews.get(webViewId);
            if(view==null)
            {
                _SharedData.Error("GoForward: Can't find webview " + webViewId);
                return;
            }

            view.GoForward();
        });
    }

    public static void GoBack(int webViewId)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"GoBack: sharedData null");
            return;
        }
        _SharedData.Debug("GoBack");
        _SharedData.RunOnUiThread(()->
        {
            MyWebView view= _WebViews.get(webViewId);
            if(view==null)
            {
                _SharedData.Error("GoBack: Can't find webview " + webViewId);
                return;
            }

            view.GoBack();
        });
    }

    public static void Resize(int webViewId, float x, float y,float width, float height)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"Resize: sharedData null");
            return;
        }
        _SharedData.Debug("Resize");
        _SharedData.RunOnUiThread(()->
        {
            MyWebView view= _WebViews.get(webViewId);
            if(view==null)
            {
                _SharedData.Error("Resize: Can't find webview " + webViewId);
                return;
            }

            view.Resize(x,y,width,height);
        });
    }

    public static void Reload(int webViewId)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"Reload: sharedData null");
            return;
        }
        _SharedData.Debug("Reload");
        _SharedData.RunOnUiThread(()->
        {
            MyWebView view= _WebViews.get(webViewId);
            if(view==null)
            {
                _SharedData.Error("Reload: Can't find webview " + webViewId);
                return;
            }

            view.Reload();
        });
    }

    public static boolean IsVisible(int webViewId)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"IsVisible: sharedData null");
            return false;
        }

        _SharedData.Debug("IsVisible");
        return false;
    }

    public static void RunJavaScript(int webViewId, String jsCode)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"RunJavaScript: sharedData null");
            return;
        }
        _SharedData.Debug("RunJavaScript");
        _SharedData.RunOnUiThread(()->
        {
            MyWebView view= _WebViews.get(webViewId);
            if(view==null)
            {
                _SharedData.Error("RunJavaScript: Can't find webview " + webViewId);
                return;
            }

            view.RunJsCode(jsCode);
        });
    }

    public static void SetBGColor(int webViewId, int color)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"SetBGColor: sharedData null");
            return;
        }
        _SharedData.Debug("SetBGColor");
        _SharedData.RunOnUiThread(()->
        {
            MyWebView view= _WebViews.get(webViewId);
            if(view==null)
            {
                _SharedData.Error("SetBGColor: Can't find webview " + webViewId);
                return;
            }

            view.SetBGColor(color);
        });
    }

    public static void SetVisible(int webViewId, boolean visible)
    {
        if(_SharedData==null)
        {
            android.util.Log.e(WebViewDefine.LogTag,"SetVisible: sharedData null");
            return;
        }
        _SharedData.Debug("SetVisible " +webViewId+ " "+ visible);
        _SharedData.RunOnUiThread(()->
        {
            MyWebView view= _WebViews.get(webViewId);
            if(view==null)
            {
                _SharedData.Error("SetVisible: Can't find webview " + webViewId);
                return;
            }

            view.SetVisible(visible);
        });
    }
}