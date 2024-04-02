
using UnityEngine;
using UnityEditor;
using UnityEditor.Android;
using System.IO;

namespace FH.DI.Ed
{
    public class AndroidPostBuildProcessor : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder
        {
            get
            {
                return 999;
            }
        }

        private const string Default=@"
org.gradle.jvmargs=-Xmx**JVM_HEAP_SIZE**M
org.gradle.parallel=true
unityStreamingAssets=**STREAMING_ASSETS**
**ADDITIONAL_PROPERTIES**
";


        void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string path)
        {
            Debug.Log("Bulid path : " + path);
            string gradlePropertiesFile =path + "/../gradle.properties";
            if(!File.Exists(gradlePropertiesFile))
            {
                StreamWriter writer = File.CreateText(gradlePropertiesFile);
                writer.WriteLine(Default);
                writer.Close();
            }

            
            {
                System.IO.File.AppendAllLines (gradlePropertiesFile,
                                        new string[]{
                                            "",
                                            "android.useAndroidX=true"
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
