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

namespace FH.UI.ViewGenerate.Ed
{
    /// <summary>
    /// 给Prefab 添加 UIViewCompReference 组件
    /// </summary>
    public class ViewCompReferenceProcessor : IViewGeneratePreprocessor
    {
        public void Process(EdUIViewGenContext context)
        {
            if (context.ViewList.Count == 0)
            {
                UnityEngine.Debug.LogErrorFormat("empty comp list");
                return;
            }

            foreach (var view in context.ViewList)
            {
                GameObject asset_prefab = view.Prefab;
                VersionControlUtil.Checkout(view.Desc.PrefabPath);
                var view_ref = _GetOrCreateViewRef(asset_prefab, view.Desc.PrefabPath);

                //如果脚本没有变化，那么就不需要更新，主要是考虑到效率问题
                //因为更新某个prefab之后，会导致所有引用到他的prefab也跟着一起reimport
                bool changed = _RefreshMono(view, view_ref, asset_prefab);
                if (!changed)
                    continue;

                //存回到prefab asset里
                EditorUtility.SetDirty(view_ref);
                PrefabUtility.SavePrefabAsset(asset_prefab);
            }
        }


        private static UIViewCompReference _GetOrCreateViewRef(GameObject prefab, string asset_path)
        {
            UIViewCompReference ret = EdUIViewGenPrefabUtil.GetViewReference(prefab, true);
            if (null == ret)
            {
                EdUIViewGenPrefabUtil.AddComponent(prefab, asset_path);
                ret = EdUIViewGenPrefabUtil.GetViewReference(prefab, true);
            }

            return ret;
        }


        private static bool _RefreshMono(EdUIView view, UIViewCompReference mono, GameObject asset_prefab)
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

        private static bool _CheckChanged(EdUIView c, UIViewCompReference view_ref, out string new_name)
        {
            //1.先检查名字是否有变化
            new_name = Path.GetFileNameWithoutExtension(c.Desc.PrefabPath);
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
    }

}