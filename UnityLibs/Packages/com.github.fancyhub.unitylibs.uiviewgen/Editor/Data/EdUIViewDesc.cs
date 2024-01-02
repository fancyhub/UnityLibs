/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using System.IO;


namespace FH.UI.ViewGenerate.Ed
{
    /// <summary>
    /// 配置，描述 prefab的配置和 class之间的映射关系
    /// </summary>
    public sealed class EdUIViewDesc
    {
        public readonly string PrefabPath;
        public readonly string ParentPrefabPath;

        public readonly string PrefabName;
        public readonly string ParentPrefabName;

        public EdUIViewDesc(string prefab_path, string parent_prefab_path)
        {
            PrefabPath = prefab_path;
            PrefabName = Path.GetFileNameWithoutExtension(prefab_path);
            
            if (string.IsNullOrEmpty(parent_prefab_path))
            {
                ParentPrefabPath = "";
                ParentPrefabName = "";
            }
            else
            {
                ParentPrefabPath = parent_prefab_path;
                ParentPrefabName = Path.GetFileNameWithoutExtension(ParentPrefabPath);
            }                
        }

        //public string CsClassName;
        //public string CsParentClassName;

        //public string GetCsFileNameRes()
        //{
        //    return CsClassName + UIViewGeneratorConfig.CSharpConfig.ResSuffix;
        //}

        //public string GetCsFileNameExt()
        //{
        //    return CsClassName + UIViewGeneratorConfig.CSharpConfig.ExtSuffix;
        //}
    }
}
