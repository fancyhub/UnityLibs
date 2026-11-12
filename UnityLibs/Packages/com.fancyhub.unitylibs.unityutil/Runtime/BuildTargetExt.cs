/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/26
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR 
using UnityEditor;
#endif

namespace FH
{
    public static class BuildTargetExt
    {
#if UNITY_EDITOR 
        public static string Ext2DirName(this BuildTarget target)
        {
            switch (target)
            {
                default:
                    return $"Unsupport_{target}";

                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "IOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Win";
                case BuildTarget.StandaloneOSX:
                    return "OSX";                
            }
        }
#endif

        public static string Ext2DirName(this RuntimePlatform target)
        {
            switch (target)
            {
                default:
                    return $"Unsupport_{target}";

                case RuntimePlatform.Android:
                    return "Android";

                case RuntimePlatform.IPhonePlayer:
                    return "IOS";

                case RuntimePlatform.WindowsPlayer:
                    return "Win";

                case RuntimePlatform.OSXPlayer:
                    return "OSX";

#if UNITY_EDITOR 
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.OSXEditor:
                    return UnityEditor.EditorUserBuildSettings.activeBuildTarget.Ext2DirName();
#endif          


            }
        }

    }
}