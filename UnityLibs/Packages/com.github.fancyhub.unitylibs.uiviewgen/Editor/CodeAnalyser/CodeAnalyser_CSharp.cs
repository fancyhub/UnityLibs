/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace FH.UI.ViewGenerate.Ed
{
    public class CodeAnalyser_CSharp : ICodeAnalyser
    {
        private const string CReg_PrefabPath = @"PrefabPath:""(?<PrefabPath>[a-zA-Z0-9_\./]*)""";
        private const string CReg_ParentPrefabPath = @"ParentPrefabPath:""(?<ParentPrefabPath>[a-zA-Z0-9_\./]*)""";
        private const string CReg_CsClassName = @"CsClassName:""(?<CsClassName>[a-zA-Z0-9_]*)""";
        private const string CReg_ParentCsClassName = @"ParentCsClassName:""(?<ParentCsClassName>[a-zA-Z0-9_\.]*)""";

        private static Regex CReg = new Regex(@$"//{CReg_PrefabPath},\s*{CReg_ParentPrefabPath},\s*{CReg_CsClassName},\s*{CReg_ParentCsClassName}");

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


            foreach (string line in all_lines)
            {
                if (!line.Contains("//PrefabPath:"))
                    continue;
                var match_result = CReg.Match(line);
                if (!match_result.Success)
                    continue;

                string prefab_path = match_result.Groups["PrefabPath"].ToString();
                string parent_prefab_path = match_result.Groups["ParentPrefabPath"].ToString();
                string class_name = match_result.Groups["CsClassName"].ToString();
                string parent_class_name = match_result.Groups["ParentCsClassName"].ToString();

                EdUIViewDesc ret = new EdUIViewDesc(prefab_path, parent_prefab_path);
                //ret.CsClassName = class_name;
                //ret.CsParentClassName = parent_class_name;

                return ret;
            }
            return null;
        }
    }
}
