/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.ViewGenerate.Ed
{
    public class FieldResolverObjectName : IFieldResolver
    {
        public List<EdUIField> CreateFields(EdUIViewGenContext context, EdUIView view)
        {
            List<EdUIField> field_list = new List<EdUIField>();
            Transform root_tran = view.Prefab.transform;
            Transform[] transforms = view.Prefab.GetComponentsInChildren<Transform>(true);
            foreach (Transform tran in transforms)
            {
                EPrefabComponentReleation obj_type = EdUIViewGenPrefabUtil.GetComponentRelation(root_tran, tran);
                switch (obj_type)
                {
                    case EPrefabComponentReleation.CurrentPrefab:
                        {
                            EdUIField field = _CreateField_Component(context, root_tran, tran);
                            if (null != field)
                                field_list.Add(field);
                        }
                        break;


                    case EPrefabComponentReleation.CurrentPrefab_NestedPrefabRoot:
                        {
                            EdUIField field = _CreateField_Prefab(context, root_tran, tran);
                            if (null != field)
                                field_list.Add(field);
                        }
                        break;


                    case EPrefabComponentReleation.ParentPrefab:
                        {
                            //属于 其他prefab的对象
                        }
                        break;

                    case EPrefabComponentReleation.NestedPrefab:
                        {
                            //不处理,该节点由 子 prefab 来处理
                        }
                        break;

                    default:
                        Debug.LogError("Error:" + obj_type);
                        break;
                }
            }

            return field_list;
        }

        private static EdUIField _CreateField_Prefab(EdUIViewGenContext context, Transform root, Transform target)
        {
            if (!target.name.StartsWith("_"))
                return null;

            string inner_prefab_path = EdUIViewGenPrefabUtil.GetInnerPrefabAssetPath(target.gameObject);

            if (context.Config.IsPrefabPathValid(inner_prefab_path))
            {
                string go_path = EdUIViewGenPrefabUtil.GetHierarchyPath(target, root);
                var dep_conf = context.AddDependPath(inner_prefab_path);

                EdUIField field = new EdUIField();
                field.Path = go_path;
                field.FieldType = EdUIFieldType.CreateSubView(dep_conf);
                field.Fieldname = _GenFiledName(target.name);

                return field;
            }
            else //如果子 prefab 在别的目录,就当作普通的GameObject 来处理
            {
                UnityEngine.Debug.LogErrorFormat("Prefab 里面的对象 {0} 对应的路径不合法 {1}", target.name, inner_prefab_path);
                string target_name = target.name;
                //如果不是根节点，必须要以 下划线开头才能导出 _
                if (root != target && !target_name.StartsWith("_"))
                    return null;

                Component component = context.Config.FieldResolverObjectName.GetComponent(target, root);
                if (null == component)
                    return null;

                string go_path = EdUIViewGenPrefabUtil.GetHierarchyPath(target, root);
                string field_name = _GenFiledName(target_name);

                EdUIField field = new EdUIField();
                field.Path = go_path;
                field.Fieldname = field_name;
                field.FieldType = EdUIFieldType.CreateComponent(component.GetType());
                return field;
            }
        }


        private static EdUIField _CreateField_Component(EdUIViewGenContext context, Transform root, Transform target)
        {
            string target_name = target.name;
            //如果不是根节点，必须要以 下划线开头才能导出 _
            if (root != target && !target_name.StartsWith("_"))
                return null;

            Component component = context.Config.FieldResolverObjectName.GetComponent(target, root);
            if (null == component)
                return null;

            string go_path = EdUIViewGenPrefabUtil.GetHierarchyPath(target, root);
            string field_name = _GenFiledName(target_name);

            EdUIField field = new EdUIField();
            field.Path = go_path;
            field.Fieldname = field_name;
            field.FieldType = EdUIFieldType.CreateComponent(component.GetType());

            return field;
        }

        private static string _GenFiledName(string name)
        {
            if (name.StartsWith("_"))
                return name;
            return "_" + name;
        }
    }
}
