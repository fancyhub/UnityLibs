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
    public class BuildCopyStreamingAsset_All : BuildCopyStreamingAsset
    {
        public override HashSet<string> GetFilesToCopy(FileManifest manifest)
        {
            HashSet<string> result = new HashSet<string>();
            foreach(var file in manifest.Files)
            {
                result.Add(file.FullName);
            }
            return result;
        }
    }
}
