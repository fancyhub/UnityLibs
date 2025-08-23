/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/25
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FH.FileManagement.Ed
{
    [CreateAssetMenu(fileName = "FileBuilderConfig", menuName = "fancyhub/FileBuilderConfig")]
    public class FileBuilderConfig : ScriptableObject
    {
        private static FileBuilderConfig _Default;
        public const string CPath = "Assets/fancyhub/FileBuilderConfig.asset";

        [SerializeField]
        private string _OutputDir = "ProjTemp/Build/CDN/{Target}";
        public string DefaultExt = ".bytes";
        public bool GenGZ = false;

        public string GetOutputDir(UnityEditor.BuildTarget target)
        {
            return _OutputDir.Replace("{Target}", target.ToString());
        }
        [HideInInspector] public List<BuildStep> BuildSteps = new List<BuildStep>();

        [HideInInspector] public List<BuildCopyStreamingAsset> CopyStreamingAsset = new List<BuildCopyStreamingAsset>();

        public List<IBuildStep> GetBuildSteps()
        {
            List<IBuildStep> ret = new List<IBuildStep>();
            foreach (var p in BuildSteps) 
            {
                IBuildStep step = p.GetBuildStep();
                if (step != null)
                    ret.Add(step);
            }

            return ret;
        }

        public List<FileManifest.FileItem> GetFilesNeedToCopy2StreamingAssets(FileManifest file_manifest)
        {
            HashSet<string> file_set = new HashSet<string>();

            foreach (var p in CopyStreamingAsset)
            {
                if (p == null)
                    continue;
                IBuildCopyStreamingAsset r = p.GetCopyStreamingAsset();
                if (r == null)
                    continue;

                HashSet<string> temp = r.GetFilesToCopy(file_manifest);
                foreach (var p2 in temp)
                    file_set.Add(p2);
            }

            List<FileManifest.FileItem> ret = new List<FileManifest.FileItem>();
            foreach (var p in file_set)
            {
                var item = file_manifest.FindFile(p);
                if (item == null)
                {
                    Debug.LogError($"找不到 {p} 复制到StreamingAssets");
                }
                else
                {
                    ret.Add(item);
                }
            }
            return ret;
        }

        public static FileBuilderConfig GetDefault()
        {
            if (_Default != null)
                return _Default;
            _Default = AssetDatabase.LoadAssetAtPath<FileBuilderConfig>(CPath);
            if (_Default == null)
            {
                Debug.LogError("加载 FileBuilderConfig 失败 " + CPath);
                _Default = new FileBuilderConfig();
                AssetDatabase.CreateAsset(_Default, CPath);
                Debug.Log("创建一个 FileBuilderConfig");
            }
            return _Default;
        }
    }
}
