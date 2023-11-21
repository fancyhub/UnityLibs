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

        public string GetCsFileNameRes()
        {
            return ClassName + C_CS_RES_SUFFIX;
        }

        public string GetCsFileNameExt()
        {
            return ClassName + C_CS_EXT_SUFFIX;
        }
    }

    /// <summary>
    /// 配置，描述了 EdSmUiConf的集合
    /// 给了一些搜索，添加，保存，加载的方法
    /// </summary>
    public class EdUIViewConfDb
    {
        private static Regex C_CLASS_NAME_REG = new Regex(@"[\t\s\w]*public\s*partial\s*class\s*(?<class_name>[_/\w\.]*)\s*:\s*(?<parent_class_name>[_/\w\.]*)\s*");
        private static Regex C_ASSET_PATH_REG = new Regex(@"[\t\s\w]*const\s*string\s*C_AssetPath\s*=\s*""(?<path>[_/\w\.]*)"";");

        public UIViewGenConfig Config;

        private List<EdUIViewConf> _all_confs = new();

        //key is class name
        private Dictionary<string, EdUIViewConf> _class_2_conf = new();
        // key is prefab path
        private Dictionary<string, EdUIViewConf> _path_2_conf = new();

        public static EdUIViewConfDb LoadConfFromCSCode(UIViewGenConfig config)
        {
            EdUIViewConfDb ret = new EdUIViewConfDb();
            ret.Config = config;
            string[] files = System.IO.Directory.GetFiles(config.CodeFolder);
            foreach (string file in files)
            {
                if (file.EndsWith(EdUIViewConf.C_CS_RES_SUFFIX))
                {
                    EdUIViewConf conf = _GenCnfFromCS(file);
                    ret._all_confs.Add(conf);
                }
            }
            ret._BuildCache();
            return ret;
        }

        public List<EdUIViewConf> GetAllConfigs()
        {
            return _all_confs;
        }
        public IEnumerable<string> GetPathList()
        {
            return _path_2_conf.Keys;
        }

        public EdUIViewConf FindConfWithPrefabPath(string prefab_path)
        {
            if (string.IsNullOrEmpty(prefab_path))
                return null;

            EdUIViewConf ret = null;
            _path_2_conf.TryGetValue(prefab_path, out ret);
            return ret;
        }

        public EdUIViewConf FindConfWithClassName(string class_name)
        {
            if (string.IsNullOrEmpty(class_name))
                return null;
            EdUIViewConf ret = null;
            _class_2_conf.TryGetValue(class_name, out ret);
            return ret;
        }

        private static List<string> _parent_class_list0 = new List<string>();
        private static List<string> _parent_class_list1 = new List<string>();

        public string GetCommonBase(string class_name1, string class_name2)
        {
            if (class_name1 == class_name2)
                return class_name1;

            GetParentClassList(class_name1, ref _parent_class_list0);
            GetParentClassList(class_name2, ref _parent_class_list1);

            if (_parent_class_list0.Count == 0 || _parent_class_list1.Count == 0)
            {
                return Config.BaseClassName;
            }

            int count = System.Math.Min(_parent_class_list0.Count, _parent_class_list1.Count);
            for (int i = count - 1; i >= 0; i--)
            {
                if (_parent_class_list0[i] == _parent_class_list1[i])
                    return _parent_class_list0[i];
            }

            return Config.BaseClassName;
        }

        public bool IsParentClass(string class_name, string parent_class_name)
        {
            GetParentClassList(class_name, ref _parent_class_list0);
            for (int i = _parent_class_list0.Count - 2; i >= 0; i--)
            {
                if (parent_class_name == _parent_class_list1[i])
                    return true;
            }
            return false;
        }


        //0: 是Base, 最后一个是自己
        public void GetParentClassList(string class_name, ref List<string> out_list)
        {
            out_list.Clear();

            EdUIViewConf conf = FindConfWithClassName(class_name);
            if (conf == null)
                return;

            for (; ; )
            {
                out_list.Add(conf.ClassName);

                if (conf.ParentClassName == Config.BaseClassName)
                {
                    out_list.Add(conf.ParentClassName);
                    break;
                }
                conf = FindConfWithClassName(conf.ParentClassName);
                if (conf == null)
                {
                    Log.E("Error " + conf.ClassName + " : " + conf.ParentClassName);
                    //出错了
                    out_list.Clear();
                    break;
                }
            }

            out_list.Reverse();
        }

        public bool AddConf(EdUIViewConf conf)
        {
            _CheckExist(conf);
            _path_2_conf.Add(conf.PrefabPath, conf);
            _class_2_conf.Add(conf.ClassName, conf);
            _all_confs.Add(conf);
            return true;
        }

        public void RemoveInvalidatePath()
        {
            bool changed = false;
            for (int i = _all_confs.Count - 1; i >= 0; --i)
            {
                EdUIViewConf conf = _all_confs[i];
                if (!File.Exists(conf.PrefabPath))
                {
                    _all_confs.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed)
                _BuildCache();
        }


        private void _CheckExist(EdUIViewConf conf)
        {
            EdUIViewConf old_conf;
            _class_2_conf.TryGetValue(conf.ClassName, out old_conf);
            if (null == old_conf)
                return;

            Log.E("出现了重复,prefab可能移动过目录，但生成的res代码文件中路径还是原来的: {0} \nPath1: {1}\nPath2: {2}"
                , conf.ClassName
                , conf.PrefabPath
                , old_conf.PrefabPath);
        }

        private void _BuildCache()
        {
            _path_2_conf.Clear();
            _class_2_conf.Clear();

            foreach (EdUIViewConf conf in _all_confs)
            {
                _path_2_conf.Add(conf.PrefabPath, conf);
                _class_2_conf.Add(conf.ClassName, conf);
            }
        }


        private static EdUIViewConf _GenCnfFromCS(string file_path)
        {
            string[] all_lines = System.IO.File.ReadAllLines(file_path);

            string prefab_path = null;
            string class_name = null;
            string parent_class_name = null;

            foreach (string line in all_lines)
            {
                var nameMatchResult = C_CLASS_NAME_REG.Match(line);
                if(nameMatchResult.Success)
                {
                    class_name = nameMatchResult.Groups["class_name"].ToString();
                    parent_class_name = nameMatchResult.Groups["parent_class_name"].ToString();
                    continue;
                }
                var matchResult = C_ASSET_PATH_REG.Match(line);
                if(matchResult.Success)
                {
                    prefab_path = matchResult.Groups["path"].ToString().Trim();
                    break;
                }                
            }

            EdUIViewConf ret = new EdUIViewConf();
            ret.ClassName = class_name;
            ret.PrefabPath = prefab_path;
            ret.ParentClassName = parent_class_name;
            return ret;
        }
    }
}
