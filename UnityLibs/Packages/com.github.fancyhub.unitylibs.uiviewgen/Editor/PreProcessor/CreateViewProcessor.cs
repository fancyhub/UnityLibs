/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FH.UI.ViewGenerate.Ed
{
    public class CreateViewProcessor : IViewGeneratePreprocessor
    {
        public void Process(EdUIViewGenContext context)
        {
            List<EdUIView> view_list = new List<EdUIView>();
            for (; ; )
            {
                EdUIViewDesc next_conf = context.GetNextPrefabConf();
                if (null == next_conf)
                    break;
                EdUIView view = CreateView(context, next_conf);
                view_list.Add(view);
            }

            context.ViewList = view_list;
        }


        public static EdUIView CreateView(EdUIViewGenContext context, EdUIViewDesc desc)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(desc.PrefabPath);

            var type = PrefabUtility.GetPrefabAssetType(prefab);
            switch (type)
            {
                case PrefabAssetType.Regular:
                    {
                        EdUIView ret = new EdUIView();
                        ret.Desc = desc;
                        ret.Prefab = prefab;
                        ret.ParentDesc = null;
                        return ret;
                    }

                case PrefabAssetType.Variant:
                    {
                        GameObject orig_prefab = EdUIViewGenPrefabUtil.GetOrigPrefabWithVariant(prefab);
                        string orig_prefab_path = AssetDatabase.GetAssetPath(orig_prefab);
                        if (context.Config.IsPrefabPathValid(orig_prefab_path))
                        {
                            var parent_desc = context.AddDependPath_Variant(orig_prefab_path);
                            EdUIView ret = new EdUIView();
                            ret.Desc = desc;
                            ret.Prefab = prefab;
                            ret.ParentDesc = parent_desc;
                            return ret;
                        }
                        else
                        {
                            UnityEngine.Debug.LogErrorFormat("Prefab 路径不合法\n Prefab : [{0}] \n Variant Prefab : [{1}]\n", orig_prefab_path, desc.PrefabPath);
                            EdUIView ret = new EdUIView();
                            ret.Desc = desc;
                            ret.Prefab = prefab;
                            ret.ParentDesc = null;
                            return ret;
                        }
                    }

                default:
                    Debug.LogErrorFormat(prefab, " 未知的类型 {0}", type);
                    return null;
            }
        }

    }
}