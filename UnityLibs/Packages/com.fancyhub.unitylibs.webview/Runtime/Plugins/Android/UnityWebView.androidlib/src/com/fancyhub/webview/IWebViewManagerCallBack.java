/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/12/1
 * Title   :
 * Desc    :
 *************************************************************************************/
package com.fancyhub.webview;

public interface IWebViewManagerCallBack
{
    void OnInternalLog(int logLevel,String msg);

    public void OnJsLog(int webViewId, String logType, String source, int lineNumber, String msg);

    void OnJsMessage(int webViewId, String msg);

    void OnEvent(int webViewId, int eventType);
}
