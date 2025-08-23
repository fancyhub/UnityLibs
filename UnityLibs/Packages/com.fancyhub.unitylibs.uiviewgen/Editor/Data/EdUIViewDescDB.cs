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

namespace FH.UI.ViewGenerate.Ed
{
    /// <summary>
    /// 配置，描述了 EdSmUiConf的集合
    /// 给了一些搜索，添加，保存，加载的方法
    /// </summary>
    public class EdUIViewDescDB
    {
        //public UIViewGeneratorConfig Config;
        private List<EdUIViewDesc> _AllDesc = new();

        //key is cs prefab name, Lower
        private Dictionary<string, EdUIViewDesc> _PrefabName2Desc = new();


        public EdUIViewDescDB(List<EdUIViewDesc> list)
        {
            _AllDesc.AddRange(list);
            _BuildCache();
        }

        public List<EdUIViewDesc> GetAllDesc()
        {
            return _AllDesc;
        }
        public List<string> GetPathList()
        {
            List<string> list = new List<string>(_AllDesc.Count);
            foreach (var p in _AllDesc)
            {
                list.Add(p.PrefabPath);
            }
            return list;
        }

        public EdUIViewDesc FindDescWithPrefabPath(string prefab_path)
        {
            if (string.IsNullOrEmpty(prefab_path))
            {
                UnityEngine.Debug.LogError($"\"{prefab_path}\" is null or empty");
                return null;
            }

            string name = Path.GetFileNameWithoutExtension(prefab_path);

            EdUIViewDesc ret = null;
            _PrefabName2Desc.TryGetValue(name.ToLower(), out ret);
            return ret;
        }

        public EdUIViewDesc FindDescWithPrefabName(string prefab_name)
        {
            if (string.IsNullOrEmpty(prefab_name))
                return null;
            EdUIViewDesc ret = null;
            _PrefabName2Desc.TryGetValue(prefab_name.ToLower(), out ret);
            return ret;
        }

        private static List<EdUIViewDesc> _parent_class_list0 = new List<EdUIViewDesc>();
        private static List<EdUIViewDesc> _parent_class_list1 = new List<EdUIViewDesc>();

        /// <summary>
        /// 获取两个view共同的父类
        /// </summary>
        public EdUIViewDesc GetCommonBase(EdUIViewDesc prefab_name1, EdUIViewDesc prefab_name2)
        {
            if (prefab_name1 == prefab_name2)
                return prefab_name1;

            if (prefab_name1 == null || prefab_name2 == null)
                return null;

            _GetParentClassList(prefab_name1, ref _parent_class_list0);
            _GetParentClassList(prefab_name2, ref _parent_class_list1);

            if (_parent_class_list0.Count == 0 || _parent_class_list1.Count == 0)
            {
                return null;
            }

            int count = System.Math.Min(_parent_class_list0.Count, _parent_class_list1.Count);
            for (int i = count - 1; i >= 0; i--)
            {
                if (_parent_class_list0[i] == _parent_class_list1[i])
                    return _parent_class_list0[i];
            }

            return null;
        }

        public bool IsParentClass(EdUIViewDesc self, EdUIViewDesc parent)
        {
            if (self == parent)
                return true;
            if (parent == null)
                return true;

            for (; ; )
            {
                if (string.IsNullOrEmpty(self.ParentPrefabName))
                    return false;

                var temp = FindDescWithPrefabName(self.ParentPrefabName);
                if (temp == null)
                {
                    UnityEngine.Debug.LogError("Error " + self.PrefabName + " : " + self.ParentPrefabName);
                    return false;
                }
                self = temp;
                if (self == parent)
                    return true;
            }
        }

        /// <summary>
        /// 最后一个是自己, 继承链
        /// </summary>        
        private void _GetParentClassList(EdUIViewDesc self, ref List<EdUIViewDesc> out_list)
        {
            out_list.Clear();

            EdUIViewDesc temp = self;
            for (; ; )
            {
                out_list.Add(temp);

                if (string.IsNullOrEmpty(temp.ParentPrefabName))
                    break;

                var parent = FindDescWithPrefabName(temp.ParentPrefabName);
                if (parent == null)
                {
                    UnityEngine.Debug.LogError("Error " + temp.PrefabName + " : " + temp.ParentPrefabName);
                    //出错了
                    out_list.Clear();
                    break;
                }
                temp = parent;
            }

            out_list.Reverse();
        }

        public bool AddConf(EdUIViewDesc conf)
        {
            _CheckExist(conf);
            _PrefabName2Desc.Add(conf.PrefabName.ToLower(), conf);
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
            _PrefabName2Desc.TryGetValue(desc.PrefabName.ToLower(), out old_desc);
            if (null == old_desc)
                return;

            UnityEngine.Debug.LogErrorFormat("出现了重复,prefab可能移动过目录，但生成的res代码文件中路径还是原来的: {0} \nPath1: {1}\nPath2: {2}\n\n"
                , desc.PrefabName
                , desc.PrefabPath
                , old_desc.PrefabPath);
        }

        private void _BuildCache()
        {
            _PrefabName2Desc.Clear();

            foreach (EdUIViewDesc desc in _AllDesc)
            {
                _PrefabName2Desc.Add(desc.PrefabName.ToLower(), desc);
            }
        }
    }
}
