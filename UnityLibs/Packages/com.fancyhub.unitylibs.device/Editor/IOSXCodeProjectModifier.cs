/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/02/03
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace FH.DI.Ed
{
    public class IOSXCodeProjectModifier : ScriptableObject
    {
        private const int CallbackOrder = 100001;
        private const string PreprocessorDefinitionsKey = "GCC_PREPROCESSOR_DEFINITIONS";

        [SerializeField] public IOSXCodeProjectModifierAsset Asset;        


//#if UNITY_IOS
        [PostProcessBuild(CallbackOrder)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuildProject)
        {
            //1. check target
            if (buildTarget != BuildTarget.iOS)
                return;

            //2. chect config
            IOSXCodeProjectModifier modifier = ScriptableObject.CreateInstance<IOSXCodeProjectModifier>();
            var asset = modifier.Asset;            
            if (asset == null)
                return;

            Debug.Log("====== XCodeProject Begin Modify =======");
            //3. load xcode project
            string projPath = pathToBuildProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            UnityEditor.iOS.Xcode.PBXProject proj = new UnityEditor.iOS.Xcode.PBXProject();
            proj.ReadFromFile(projPath);

            if (_Process(proj, asset))
            {
                //5.save
                proj.WriteToFile(projPath);
            }

            Debug.Log("======= XCodeProject End Modify =======");
        }

        private static bool _Process(UnityEditor.iOS.Xcode.PBXProject proj, IOSXCodeProjectModifierAsset asset)
        {
            bool dirty = false;

#if UNITY_2019_3_OR_NEWER
            string targetGuid = proj.GetUnityMainTargetGuid();
            string frameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();
#else
            string targetGuid = proj.TargetGuidByName("Unity-iPhone");
            string frameworkTargetGuid = targetGuid;
#endif

            //frame works
            {
                // 添加到 UnityFramework target（关键！）
                foreach (var framework in asset.FrameworksToAdd)
                {
                    if (framework == null || string.IsNullOrEmpty(framework.FrameworkName))
                        continue;

                    if (proj.ContainsFramework(frameworkTargetGuid, framework.FrameworkName))
                        continue;

                    Debug.Log($"Add Framework: {framework.FrameworkName} ");
                    proj.AddFrameworkToProject(frameworkTargetGuid, framework.FrameworkName, framework.Weak);
                    dirty = true;
                }
            }


            //Macros
            var macrosToRemove = _BuildMacrosToRemove(asset.MacrosToRemove);
            var macrosToAdd = _BuildMacrosToAdd(asset.MacrosToAdd);
            if (macrosToRemove.Count > 0 || macrosToAdd.Count > 0)
            {
                List<string> targetGuidList = new();
                targetGuidList.Add(targetGuid);
                if (frameworkTargetGuid != targetGuid && frameworkTargetGuid != null)
                    targetGuidList.Add(frameworkTargetGuid);

                foreach (var guid in targetGuidList)
                {
                    if (_RemovePreprocessorDefinitions(proj, guid, macrosToRemove))
                    {
                        dirty = true;
                    }

                    foreach (var m in macrosToAdd)
                    {
                        if (_AddPreprocessorDefinition(proj, guid, m))
                            dirty = true;
                    }
                }
            }

            return dirty;
        }

        private static List<string> _BuildMacrosToAdd(List<string> macros)
        {
            List<string> result = new();
            if (macros == null)
                return result;

            foreach (var macro in macros)
            {
                _AddUnique(result, _NormalizeMacro(macro));
            }

            return result;
        }

        private static List<string> _BuildMacrosToRemove(List<string> macros)
        {
            List<string> result = new();
            if (macros == null)
                return result;

            foreach (var macro in macros)
            {
                var raw = (macro ?? string.Empty).Trim();
                if (raw.Length == 0)
                    continue;

                _AddUnique(result, _NormalizeMacro(raw));

                // Also remove the bare macro form in case an older export used CN_BUILD instead of CN_BUILD=1.
                if (!raw.Contains("="))
                    _AddUnique(result, raw);
            }

            return result;
        }

        private static bool _RemovePreprocessorDefinitions(UnityEditor.iOS.Xcode.PBXProject proj, string targetGuid, List<string> defines)
        {
            if (defines == null || defines.Count == 0)
                return false;

            bool dirty = false;
            foreach (var configName in proj.BuildConfigNames())
            {
                var configGuid = proj.BuildConfigByName(targetGuid, configName);
                var existing = proj.GetBuildPropertyForConfig(configGuid, PreprocessorDefinitionsKey);

                List<string> matches = new();
                foreach (var define in defines)
                {
                    if (_HasBuildPropertyValue(existing, define))
                        matches.Add(define);
                }

                if (matches.Count == 0)
                    continue;

                proj.UpdateBuildPropertyForConfig(
                    configGuid,
                    PreprocessorDefinitionsKey,
                    null,
                    matches.ToArray());
                dirty = true;
            }

            return dirty;
        }

        private static bool _AddPreprocessorDefinition(UnityEditor.iOS.Xcode.PBXProject proj, string targetGuid, string define)
        {
            if (string.IsNullOrWhiteSpace(define))
                return false;

            bool dirty = false;
            foreach (var configName in proj.BuildConfigNames())
            {
                var configGuid = proj.BuildConfigByName(targetGuid, configName);

                var existing = proj.GetBuildPropertyForConfig(configGuid, PreprocessorDefinitionsKey);

                if (_HasBuildPropertyValue(existing, define))
                    continue;

                proj.UpdateBuildPropertyForConfig(
                    configGuid,
                    PreprocessorDefinitionsKey,
                    new[] { define },
                    null);
                dirty = true;
            }

            return dirty;
        }

        private static string _NormalizeMacro(string macro)
        {
            var value = (macro ?? string.Empty).Trim();
            if (value.Length == 0)
                return string.Empty;

            return value.Contains("=") ? value : value + "=1";
        }

        private static void _AddUnique(List<string> values, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            foreach (var existing in values)
            {
                if (_NormalizeBuildPropertyValue(existing) == _NormalizeBuildPropertyValue(value))
                    return;
            }

            values.Add(value);
        }

        private static bool _HasBuildPropertyValue(string existing, string value)
        {
            if (string.IsNullOrWhiteSpace(existing) || string.IsNullOrWhiteSpace(value))
                return false;

            var target = _NormalizeBuildPropertyValue(value);
            var parts = existing.Split(
                new[] { ' ', '\t', '\r', '\n', ',', ';', '(', ')' },
                System.StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                if (_NormalizeBuildPropertyValue(part) == target)
                    return true;
            }

            return false;
        }

        private static string _NormalizeBuildPropertyValue(string value)
        {
            return (value ?? string.Empty)
                .Trim()
                .Trim('"', '\'')
                .Trim();
        }
//#endif
    }
}
