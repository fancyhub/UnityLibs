/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/25
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI.ViewGenerate.Ed
{
    public static class ClassNameUtil
    {
        /// <summary>
        /// 从 prefab的名字生成 class name
        /// </summary>
        public static string GenClassNameFromPath(string prefab_path, string class_prefix, string class_suffix)
        {
            string prefab_name = System.IO.Path.GetFileNameWithoutExtension(prefab_path);
            prefab_name = prefab_name.Replace(' ', '_');
            string[] name_split = prefab_name.Split('_');
            string class_name = string.Empty;
            foreach (string s in name_split)
            {
                class_name += _UpperFirstAlpha(s);
            }
            return class_prefix + class_name + class_suffix;
        }

        private static string _UpperFirstAlpha(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            if (s[0] < 'a' || s[0] > 'z') return s;
            string start = ((char)(s[0] + 'A' - 'a')).ToString().ToUpper();
            return s.Remove(0, 1).Insert(0, start);
        }
    }
}
