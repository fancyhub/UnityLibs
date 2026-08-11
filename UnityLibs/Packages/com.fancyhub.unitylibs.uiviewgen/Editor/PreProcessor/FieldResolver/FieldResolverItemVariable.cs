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
        public List<FieldResolverItem> CollectFields(GameObject prefab)
        {
            List<FieldResolverItem> ret = new List<FieldResolverItem>();
            Transform root = prefab.transform;
            UIItemVariable[] item_variables = prefab.GetComponentsInChildren<UIItemVariable>(true);

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

                switch (item.ExportObject)
                {
                    case GameObject obj:
                        ret.Add(new FieldResolverItem()
                        {
                            TargetComp = obj.transform,
                            TargetType = obj.transform.GetType(),
                            FieldName = item.GetExportedName(),
                            SubView = true,
                        });
                        break;

                    case Component comp:
                        ret.Add(new FieldResolverItem()
                        {
                            TargetComp = comp,
                            TargetType = item.GetExportObjectType(),
                            FieldName = item.GetExportedName(),
                            SubView = false,
                        });
                        break;
                }
            }

            return ret;
        }         
    }
}
