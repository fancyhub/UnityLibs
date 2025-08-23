package com.fancyhub.webview;

import com.google.gson.Gson;


class UnityMessenger implements IUnityMessenger
{
    private static class WebViewMessage
    {
        public int WebViewId;

        public String MethodName;

        public String Parameters;
    }

    private String _UnityObjName;
    private Gson _Gson;

    public UnityMessenger(String unityObjName)
    {
        _UnityObjName = unityObjName;
        _Gson = new Gson();
    }
    public void Debug(String msg)
    {
        com.unity3d.player.UnityPlayer.UnitySendMessage(_UnityObjName,"OnWebViewLog", msg);
    }
    public void Info(String msg)
    {
        com.unity3d.player.UnityPlayer.UnitySendMessage(_UnityObjName,"OnWebViewInfo", msg);
    }

    public void Error(String msg)
    {
        com.unity3d.player.UnityPlayer.UnitySendMessage(_UnityObjName,"OnWebViewError", msg);
    }

    public void Call(int webViewId,String methodName,  String parameters)
    {
        WebViewMessage message = new WebViewMessage();
        message.MethodName = methodName;
        message.WebViewId = webViewId;
        message.Parameters = parameters;
        String content = _Gson.toJson(message);

        com.unity3d.player.UnityPlayer.UnitySendMessage(_UnityObjName, "OnWebViewMsg", content);
    }
}
