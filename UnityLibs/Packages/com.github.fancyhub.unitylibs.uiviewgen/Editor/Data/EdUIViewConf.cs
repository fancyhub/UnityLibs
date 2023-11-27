/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace FH.UI.View.Gen.ED
{
    /// <summary>
    /// 配置，描述 prefab的配置和 class之间的映射关系
    /// </summary>
    public class EdUIViewConf
    {
        public const string C_CS_EXT_SUFFIX = ".ext.cs";
        public const string C_CS_RES_SUFFIX = ".res.cs";

        public string PrefabPath;
        public string ClassName;
        public string ParentClassName;
        public string CsFilePath;

        public string GetCsFileNameRes()
        {
            return ClassName + C_CS_RES_SUFFIX;
        }

        public string GetCsFileNameExt()
        {
            return ClassName + C_CS_EXT_SUFFIX;
        }


        private static Regex C_CLASS_NAME_REG = new Regex(@"[\t\s\w]*public\s*partial\s*class\s*(?<class_name>[a-zA-Z0-9_]*)\s*:\s*(?<parent_class_name>[a-zA-Z0-9_\.]*)\s*");
        private static Regex C_ASSET_PATH_REG = new Regex(@"[\t\s\w]*const\s*string\s*C_AssetPath\s*=\s*""(?<path>[_/\w\.]*)"";");
        public static EdUIViewConf ParseFromCsFile(string cs_file_path)
        {
            if (!cs_file_path.EndsWith(C_CS_RES_SUFFIX))
                return null;

            string[] all_lines = System.IO.File.ReadAllLines(cs_file_path);

            string prefab_path = null;
            string class_name = null;
            string parent_class_name = null;

            foreach (string line in all_lines)
            {
                var nameMatchResult = C_CLASS_NAME_REG.Match(line);
                if (nameMatchResult.Success)
                {
                    class_name = nameMatchResult.Groups["class_name"].ToString();
                    parent_class_name = nameMatchResult.Groups["parent_class_name"].ToString();
                    continue;
                }
                var matchResult = C_ASSET_PATH_REG.Match(line);
                if (matchResult.Success)
                {
                    prefab_path = matchResult.Groups["path"].ToString().Trim();
                    break;
                }
            }

            EdUIViewConf ret = new EdUIViewConf();
            ret.CsFilePath = cs_file_path;
            ret.ClassName = class_name;
            ret.PrefabPath = prefab_path;
            ret.ParentClassName = parent_class_name;
            return ret;
        }
    }
     
}
