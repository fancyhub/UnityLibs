/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FH.UI.View.Gen.ED
{
    /// <summary>
    /// 用来自动生成 UI 代码的
    /// </summary>
    public static class EdUIViewGen
    {
        public static void GenCode(EdUIViewData data)
        {
            EdUIFactory factory = new EdUIFactory(data);

            List<EdUIView> comp_list = _GenViews(data, factory);

            _LinkParentSubViews(comp_list,data.Config);
            _CreateListFields(comp_list, factory);
            _InitPrefabPathMono(comp_list);

            foreach (EdUIView view in comp_list)
            {
                EdUIViewCodeExporter_CSharp.Export(data.Config,view);
            }
        }

        //初始化prefab上用来保存路径的mono脚本
        private static void _InitPrefabPathMono(List<EdUIView> views)
        {
            if (views.Count == 0)
            {
                UnityEngine.Debug.LogErrorFormat("empty comp list");
                return;
            }

            foreach (var c in views)
            {
                var asset_prefab = c.Prefab;
                var mono = _GetOrCreateView(asset_prefab, c.Conf.PrefabPath);

                //如果脚本没有变化，那么就不需要更新，主要是考虑到效率问题
                //因为更新某个prefab之后，会导致所有引用到他的prefab也跟着一起reimport
                bool changed = _RefreshMono(c, mono, asset_prefab);
                if (!changed)
                    continue;

                //存回到prefab asset里
                EditorUtility.SetDirty(mono);
                PrefabUtility.SavePrefabAsset(asset_prefab);
            }
        }

        public static bool _RefreshMono(EdUIView view, UIViewReference mono, GameObject asset_prefab)
        {
            //1. 先检查prefab name和key list。如果有变化，那么清空掉重建
            bool changed = _CheckChanged(view, mono, out var new_name);
            if (changed)
            {
                mono._prefab_name = new_name;
                mono.Clear();
                //1.1 清空之后，把key建好，后面就可以直接处理obj的部分了
                foreach (var field in view.Fields)
                {
                    string field_name = field.Fieldname;
                    mono.EdAdd(field_name, null);
                }
            }

            //2. 单独处理obj
            foreach (var field in view.Fields)
            {
                string field_name = field.Fieldname;
                string field_path = field.Path;

                var trans = asset_prefab.transform.Find(field_path);
                UnityEngine.Debug.AssertFormat(null != trans, "cant find obj with path [{0}].please check your prefab!", field.Path);

                var cur = trans.gameObject;
                var ori = mono.GetObj(field_name);
                if (null != ori && ori == cur)
                    continue;

                changed = true;
                mono.EdSet(field_name, cur);
            }

            return changed;
        }

        public static bool _CheckChanged(EdUIView c, UIViewReference view_ref, out string new_name)
        {
            //1.先检查名字是否有变化
            new_name = Path.GetFileNameWithoutExtension(c.Conf.PrefabPath);
            bool name_equal = string.Equals(view_ref._prefab_name, new_name);
            if (!name_equal)
                return true;

            //2.再检查成员变量
            //2.1 先检查数量是否一致,如果不一致，那么重建
            if (view_ref._objs.Length != c.Fields.Count)
                return true;

            //2.2 再检查名字是否都存在
            foreach (var field in c.Fields)
            {
                //把field name当作key，然后保存obj到mono脚本上
                string field_name = field.Fieldname;
                if (!view_ref.Exist(field_name))
                    return true;
            }

            return false;
        }

        public static UIViewReference _GetOrCreateView(GameObject prefab, string asset_path)
        {
            UIViewReference ret = EdUIViewGenPrefabUtil.GetViewReference(prefab, true);
            if (null == ret)
            {
                EdUIViewGenPrefabUtil.AddComponent(prefab, asset_path);
                ret = EdUIViewGenPrefabUtil.GetViewReference(prefab, true);
            }

            return ret;
        }

        public static void _LinkParentSubViews(List<EdUIView> comp_list, UIViewGenConfig config)
        {
            //把父类link起来
            foreach (EdUIView comp in comp_list)
            {
                comp.ParentView = _FindViewWithClassName(comp_list, comp.ParentClass,config);
            }
        }

        public static void _CreateListFields(List<EdUIView> view_list, EdUIFactory factory)
        {
            //1. 先创建
            foreach (var p in view_list)
            {
                p.ListFields = factory.CreateListField(p);
            }

            //2. 把field list link起来
            // 因为可能 父类里面已经声明了该对象，需要link起来
            // 比如 UIViewA 还有 List<Lable> _lbl_list;
            // UIViewB: UIViewA, B里面也有一个 _lbl,需要添加父结构里面的 _lbl_list 里面
            foreach (EdUIView view in view_list)
            {
                view.PostProcessListFields(factory._data.ConfigDB);
            }
        }

        public static List<EdUIView> _GenViews(EdUIViewData data, EdUIFactory factory)
        {
            List<EdUIView> ret = new List<EdUIView>();
            for (; ; )
            {
                EdUIViewConf next_conf = data.GetNextPrefabConf();
                if (null == next_conf)
                    break;

                EdUIView view = factory.CreateView(next_conf);
                view.Fields = factory.CreateFields(view.Prefab);
                ret.Add(view);
            }

            return ret;
        }

        public static EdUIView _FindViewWithClassName(List<EdUIView> view_list, string class_name, UIViewGenConfig config)
        {
            if (class_name == config.BaseClassName)
                return null;
            foreach (EdUIView view in view_list)
            {
                if (view.Conf.ClassName == class_name)
                    return view;
            }

            Debug.LogError("can't find parent class " + class_name);
            return null;
        }
    }
}