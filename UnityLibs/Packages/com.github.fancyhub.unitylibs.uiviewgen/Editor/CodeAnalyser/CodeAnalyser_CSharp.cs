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

namespace FH.UI.ViewGenerate.Ed
{
 

    public class CodeAnalyser_CSharp : ICodeAnalyser
    {
        private static Regex C_CLASS_NAME_REG = new Regex(@"[\t\s\w]*public\s*partial\s*class\s*(?<class_name>[a-zA-Z0-9_]*)\s*:\s*(?<parent_class_name>[a-zA-Z0-9_\.]*)\s*");
        private static Regex C_ASSET_PATH_REG = new Regex(@"[\t\s\w]*const\s*string\s*C_AssetPath\s*=\s*""(?<path>[_/\w\.]*)"";");

        public List<EdUIViewDesc> ParseAll(string cs_file_folder)
        {
            List<EdUIViewDesc> list = new List<EdUIViewDesc>();
            string[] files = Directory.GetFiles(cs_file_folder, "*" + UIViewGeneratorConfig.CSharpConfig.ResSuffix);
            foreach (string file in files)
            {
                EdUIViewDesc desc = Parse(file);
                if (desc != null)
                {
                    list.Add(desc);
                }
            }
            return list;
        }

        public EdUIViewDesc Parse(string cs_file_path)
        {
            if (!cs_file_path.EndsWith(UIViewGeneratorConfig.CSharpConfig.ResSuffix))
                return null;

            string[] all_lines = File.ReadAllLines(cs_file_path);

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

            EdUIViewDesc ret = new EdUIViewDesc();
            ret.CsFilePath = cs_file_path;
            ret.ClassName = class_name;
            ret.PrefabPath = prefab_path;
            ret.ParentClassName = parent_class_name;
            return ret;
        }
    }
}
