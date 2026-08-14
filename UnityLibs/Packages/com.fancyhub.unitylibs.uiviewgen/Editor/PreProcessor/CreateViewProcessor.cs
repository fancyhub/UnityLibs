/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace FH.UI.ViewGenerate.Ed
{
    public class CreateViewProcessor : IViewGeneratePreprocessor
    {
        private IFieldResolver _FieldResolver;

        public CreateViewProcessor(IFieldResolver field_resolver)
        {
            _FieldResolver = field_resolver;
        }

        public void Process(EdUIViewGenContext context)
        {
            List<EdUIView> view_list = new List<EdUIView>();
            for (; ; )
            {
                EdUIViewDesc next_conf = context.GetNextPrefabConf();
                if (null == next_conf)
                    break;

                EdUIView view = _CreateView(context, next_conf);
                var items = _FieldResolver.CollectFields(view.Prefab);
                view.Fields = _CreateFields(context, view, items);
                view_list.Add(view);
            }

            context.ViewList = view_list;
        }

        public static List<EdUIField> _CreateFields(EdUIViewGenContext context, EdUIView view, List<FieldResolverItem> items)
        {
            List<EdUIField> ret = new List<EdUIField>();
            var root_tran = view.Prefab.transform;

            foreach (var p in items)
            {
                if (p.SubView)
                {
                    string sub_view_path = EdUIViewGenPrefabUtil.GetSubViewPrefabPath(p.GetTargetGameObject());

                    if (context.Config.IsPrefabPathValid(sub_view_path))
                    {
                        var dep_conf = context.AddDependPath(sub_view_path);

                        EdUIField field = new EdUIField();
                        field.TargetObj = p.TargetObject;
                        field.FieldType = EdUIFieldType.CreateSubView(dep_conf);
                        field.Fieldname = p.FieldName;
                        ret.Add(field);
                    }
                    else //路径不合法, 退化到普通组件
                    {
                        string go_path = EdUIViewGenPrefabUtil.GetHierarchyPath(p.GetTargetGameObject().transform, root_tran);
                        if (sub_view_path == null)
                            UnityEngine.Debug.LogError($"Prefab {view.Prefab.name} 里面的对象 {p.FieldName} 不是prefab, 退化到了普通对象", view.Prefab);
                        else
                            UnityEngine.Debug.LogError($"Prefab {view.Prefab.name} 里面的对象 {p.FieldName}({go_path}) 对应的子View路径不合法 {sub_view_path}, 退化到了普通对象", view.Prefab);


                        EdUIField field = new EdUIField();
                        field.TargetObj = p.TargetObject;
                        field.Fieldname = p.FieldName;
                        field.FieldType = EdUIFieldType.CreateComponent(p.TargetType);
                        ret.Add(field);
                    }
                }
                else
                {
                    EdUIField field = new EdUIField();
                    field.TargetObj = p.TargetObject;
                    field.Fieldname = p.FieldName;
                    field.FieldType = EdUIFieldType.CreateComponent(p.TargetType);
                    ret.Add(field);
                }
            }
            return ret;
        }


        public static EdUIView _CreateView(EdUIViewGenContext context, EdUIViewDesc desc)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(desc.PrefabPath);

            Debug.Assert(prefab != null, "Load Prefab failed " + desc.PrefabPath);
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
                        GameObject parent_prefab = EdUIViewGenPrefabUtil.GetParentPrefab(prefab);
                        string parent_prefab_path = AssetDatabase.GetAssetPath(parent_prefab);
                        if (context.Config.IsPrefabPathValid(parent_prefab_path))
                        {
                            var parent_desc = context.AddDependPath_Variant(parent_prefab_path);
                            EdUIView ret = new EdUIView();
                            ret.Desc = desc;
                            ret.Prefab = prefab;
                            ret.ParentDesc = parent_desc;

                            if (desc.ParentPrefabPath != parent_prefab_path)
                            {
                                desc.SetParentPrefabPath(parent_prefab_path);
                            }
                            return ret;
                        }
                        else
                        {
                            UnityEngine.Debug.LogErrorFormat("Prefab 路径不合法\n Prefab : [{0}] \n Variant Prefab : [{1}]\n", parent_prefab_path, desc.PrefabPath);
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
