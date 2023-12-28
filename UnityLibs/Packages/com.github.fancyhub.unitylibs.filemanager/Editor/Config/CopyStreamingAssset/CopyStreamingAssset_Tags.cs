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
    public class BuildCopyStreamingAsset_Tags : BuildCopyStreamingAsset
    {
        [Header("eg: tag_a;tag_b")]
        public string Tags = "";
        public override HashSet<string> GetFilesToCopy(FileManifest manifest)
        {
            List<string> tags = new List<string>(Tags.Split(';', StringSplitOptions.RemoveEmptyEntries));

            HashSet<string> file_need_copy = new HashSet<string>();
            foreach (FileManifest.TagItem tag_item in manifest.Tags)
            {
                if (!tags.Contains(tag_item.Name))
                    continue;

                foreach (int file_index in tag_item.Files)
                {
                    file_need_copy.Add(manifest.Files[file_index].FullName);
                }
            }
            return null;
        }
    }
}
