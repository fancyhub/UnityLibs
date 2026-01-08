/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/12/1
 * Title   :
 * Desc    :
 *************************************************************************************/
package com.fancyhub.webview;


import android.graphics.Bitmap;
import android.os.Build;
import android.webkit.CookieManager;
import android.webkit.CookieSyncManager;
import android.webkit.WebResourceError;
import android.webkit.WebResourceRequest;
import android.webkit.WebResourceResponse;
import android.webkit.WebView;
import android.webkit.WebViewClient;

class MyWebViewClient extends WebViewClient
{
    private MyWebViewData _Data;
    public MyWebViewClient(MyWebViewData data)
    {
        _Data = data;
    }

    @Override
    public boolean shouldOverrideUrlLoading(WebView view, WebResourceRequest request) {
        if(Build.VERSION.SDK_INT >= 21)
        {
            if(request != null && request.getUrl() != null)
            {
                _Data.Debug("shouldOverrideUrlLoading: " + request.getUrl().toString());
                return shouldOverrideUrlLoading(view, request.getUrl().toString());
            }
            else
            {
                _Data.Debug("shouldOverrideUrlLoading: request is null");
            }
        }
        return false; //由父类处理
    }

    @Override
    public boolean shouldOverrideUrlLoading(WebView view, String url)
    {
        _Data.Debug("shouldOverrideUrlLoading: " + url);
        if(!url.startsWith("http://") && !url.startsWith("https://"))
        {
            _Data.RequestIntent(url);
            return true; //已经处理了, 不要处理了
        }
        return false; //由父类处理
    }

    @Override
    public void onLoadResource(WebView view, String url) {
        _Data.Debug("onLoadResource: " + url);
    }

    @Override
    public WebResourceResponse shouldInterceptRequest(WebView view, WebResourceRequest request) {
        if(Build.VERSION.SDK_INT >= 21)
        {
            if(request != null && request.getUrl() != null)
                _Data.Debug("shouldInterceptRequest: " + request.getUrl().toString());
        }
        return null;
    }

    @Override
    public WebResourceResponse shouldInterceptRequest(WebView view, String url) {
        _Data.Debug("shouldInterceptRequest: " + url);
        return null;
    }

    @Override
    public void onReceivedHttpError(WebView view, WebResourceRequest request, WebResourceResponse errorResponse) {
        if(Build.VERSION.SDK_INT >= 21)
        {
            String msg = "";

            if(request != null)
                msg+="Req: "  + request.getUrl().toString();
            if(errorResponse != null)
                msg+= " Resp: " +errorResponse.getStatusCode()+  ": " + errorResponse.getReasonPhrase();

            _Data.Error("onReceivedHttpError: " +msg);
        }
    }

    @Override
    public void onPageStarted(WebView view, String url, Bitmap favicon) {
        _Data.Debug("onPageStarted:" + url);
        _Data.URL=url;
        _Data.IsLoading=true;

        {
            boolean hasBgColor = false;
            int bgColor = 0;
            if(_Data.SharedData.GlobalGBColor.HasValue())
            {
                bgColor = _Data.SharedData.GlobalGBColor.GetValue();
                hasBgColor=true;
            }
            if(_Data.BGColor.HasValue())
            {
                bgColor=_Data.BGColor.GetValue();
                hasBgColor=true;
            }

            if(hasBgColor)
            {
                try
                {
                    view.setBackgroundColor(bgColor);
                }
                catch(Throwable t)
                {
                    _Data.Error("onPageStarted: Error "+t);
                }
            }
        }

    }

    @Override
    public void onPageFinished(WebView view, String url) {
        _Data.Debug("onPageFinished: "+url);
        _Data.URL=url;
        _Data.IsLoading=true;

        try
        {
            if(_Data.BGColor.HasValue())
                view.setBackgroundColor(_Data.BGColor.GetValue());

            if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP)
            {
                CookieManager.getInstance().flush();
            }
            else
            {
                CookieSyncManager.getInstance().sync();
            }
        }
        catch(Throwable t)
        {
            _Data.Error("onPageFinished: Error "+t);
        }
    }

    @Override
    public void onReceivedError(WebView view, int errorCode, String description, String failingUrl) {
        _Data.Error("onReceivedError: "  + failingUrl + ": " + errorCode + ": " + description);
        _Data.IsLoading = false ;
        _Data.URL = failingUrl;
    }

    @Override
    public void onReceivedError(WebView view, WebResourceRequest request, WebResourceError error) {
        if(Build.VERSION.SDK_INT >= 21)
        {
            if(request != null && request.getUrl() != null)
                _Data.Error("onReceivedError: "+ request.getUrl().toString());
        }
        if(Build.VERSION.SDK_INT >= 23)
        {
            if(error != null)
                _Data.Error("onReceivedError: " + error.getErrorCode() + ": " + error.getDescription());
        }
    }
}