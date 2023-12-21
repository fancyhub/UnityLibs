package com.github.fancyhub.nativeio;

import android.app.Activity;
import android.content.Context;
import android.content.res.AssetManager;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

public class JNIContext
{
    private static AssetManager g_asset_mgr; 
	private static native void nativeSetContext( final AssetManager pAssetManager);	
	
	static {
        System.loadLibrary("fhnativeio");
    }

    public static void Init()
    {
        Activity activity = UnityPlayer.currentActivity;
        if(activity == null)
        {
            Log.e("NativeIO","UnityPlayer.currentActivity is null");
            return;
        }
        AssetManager am= activity.getAssets();
        if(g_asset_mgr== am)
            return;
        g_asset_mgr = am;

        nativeSetContext(am);
    }
}
