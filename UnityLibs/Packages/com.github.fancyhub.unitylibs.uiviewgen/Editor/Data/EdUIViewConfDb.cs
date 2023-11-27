/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using System.IO;

namespace FH.UI.View.Gen.ED
{
    /// <summary>
    /// 配置，描述了 EdSmUiConf的集合
    /// 给了一些搜索，添加，保存，加载的方法
    /// </summary>
    public class EdUIViewConfDb
    {

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
            string[] files = System.IO.Directory.GetFiles(config.CodeFolder,"*"+ EdUIViewConf.C_CS_RES_SUFFIX);
            foreach (string file in files)
            {
                EdUIViewConf conf = EdUIViewConf.ParseFromCsFile(file);
                if (conf != null)
                    ret._all_confs.Add(conf);
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
                    UnityEngine.Debug.LogError("Error " + conf.ClassName + " : " + conf.ParentClassName);
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

            UnityEngine.Debug.LogErrorFormat("出现了重复,prefab可能移动过目录，但生成的res代码文件中路径还是原来的: {0} \nPath1: {1}\nPath2: {2}\n\n"
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
    }
}
