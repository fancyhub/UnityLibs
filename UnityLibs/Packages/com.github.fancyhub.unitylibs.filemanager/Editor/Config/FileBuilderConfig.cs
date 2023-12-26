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
    [CreateAssetMenu(fileName = "FileBuilderConfig", menuName = "fanchhub/FileBuilderConfig")]
    public class FileBuilderConfig : ScriptableObject
    {
        private static FileBuilderConfig _Default;
        public const string CPath = "Assets/fancyhub/FileBuilderConfig.asset";

        public string OutputDir = "Bundle/Server";
        public string DefaultExt = ".bytes";

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

        public IBuildCopyStreamingAsset GetCopyStreamingAsset()
        {
            foreach (var p in CopyStreamingAsset)
            {
                if (p == null)
                    continue;
                var r = p.GetCopyStreamingAsset();
                if (r == null)
                    continue;
                return r;
            }
            return null;
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
