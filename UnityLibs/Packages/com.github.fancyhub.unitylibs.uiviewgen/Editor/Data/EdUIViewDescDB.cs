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
    /// 配置，描述了 EdSmUiConf的集合
    /// 给了一些搜索，添加，保存，加载的方法
    /// </summary>
    public class EdUIViewDescDB
    {
        public UIViewGeneratorConfig Config;
        private List<EdUIViewDesc> _AllDesc = new();

        //key is class name
        private Dictionary<string, EdUIViewDesc> _Class2Desc = new();
        // key is prefab path
        private Dictionary<string, EdUIViewDesc> _Path2Desc = new();

        public EdUIViewDescDB(UIViewGeneratorConfig config,List<EdUIViewDesc> list)
        {
            Config = config;
            _AllDesc.AddRange(list);

            _BuildCache();
        }         

        public List<EdUIViewDesc> GetAllDesc()
        {
            return _AllDesc;
        }
        public IEnumerable<string> GetPathList()
        {
            return _Path2Desc.Keys;
        }

        public EdUIViewDesc FindDescWithPrefabPath(string prefab_path)
        {
            if (string.IsNullOrEmpty(prefab_path))
                return null;

            EdUIViewDesc ret = null;
            _Path2Desc.TryGetValue(prefab_path, out ret);
            return ret;
        }

        public EdUIViewDesc FindDescWithClassName(string class_name)
        {
            if (string.IsNullOrEmpty(class_name))
                return null;
            EdUIViewDesc ret = null;
            _Class2Desc.TryGetValue(class_name, out ret);
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
                return Config.Csharp.BaseClassName;
            }

            int count = System.Math.Min(_parent_class_list0.Count, _parent_class_list1.Count);
            for (int i = count - 1; i >= 0; i--)
            {
                if (_parent_class_list0[i] == _parent_class_list1[i])
                    return _parent_class_list0[i];
            }

            return Config.Csharp.BaseClassName;
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

            EdUIViewDesc desc = FindDescWithClassName(class_name);
            if (desc == null)
                return;

            for (; ; )
            {
                out_list.Add(desc.ClassName);

                if (desc.ParentClassName == Config.Csharp.BaseClassName)
                {
                    out_list.Add(desc.ParentClassName);
                    break;
                }
                desc = FindDescWithClassName(desc.ParentClassName);
                if (desc == null)
                {
                    UnityEngine.Debug.LogError("Error " + desc.ClassName + " : " + desc.ParentClassName);
                    //出错了
                    out_list.Clear();
                    break;
                }
            }

            out_list.Reverse();
        }

        public bool AddConf(EdUIViewDesc conf)
        {
            _CheckExist(conf);
            _Path2Desc.Add(conf.PrefabPath, conf);
            _Class2Desc.Add(conf.ClassName, conf);
            _AllDesc.Add(conf);
            return true;
        }

        public void RemoveInvalidatePath()
        {
            bool changed = false;
            for (int i = _AllDesc.Count - 1; i >= 0; --i)
            {
                EdUIViewDesc desc = _AllDesc[i];
                if (!File.Exists(desc.PrefabPath))
                {
                    _AllDesc.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed)
                _BuildCache();
        }


        private void _CheckExist(EdUIViewDesc desc)
        {
            EdUIViewDesc old_desc;
            _Class2Desc.TryGetValue(desc.ClassName, out old_desc);
            if (null == old_desc)
                return;

            UnityEngine.Debug.LogErrorFormat("出现了重复,prefab可能移动过目录，但生成的res代码文件中路径还是原来的: {0} \nPath1: {1}\nPath2: {2}\n\n"
                , desc.ClassName
                , desc.PrefabPath
                , old_desc.PrefabPath);
        }

        private void _BuildCache()
        {
            _Path2Desc.Clear();
            _Class2Desc.Clear();

            foreach (EdUIViewDesc desc in _AllDesc)
            {
                _Path2Desc.Add(desc.PrefabPath, desc);
                _Class2Desc.Add(desc.ClassName, desc);
            }
        }
    }

   
}
