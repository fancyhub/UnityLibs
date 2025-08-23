/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/03/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using FH.FileManagement.Ed;
using UnityEngine;

namespace FH.Ed.FileManagement
{
    /// <summary>
    /// 复制原始文件
    /// </summary>
    public class FileBuildStep_BuildRawFiles : FH.FileManagement.Ed.BuildStep
    {
        [Header("eg: tag_a;tag_b")]
        public string Tags;
        public string RootDir;
        public bool KeepReletivePath = true;
        [System.Serializable]
        public struct SearchItem
        {
            public string SearchDir;
            public string SearchPattern;
            public System.IO.SearchOption SearchOption;
            public BitEnum32<UnityEditor.BuildTarget> BuildTargets;
        }
        public List<SearchItem> SearchList = new List<SearchItem>();

        public override List<BuildFileInfo> Build(BuildContext context)
        {
            List<string> tags = new(Tags.Split(';', StringSplitOptions.RemoveEmptyEntries));
            List<BuildFileInfo> ret = new List<BuildFileInfo>();

            if (KeepReletivePath && string.IsNullOrEmpty(RootDir))
            {
                throw new Exception("如果要keep 相对路径, 必须设置 RootDir");
            }

            string root_dir_path = System.IO.Path.GetFullPath(RootDir);
            root_dir_path = root_dir_path.Replace('\\', '/');
            if (!root_dir_path.EndsWith("/"))
                root_dir_path += "/";

            foreach (var item in SearchList)
            {
                if (!item.BuildTargets[context.BuildTarget])
                    continue;
                string search_dir = System.IO.Path.GetFullPath(item.SearchDir);
                search_dir = search_dir.Replace('\\', '/');
                if (KeepReletivePath && !search_dir.StartsWith(root_dir_path))
                {
                    throw new Exception($"SearchDir is not subpath of RootDir,{search_dir} of {root_dir_path}");
                }

                var file_list = System.IO.Directory.GetFiles(search_dir, item.SearchPattern, item.SearchOption);
                foreach (var file_path in file_list)
                {
                    BuildFileInfo info = new BuildFileInfo();
                    info.FileHash = MD5Helper.ComputeFile(file_path);
                    info.FilePath = file_path;
                    info.Tags = tags;

                    if (KeepReletivePath)
                    {
                        info.FileRelativePath = file_path.Substring(root_dir_path.Length).Replace("\\", "/");
                        info.FileName = info.FileRelativePath.Replace("/", "_");
                    }
                    else
                    {
                        info.FileName = System.IO.Path.GetFileName(file_path);
                    }
                    ret.Add(info);
                }
            }
            return ret;
        }
    }
}
