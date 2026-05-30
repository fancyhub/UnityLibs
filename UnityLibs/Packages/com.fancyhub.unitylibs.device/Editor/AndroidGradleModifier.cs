/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/02/03
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.Android;
using System.IO;

namespace FH.DI.Ed
{
    public class AndroidGradleModifier : IPostGenerateGradleAndroidProject
    {
        private const int CallbackOrder = 999;
        public int callbackOrder => CallbackOrder;

        private const string Default=@"
org.gradle.jvmargs=-Xmx**JVM_HEAP_SIZE**M
org.gradle.parallel=true
unityStreamingAssets=**STREAMING_ASSETS**
**ADDITIONAL_PROPERTIES**
";


        void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string unityLibarayDir)
        {
            string rootDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(unityLibarayDir, "..").Replace("\\", "/"));
            string launcherDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(rootDir, "launcher").Replace("\\", "/"));

            string unityLibarayBuildGradle = System.IO.Path.GetFullPath(System.IO.Path.Combine(unityLibarayDir, "build.gralde")).Replace("\\", "/");
            string launcherBuildGralde = System.IO.Path.GetFullPath(System.IO.Path.Combine(launcherDir, "build.gralde")).Replace("\\", "/");
            string rootBuildGradle = System.IO.Path.GetFullPath(System.IO.Path.Combine(rootDir, "build.gralde")).Replace("\\", "/");
            string rootGraldleProperties= System.IO.Path.GetFullPath(System.IO.Path.Combine(rootDir, "gradle.properties")).Replace("\\", "/");
            string rootSettingGraldle= System.IO.Path.GetFullPath(System.IO.Path.Combine(rootDir, "gradle.properties")).Replace("\\", "/");


            Debug.Log($@"{nameof(AndroidGradleModifier)}
rootBuildGralde: {rootBuildGradle}
rootGraldleProperties: {rootGraldleProperties},
rootSettingGraldle: {rootSettingGraldle},
unityLibarayBuildGradle: {unityLibarayBuildGradle},
launcherBuildGralde: {launcherBuildGralde}");


            if(!File.Exists(rootGraldleProperties))
            {
                StreamWriter writer = File.CreateText(rootGraldleProperties);
                writer.WriteLine(Default);
                writer.Close();
            }

            
            {
                System.IO.File.AppendAllLines (rootGraldleProperties,
                                        new string[]{
                                            "",
                                            "android.useAndroidX=true",
                                            //"android.enableJetifier=true"
                                            });
            }
/*
            StreamWriter writer = File.CreateText(gradlePropertiesFile);
            writer.WriteLine("org.gradle.jvmargs=-Xmx4096M");
            writer.WriteLine("android.useAndroidX=true");
            writer.WriteLine("android.enableJetifier=true");
            writer.Flush();
            writer.Close();
            */

        }
    }
}
