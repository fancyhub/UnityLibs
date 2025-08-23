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
    public interface IBuildCopyStreamingAsset
    {
        public HashSet<string> GetFilesToCopy(FileManifest manifest);
    }

    public abstract class BuildCopyStreamingAsset : ScriptableObject, IBuildCopyStreamingAsset
    {
        public string Name;
        public bool Enable = true;
        public virtual IBuildCopyStreamingAsset GetCopyStreamingAsset()
        {
            if (Enable)
                return this;
            return null;
        }
        public abstract HashSet<string> GetFilesToCopy(FileManifest manifest);
    }
}
