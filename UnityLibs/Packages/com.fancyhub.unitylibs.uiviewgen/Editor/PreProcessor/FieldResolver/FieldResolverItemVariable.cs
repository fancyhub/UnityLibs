/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/08/10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.ViewGenerate.Ed
{

    public class FieldResolverItemVariable : IFieldResolver
    {
        public List<EdUIField> CreateFields(EdUIViewGenContext context, EdUIView view)
        {
            List<EdUIField> field_list = new List<EdUIField>();
            Transform root = view.Prefab.transform;
            UIItemVariable[] item_variables = view.Prefab.GetComponentsInChildren<UIItemVariable>(true);

            foreach (UIItemVariable item in item_variables)
            {
                if (item.ExportObject == null)
                    continue;

                //这个导出组件本身不属于自己
                var item_type = EdUIViewGenPrefabUtil.GetComponentRelation(root, item);                
                if (item_type != EPrefabComponentReleation.CurrentPrefab && item_type != EPrefabComponentReleation.CurrentPrefab_NestedPrefabRoot)
                {
                    continue;
                }

                if (item.ExportObject is GameObject obj)
                {
                    var obj_type = EdUIViewGenPrefabUtil.GetComponentRelation(root, obj.transform);
                    switch (obj_type)
                    {
                        case EPrefabComponentReleation.CurrentPrefab:
                            {
                                EdUIField field = _CreateField_Component(root, obj.transform, item);
                                if (field != null)
                                    field_list.Add(field);
                            }
                            break;
                        case EPrefabComponentReleation.CurrentPrefab_NestedPrefabRoot:
                            {
                                string inner_prefab_path = EdUIViewGenPrefabUtil.GetInnerPrefabAssetPath(obj);

                                if (context.Config.IsPrefabPathValid(inner_prefab_path))
                                {

                                    string go_path = EdUIViewGenPrefabUtil.GetHierarchyPath(obj.transform, root);
                                    var dep_conf = context.AddDependPath(inner_prefab_path);

                                    EdUIField field = new EdUIField();
                                    field.Path = go_path;
                                    field.FieldType = EdUIFieldType.CreateSubView(dep_conf);
                                    field.Fieldname = item.GetExportedName();

                                    field_list.Add(field);
                                }
                                else
                                {
                                    EdUIField field = _CreateField_Component(root, obj.transform, item);
                                    if (field != null)
                                        field_list.Add(field);
                                }
                            }
                            break;
                    }
                }
                else if (item.ExportObject is Component comp)
                {
                    EdUIField field = _CreateField_Component(root, comp, item);
                    if (field != null)
                        field_list.Add(field);
                }
            }

            return field_list;
        }

        private static EdUIField _CreateField_Component(Transform root, Component target, UIItemVariable item_v)
        {

            string go_path = EdUIViewGenPrefabUtil.GetHierarchyPath(target.transform, root);

            EdUIField field = new EdUIField();
            field.Path = go_path;
            field.Fieldname = item_v.GetExportedName();
            field.FieldType = EdUIFieldType.CreateComponent(item_v.GetExportObjectType());

            return field;
        }
    }
}
