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

    public interface IBuildStep
    {
        void Build(BuildContext context);
    }

    [CreateAssetMenu(fileName = "FileBuilderConfig", menuName = "fanchhub/FileBuilderConfig")]
    public class FileBuilderConfig : ScriptableObject
    {
        private static FileBuilderConfig _Default;
        public const string CPath = "Assets/fancyhub/FileBuilderConfig.asset";

        public string OutputDir = "Bundle/Server";
        public string DefaultExt = ".bytes";

        [HideInInspector] public List<BuildStep> BuildSteps = new List<BuildStep>();


        public List<string> TagsNeedCopy2StreamingAssets = new List<string>();

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


    public class BuildFileInfo
    {
        public string FilePath;
        public string FileHash;
        public List<string> Tags = new List<string>();
    }

    public sealed class BuildContext
    {
        public BuildTarget BuildTarget;
        public List<BuildFileInfo> FileList = new List<BuildFileInfo>();

        public void AddFileInfo(string file_path, string file_hash, string tags)
        {
            FileList.Add(new BuildFileInfo()
            {
                FilePath = file_path,
                FileHash = file_hash,
                Tags = new List<string>(tags.Split(';', StringSplitOptions.RemoveEmptyEntries)),
            });
        }

        public void AddFileInfo(string file_path, string file_hash, List<string> tags)
        {
            FileList.Add(new BuildFileInfo()
            {
                FilePath = file_path,
                FileHash = file_hash,
                Tags = tags,
            });
        }

        public string Target2Name()
        {
            return Target2Name(BuildTarget);
        }

        public static string Target2Name(UnityEditor.BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    return "Android";
                case BuildTarget.iOS:
                    return "IOS";
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return "Win";
                case BuildTarget.StandaloneOSX:
                    return "OSX";
                default:
                    return null;
            }
        }
    }


    public abstract class BuildStep : ScriptableObject, IBuildStep
    {
        public string Name;
        public bool Enable = true;
        public virtual IBuildStep GetBuildStep()
        {
            if (Enable)
                return this;
            return null;
        }
        public abstract void Build(BuildContext context);
    }
}
