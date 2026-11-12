/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using FH.Ed;
using FH.FileManagement.Ed;
using UnityEditor;

namespace FH.Ed.ResManagement
{
    /// <summary>
    /// 生成自己的zip格式的 vfs, 并返回文件列表
    /// </summary>
    public class FileBuildStep_BuildVfs : FH.FileManagement.Ed.BuildStep
    {
        public FH.VFSManagement.Builder.BuilderConfig BuilderConfig;

        public override List<BuildFileInfo> Build(BuildContext context)
        {
            return Build(BuilderConfig, context.BuildTarget);
        }

        public static List<BuildFileInfo> Build(FH.VFSManagement.Builder.BuilderConfig config, UnityEditor.BuildTarget buildTarget)
        {
            if (config == null)
                return null;

            List<BuildFileInfo> ret = new List<BuildFileInfo>();
            string dest_dir = config.OutputDir.Replace("{Target}", buildTarget.Ext2DirName());
            FileUtil.CreateDir(dest_dir);
            foreach (var p in config.Items)
            {
                string dest_file = p.EdBuildZip(dest_dir);

                UnityEngine.Debug.Log($"Build Zip {p.Name} -> {dest_file}");

                string file_hash = MD5Helper.ComputeFile(dest_file);
                if (string.IsNullOrEmpty(file_hash))
                    continue;
                ret.Add(new BuildFileInfo()
                {
                    FileName = p.Name,
                    FilePath = dest_file,
                    FileHash = file_hash,
                    Tags = new List<string>(p.Tags.Split(';', StringSplitOptions.RemoveEmptyEntries)),
                });
            }
            return ret;
        }
    }

    [CustomEditor(typeof(FH.VFSManagement.Builder.BuilderConfig))]
    public class VfsBuilderConfigInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UnityEngine.GUILayout.Button("Build Vfs"))
            {
                FH.VFSManagement.Builder.BuilderConfig configTarget = target as FH.VFSManagement.Builder.BuilderConfig;
                FileBuildStep_BuildVfs.Build(configTarget, EditorUserBuildSettings.activeBuildTarget);
            }
            EditorGUILayout.Space(10);

            base.OnInspectorGUI();
        }
    }
}
