/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/12/1
 * Title   :
 * Desc    :
 *************************************************************************************/
package com.fancyhub.webview;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ResolveInfo;
import android.net.Uri;
import android.os.Build;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.webkit.ConsoleMessage;
import android.webkit.CookieManager;
import android.webkit.ValueCallback;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.widget.FrameLayout;

import com.unity3d.player.UnityPlayer;

import java.util.List;

class WebViewDefine
{
    public static final String LogTag = "<UnityWebView>";

    public static  final  int LOG_LEVEL_Debug=0;
    public static  final int LOG_LEVEL_Info = 1;
    public static  final int LOG_LEVEL_WARNING = 2;
    public static  final int LOG_LEVEL_ERROR=3;
}


class ValueBox<T> {
    private T _Value;
    private boolean _HasValue;

    public ValueBox() {
        _HasValue = false;
    }

    public ValueBox(T v) {
        this._Value = v;
        _HasValue = true;
    }

    public void SetValue(T v) {
        this._Value = v;
        this._HasValue = true;
    }

    public boolean HasValue() {
        return _HasValue;
    }

    public T GetValue() {
        return _Value;
    }
}
