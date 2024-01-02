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
    public class EdUIViewDesc
    {        
        public string PrefabPath;
        public string ClassName;
        public string ParentClassName;
        public string CsFilePath;

        public string GetCsFileNameRes()
        {
            return ClassName + UIViewGeneratorConfig.CSharpConfig.ResSuffix;
        }

        public string GetCsFileNameExt()
        {
            return ClassName + UIViewGeneratorConfig.CSharpConfig.ExtSuffix;
        }        
    }     
}
