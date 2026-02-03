/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/1/4
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace FH
{
    public static partial class DeviceInfoIOS
    {
        private static partial class _
        {
            [DllImport("__Internal")] public static extern string FH_GetIDFA();

            [DllImport("__Internal")] public static extern bool FH_IsIDFAReady();
        }

        //UnityEngine.iOS.Device.advertisingIdentifier
        //https://docs.unity3d.com/ScriptReference/iOS.Device.advertisingIdentifier.html
        public static string IDFA => _Call(_.FH_GetIDFA);
        public static bool IsIDFAReady => _Call(_.FH_IsIDFAReady);
    }


   
    #if UNITY_EDITOR
    public static  class DeviceInfoIOS_PostBuilder
    {
        private const string CTrackingUsageDescription = "We use your advertising identifier to deliver personalized ads and measure ad performance.";

        [UnityEditor.Callbacks.PostProcessBuild(100)]
        public static void OnPostProcessBuild(UnityEditor.BuildTarget buildTarget, string path)
        {
            //1. check target
            if (buildTarget != UnityEditor.BuildTarget.iOS)
                return;
            
            
            //2. Add framework to the project
            {
                string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
                UnityEditor.iOS.Xcode.PBXProject proj = new UnityEditor.iOS.Xcode.PBXProject();
                proj.ReadFromFile(projPath);

        #if UNITY_2019_3_OR_NEWER
                string targetGuid = proj.GetUnityMainTargetGuid();
                string frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();
        #else
                string targetGuid = proj.TargetGuidByName("Unity-iPhone");
                string frameworkTargetGuid = targetGuid;
        #endif

                //Debug.LogError("OnPostProcessBuild: " + projPath + " " + targetGuid + " " + frameworkTargetGuid);

                // 添加到 UnityFramework target（关键！）
                proj.AddFrameworkToProject(frameworkTargetGuid, "AdSupport.framework", false);
                proj.AddFrameworkToProject(frameworkTargetGuid, "AppTrackingTransparency.framework", false);

                proj.WriteToFile(projPath);                     
            }


            //3. Add NSUserTrackingUsageDescription to Info.Plist if need
            {
                string plistPath = System.IO.Path.Combine(path, "Info.plist");
                UnityEditor.iOS.Xcode.PlistDocument plist = new UnityEditor.iOS.Xcode.PlistDocument();
                plist.ReadFromFile(plistPath);

                UnityEditor.iOS.Xcode.PlistElementDict rootDict = plist.root;
                
                if (!rootDict.values.ContainsKey("NSUserTrackingUsageDescription"))
                {
                    rootDict.SetString("NSUserTrackingUsageDescription",CTrackingUsageDescription);
                }

                plist.WriteToFile(plistPath);
            }
        }
    }
    #endif
}