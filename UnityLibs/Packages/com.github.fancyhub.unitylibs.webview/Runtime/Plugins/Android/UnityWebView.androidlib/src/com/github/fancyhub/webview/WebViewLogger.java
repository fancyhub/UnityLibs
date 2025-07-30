package com.github.fancyhub.webview;

class WebViewLogger
{
    public static IUnityMessenger _UnityMessenger;

    public static void Log(String msg)
    {
        if(_UnityMessenger== null)            return;
        _UnityMessenger.Info(msg);
    }
    public static void Log(String msg, Throwable throwable)
    {if(_UnityMessenger== null)            return;
        _UnityMessenger.Info(msg + ": " + throwable.getClass().getName() + ": " + throwable.getMessage());
    }

    public static void Log(int webViewId, String msg)
    {if(_UnityMessenger== null)            return;
        _UnityMessenger.Info("WebViewId:" + webViewId + " , "+msg);
    }

    public static void Log(int webViewId,String msg, Throwable throwable)
    {if(_UnityMessenger== null)            return;
        _UnityMessenger.Info("WebViewId:" + webViewId + " , "+msg + ": " + throwable.getClass().getName() + ": " + throwable.getMessage());
    }

    public static void Debug(String msg)
    {if(_UnityMessenger== null)            return;
        _UnityMessenger.Debug(msg);
    }

    public static void Debug(int webViewId, String msg)
    {if(_UnityMessenger== null)            return;
        _UnityMessenger.Debug("WebViewId:" + webViewId + " , "+ msg);
    }

    public static void Debug(String msg, Throwable throwable)
    {if(_UnityMessenger== null)            return;
        _UnityMessenger.Debug(msg + ": " + throwable.getClass().getName() + ": " + throwable.getMessage());
    }

    public static void Debug(int webViewId,String msg, Throwable throwable)
    {if(_UnityMessenger== null)            return;
        _UnityMessenger.Debug("WebViewId:" + webViewId + " , "+msg + ": " + throwable.getClass().getName() + ": " + throwable.getMessage());
    }

}