package com.github.fancyhub.webview;

import android.media.MediaPlayer;
import android.view.SurfaceView;
import android.view.View;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.webkit.ConsoleMessage;
import android.webkit.WebChromeClient;
import android.webkit.WebView;
import android.widget.FrameLayout;
import android.widget.VideoView;

class UnityWebChromeClient extends WebChromeClient
{
    private final int _webViewId;
    public UnityWebChromeClient(int webViewId)
    {
        _webViewId = webViewId;
    }


    @Override
    public void onProgressChanged(WebView view, int newProgress) {
        super.onProgressChanged(view, newProgress);

        WebViewStatus status = WebViewManager._GetWebViewStatus(_webViewId);
        if(status != null)
        {
            status.LoadingProgress = newProgress / 100.0;
        }
    }

    @Override
    public boolean onConsoleMessage(ConsoleMessage consoleMessage) {
        InnerConsoleMessage cm = new InnerConsoleMessage();
        cm.message = consoleMessage.message();
        cm.messageLevel = consoleMessage.messageLevel().toString();
        cm.lineNumber = consoleMessage.lineNumber();
        cm.sourceId = consoleMessage.sourceId();
        WebViewManager.CallUnity(_webViewId, "OnConsoleMessage_Android", WebViewManager.GsonForWebView.toJson(cm));
        return true;
    }

    class InnerConsoleMessage
    {
        public String message;
        public String messageLevel;
        public int lineNumber;
        public String sourceId;
    }

    private boolean _ShowingCustomView = false;
    private CustomViewCallback _CustomViewCallback = null;
    private View _CustomView = null;
    private VideoViewListener _VideoViewListener;
    private int _WindowAttributesFlags;
    private int _SystemUiVisibility;


    @Override
    public void onShowCustomView(View view, int requestedOrientation, CustomViewCallback callback) {
        onShowCustomView(view, callback);
    }

    @Override
    public void onShowCustomView(View view, CustomViewCallback callback) {
        try
        {
            WebView webView = WebViewManager._GetWebView(_webViewId);
            if(webView == null) return;

            if (!WebViewManager.showing(_webViewId)) return;

            WindowManager.LayoutParams attrs = WebViewManager.UnityActivity().getWindow().getAttributes();
            _WindowAttributesFlags = attrs.flags;
            _SystemUiVisibility = WebViewManager.UnityActivity().getWindow().getDecorView().getSystemUiVisibility();

            if(view instanceof FrameLayout)
            {
                _ShowingCustomView = true;
                _CustomView = view;
                _CustomViewCallback = callback;

                ViewGroup.LayoutParams layoutParams = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
                WebViewManager.UnityActivityFrameLayout.addView(_CustomView, layoutParams);
                _CustomView.setVisibility(View.VISIBLE);

                FrameLayout customViewFrameLayout = (FrameLayout)view;

                View focusedChild = customViewFrameLayout.getFocusedChild();
                if(focusedChild instanceof VideoView)
                {
                    VideoView videoView = (VideoView)focusedChild;
                    _VideoViewListener = new VideoViewListener();
                    videoView.setOnPreparedListener(_VideoViewListener);
                    videoView.setOnCompletionListener(_VideoViewListener);
                    videoView.setOnErrorListener(_VideoViewListener);
                }
                else
                {
                    // Other classes, including:
                    // - android.webkit.HTML5VideoFullScreen$VideoSurfaceView, which inherits from android.view.SurfaceView (typically API level 11-18)
                    // - android.webkit.HTML5VideoFullScreen$VideoTextureView, which inherits from android.view.TextureView (typically API level 11-18)
                    // - com.android.org.chromium.content.browser.ContentVideoView$VideoSurfaceView, which inherits from android.view.SurfaceView (typically API level 19+)

                    // Handle HTML5 video ended event only if the class is a SurfaceView
                    // Test case: TextureView of Sony Xperia T API level 16 doesn't work fullscreen when loading the javascript below
                    if(webView.getSettings().getJavaScriptEnabled() && (focusedChild instanceof SurfaceView))
                    {
                        String nameInJavaScript = WebViewManager.getNameInJavaScript();
                        if(nameInJavaScript != null && nameInJavaScript.length() > 0)
                        {
                            // Run javascript code that detects the video end and notifies the Javascript interface
                            String js = "javascript:";
                            js += "var _uwv_h5_video_last;";
                            js += "var _uwv_h5_video = document.getElementsByTagName('video')[0];";
                            js += "if (_uwv_h5_video != undefined && _uwv_h5_video != _uwv_h5_video_last) {";
                            {
                                js += "_uwv_h5_video_last = _uwv_h5_video;";
                                js += "function _uwv_h5_video_ended() {";
                                {
                                    js += nameInJavaScript + ".on_HTML5_Video_Ended();"; // Must match Javascript interface name and method of VideoEnableWebView
                                }
                                js += "}";
                                js += "_uwv_h5_video.addEventListener('ended', _uwv_h5_video_ended);";
                            }
                            js += "}";
                            webView.loadUrl(js);
                        }
                    }
                }
            }


            attrs.flags |= WindowManager.LayoutParams.FLAG_FULLSCREEN;
            attrs.flags |= WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON;

            WebViewManager.UnityActivity().getWindow().setAttributes(attrs);

            WebViewManager.UnityActivity().getWindow().getDecorView().setSystemUiVisibility(View.SYSTEM_UI_FLAG_FULLSCREEN);

            WebViewManager.RunOnUiThread(new Runnable() {
                @Override
                public void run() {
                    _CustomView.forceLayout();
                }
            });


        }
        catch(Exception e) { onHideCustomView(); }
    }

    private class VideoViewListener implements
            MediaPlayer.OnPreparedListener, MediaPlayer.OnCompletionListener, MediaPlayer.OnErrorListener
    {
        @Override
        public void onPrepared(MediaPlayer mp) {

        }

        @Override
        public void onCompletion(MediaPlayer mp) {
            onHideCustomView();
        }

        @Override
        public boolean onError(MediaPlayer mp, int what, int extra) {
            return false;
        }
    }

    @Override
    public void onHideCustomView() {
        try
        {
            if(_ShowingCustomView)
            {
                if(_CustomView != null) WebViewManager.UnityActivityFrameLayout.removeView(_CustomView);

                _ShowingCustomView = false;
                if(_CustomViewCallback != null && !_CustomViewCallback.getClass().getName().contains(".chromium."))
                {
                    _CustomViewCallback.onCustomViewHidden();
                }

                _CustomView = null;
                _CustomViewCallback = null;
                _VideoViewListener = null;


                WindowManager.LayoutParams attrs = WebViewManager.UnityActivity().getWindow().getAttributes();
                attrs.flags = _WindowAttributesFlags;
                WebViewManager.UnityActivity().getWindow().setAttributes(attrs);

                WebViewManager.UnityActivity().getWindow().getDecorView().setSystemUiVisibility(_SystemUiVisibility);

            }
        }
        catch(Exception e) { }
    }

    public boolean onBackPressed() {
        if(_ShowingCustomView)
        {
            onHideCustomView();
            return true;
        }
        return false;
    }



}
