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
    public sealed class BuildFileInfo
    {
        public string FileName; //这个是key
        public string FilePath; //这个是文件路径
        public string FileHash; //文件Hash
        public string FileRelativePath;//相对路径
        public List<string> Tags = new List<string>();
    }

    public interface IBuildStep
    {
        List<BuildFileInfo> Build(BuildContext context);
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
        public abstract List<BuildFileInfo> Build(BuildContext context);
    }
}
