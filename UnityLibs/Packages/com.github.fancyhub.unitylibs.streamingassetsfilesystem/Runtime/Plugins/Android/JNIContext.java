package com.github.fancyhub.nativeio;

import android.app.Activity;
import android.content.Context;
import android.content.res.AssetManager;
import android.util.Log;
import java.util.ArrayList;

import com.unity3d.player.UnityPlayer;

public class JNIContext
{
    private static AssetManager g_asset_mgr; 
	private static native void nativeSetContext( final AssetManager pAssetManager);	
    private static native void nativeAddFolder( final String folderPath);	
	
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

    public static void FetchAllFiles()
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

        ArrayList<String> folderList = new ArrayList<String>();
        _GetAllFolders(am,"",folderList);
        for(int i=0;i<folderList.size();i++)
        {
            nativeAddFolder(folderList.get(i));
        }
    }

    private static void _GetAllFolders(AssetManager asset_mgr, String path, ArrayList<String> outFolderList)
    {
        try 
        {
           String[] list= asset_mgr.list(path);
           if(list  == null || list.length<=0)           
                return;           

           outFolderList.add(path);
           for(int i=0;i<list.length;i++)
           {
                if(path.length()==0)
                    _GetAllFolders(asset_mgr,list[i],outFolderList);
                else 
                    _GetAllFolders(asset_mgr,path+"/"+list[i],outFolderList);
           }
        }catch(Exception e)
        {

        }         
    }
}
