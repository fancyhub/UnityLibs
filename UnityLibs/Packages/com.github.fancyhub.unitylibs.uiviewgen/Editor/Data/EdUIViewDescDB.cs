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
        //public UIViewGeneratorConfig Config;
        private List<EdUIViewDesc> _AllDesc = new();

        //key is cs prefab name, Lower
        private Dictionary<string, EdUIViewDesc> _PrefabName2Desc = new();

        // key is prefab path
        private Dictionary<string, EdUIViewDesc> _Path2Desc = new();

        public EdUIViewDescDB(List<EdUIViewDesc> list)
        {
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

        public EdUIViewDesc GetCommonBase(EdUIViewDesc prefab_name1, EdUIViewDesc prefab_name2)
        {
            if (prefab_name1 == prefab_name2)
                return prefab_name1;

            if (prefab_name1 == null || prefab_name2 == null)
                return null;

            GetParentClassList(prefab_name1, ref _parent_class_list0);
            GetParentClassList(prefab_name2, ref _parent_class_list1);

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
            GetParentClassList(self, ref _parent_class_list0);
            for (int i = _parent_class_list0.Count - 2; i >= 0; i--)
            {
                if (parent == _parent_class_list1[i])
                    return true;
            }
            return false;
        }


        //0: 是Base, 最后一个是自己
        public void GetParentClassList(EdUIViewDesc self, ref List<EdUIViewDesc> out_list)
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
            _Path2Desc.Add(conf.PrefabPath, conf);
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
            _Path2Desc.Clear();
            _PrefabName2Desc.Clear();

            foreach (EdUIViewDesc desc in _AllDesc)
            {
                _Path2Desc.Add(desc.PrefabPath, desc);
                _PrefabName2Desc.Add(desc.PrefabName.ToLower(), desc);
            }
        }
    }
}
