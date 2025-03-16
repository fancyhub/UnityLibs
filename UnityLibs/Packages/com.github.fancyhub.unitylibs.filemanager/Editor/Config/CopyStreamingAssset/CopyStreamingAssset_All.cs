/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/25
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.FileManagement.Ed
{
    /// <summary>
    /// 复制生成的文件列表到 streamingassets目录
    /// </summary>
    public class BuildCopyStreamingAsset_All : BuildCopyStreamingAsset
    {
        public override HashSet<string> GetFilesToCopy(FileManifest manifest)
        {
            HashSet<string> result = new HashSet<string>();
            foreach(var file in manifest.Files)
            {
                result.Add(file.Name);
            }
            return result;
        }
    }
}
