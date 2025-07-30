package com.github.fancyhub.webview;

public interface IUnityMessenger {

    void Debug(String msg);

    void Info(String msg);

    void Error(String msg);

    void Call(int webViewId,String methodName,  String parameters);
}
