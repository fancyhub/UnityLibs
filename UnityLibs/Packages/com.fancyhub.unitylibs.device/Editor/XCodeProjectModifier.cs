/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/02/03
 * Title   : 
 * Desc    : 
*************************************************************************************/
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace FH.DI.Ed
{
    public class XCodeProjectModifier : ScriptableObject
    {
        [SerializeField] public XCodeProjectModifierAsset Asset;

#if UNITY_IOS
        [PostProcessBuild(1000)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuildProject)
        {
            //1. check target
            if (buildTarget != BuildTarget.iOS)
                return;

            //2. chect config
            XCodeProjectModifier modifier = ScriptableObject.CreateInstance<XCodeProjectModifier>();
            if (modifier.Asset == null)
                return;


            Debug.Log("====== XCodeProject Begin Modify =======");

            //3. load xcode project
            string projPath = pathToBuildProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            UnityEditor.iOS.Xcode.PBXProject proj = new UnityEditor.iOS.Xcode.PBXProject();
            proj.ReadFromFile(projPath);
            bool dirty = false;

#if UNITY_2019_3_OR_NEWER
            string targetGuid = proj.GetUnityMainTargetGuid();
            string frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();
#else
            string targetGuid = proj.TargetGuidByName("Unity-iPhone");
            string frameworkTargetGuid = targetGuid;
#endif

            //UnityEngine.Debug.Log("OnPostProcessBuild: " + projPath + " " + targetGuid + " " + frameworkTargetGuid);

            //4. Add framework to the project

            // 添加到 UnityFramework target（关键！）
            foreach (var framework in modifier.Asset.FrameworksToAdd)
            {
                if (framework == null || string.IsNullOrEmpty(framework.FrameworkName))
                    continue;

                if (proj.ContainsFramework(frameworkTargetGuid, framework.FrameworkName))
                    continue;

                Debug.Log($"Add Framework: {framework.FrameworkName} ");
                proj.AddFrameworkToProject(frameworkTargetGuid, framework.FrameworkName, framework.Weak);
                dirty = true;
            }


            //5.save
            if (dirty)
                proj.WriteToFile(projPath);

            Debug.Log("======= XCodeProject End Modify =======");
        }
#endif
    }
}
