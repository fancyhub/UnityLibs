package com.github.fancyhub.webview;


import android.content.Intent;
import android.content.pm.ResolveInfo;
import android.graphics.Bitmap;
import android.net.Uri;
import android.os.Build;
import android.view.View;
import android.webkit.CookieManager;
import android.webkit.CookieSyncManager;
import android.webkit.WebResourceError;
import android.webkit.WebResourceRequest;
import android.webkit.WebResourceResponse;
import android.webkit.WebView;
import android.webkit.WebViewClient;

import androidx.annotation.Nullable;

import java.util.List;

class UnityWebViewClient extends WebViewClient
{
    private final int _webViewId;
    private final int _bgColor;
    public UnityWebViewClient(int webViewId,int bgColor)
    {
        _webViewId = webViewId;
        _bgColor = bgColor;
    }

    @Override
    public boolean shouldOverrideUrlLoading(WebView view, WebResourceRequest request) {
        if(Build.VERSION.SDK_INT >= 21)
        {
            if(request != null && request.getUrl() != null)
            {
                WebViewLogger.Debug(_webViewId, "@shouldOverrideUrlLoading(request): " + request.getUrl().toString());
                return shouldOverrideUrlLoading(view, request.getUrl().toString());
            }
            else
            {
                WebViewLogger.Debug(_webViewId, "@shouldOverrideUrlLoading(request): request is null");
            }
        }
        return false;
    }

    @Override
    public boolean shouldOverrideUrlLoading(WebView view, String url)
    {
        WebViewLogger.Debug(_webViewId, "@shouldOverrideUrlLoading(url): " + url);
        boolean result = false;
        if(url.startsWith("intent://"))
        {
            try
            {
                Intent intent = Intent.parseUri(url, Intent.URI_INTENT_SCHEME);
                intent.addCategory(Intent.CATEGORY_BROWSABLE);
                intent.setComponent(null);
                if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.ICE_CREAM_SANDWICH_MR1)
                {
                    intent.setSelector(null);
                }
                List<ResolveInfo> resolves = WebViewManager.UnityActivity().getPackageManager().queryIntentActivities(intent, 0);
                if(resolves != null && resolves.size() > 0)
                {
                    if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN)
                    {
                        WebViewManager.UnityActivity().startActivityIfNeeded(intent, WebViewManager.WEB_VIEW_INTENT_REQUEST_CODE);
                    }
                    else
                    {
                        WebViewManager.UnityActivity().startActivityForResult(intent, WebViewManager.WEB_VIEW_INTENT_REQUEST_CODE);
                    }
                }
                return true;
            }
            catch(Throwable t)
            {
                WebViewLogger.Debug(_webViewId, "@shouldOverrideUrlLoading(): " + url, t);
            }
        }
        if(!url.startsWith("http://") && !url.startsWith("https://"))
        {
            try
            {
                Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
                intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_SINGLE_TOP);
                WebViewManager.UnityActivity().startActivity(intent);
            }
            catch(Throwable t)
            {
                WebViewLogger.Debug(_webViewId, "@shouldOverrideUrlLoading(): " + url, t);
            }
            return true;
        }
        if(!result)
        {
            try
            {
                view.loadUrl(url);
            }
            catch(Throwable t)
            {
                WebViewLogger.Debug(_webViewId, "@shouldOverrideUrlLoading(): loadUrl: " + url, t);
            }
        }
        if(!result)
        {
            WebViewStatus status = WebViewManager._GetWebViewStatus(_webViewId);
            if(status != null) status.Url = url;
        }
        return result;
    }



    @Override
    public void onLoadResource(WebView view, String url) {
        WebViewLogger.Debug(_webViewId, "@onLoadResource(): " + url);
    }

    @Nullable
    @Override
    public WebResourceResponse shouldInterceptRequest(WebView view, WebResourceRequest request) {
        if(Build.VERSION.SDK_INT >= 21)
        {
            if(request != null && request.getUrl() != null)
                WebViewLogger.Debug(_webViewId, "@shouldInterceptRequest(request): " + request.getUrl().toString());
        }
        return null;
    }

    @Nullable
    @Override
    public WebResourceResponse shouldInterceptRequest(WebView view, String url) {
        WebViewLogger.Debug(_webViewId, "@shouldInterceptRequest(url): " + url);
        return null;
    }

    @Override
    public void onReceivedHttpError(WebView view, WebResourceRequest request, WebResourceResponse errorResponse) {
        if(Build.VERSION.SDK_INT >= 21)
        {
            if(request != null) WebViewLogger.Log(_webViewId, "@onReceivedHttpError(): " + request.getUrl().toString());
            if(errorResponse != null) WebViewLogger.Log(_webViewId, "@onReceivedHttpError(): " + errorResponse.getStatusCode() + ": " + errorResponse.getReasonPhrase());
        }
    }

    @Override
    public void onPageStarted(WebView view, String url, Bitmap favicon) {
        WebViewLogger.Debug(_webViewId, "@onPageStarted(): " + url);
        WebViewStatus status = WebViewManager._GetWebViewStatus(_webViewId);
        if(status != null)
        {
            status.IsLoading = true;
            status.Url = url;
        }
        try
        {
            //view.setBackgroundColor(Color.TRANSPARENT);
            view.setBackgroundColor(_bgColor);
        }
        catch(Throwable t)
        {
            WebViewLogger.Log(_webViewId, "@onPageStarted()", t);
        }
    }

    @Override
    public void onPageFinished(WebView view, String url) {
        WebViewLogger.Debug(_webViewId, "@onPageFinished(): " + url);
        WebViewStatus status = WebViewManager._GetWebViewStatus(_webViewId);
        if(status != null)
        {
            status.IsLoading = false;
            status.Url = url;
        }

        try
        {
            view.setBackgroundColor(_bgColor);

            if(status != null && status.DeferredDisplay)
            {
                status.DeferringDisplay = false;
                if(!status.Hiding)
                {
                    WebViewLogger.Debug(_webViewId, "@onPageFinished(): showing " + url);
                    WebViewManager.UnityActivityFrameLayout.setVisibility(View.VISIBLE);
                    view.setVisibility(View.VISIBLE);
                    view.requestFocus();
                }
            }

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
            WebViewLogger.Log(_webViewId, "@onPageFinished()", t);
        }

        if(status != null)
        {
            while(!status.PendingJavaScriptInvokes.isEmpty())
            {
                JavaScriptInvoke javaScriptInvoke = status.PendingJavaScriptInvokes.removeFirst();
                WebViewManager._runJavaScript(_webViewId, javaScriptInvoke.code, javaScriptInvoke.callback, javaScriptInvoke.id);
            }
        }
    }

    @Override
    public void onReceivedError(WebView view, int errorCode, String description, String failingUrl) {
        WebViewLogger.Debug(_webViewId, "@onReceivedError(): " + failingUrl + ": " + errorCode + ": " + description);
        WebViewStatus status = WebViewManager._GetWebViewStatus(_webViewId);
        if(status != null)
        {
            status.IsLoading = false;
            status.Url = failingUrl;
        }
    }

    @Override
    public void onReceivedError(WebView view, WebResourceRequest request, WebResourceError error) {
        if(Build.VERSION.SDK_INT >= 21)
        {
            if(request != null && request.getUrl() != null)
                WebViewLogger.Debug(_webViewId, "@onReceivedError(): " + request.getUrl().toString());
        }
        if(Build.VERSION.SDK_INT >= 23)
        {
            if(error != null) WebViewLogger.Debug(_webViewId, "@onReceivedError(): " + error.getErrorCode() + ": " + error.getDescription());
        }
    }
}
