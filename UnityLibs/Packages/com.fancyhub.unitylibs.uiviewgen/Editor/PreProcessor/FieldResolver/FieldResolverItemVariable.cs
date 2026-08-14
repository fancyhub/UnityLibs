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
                if (!EdUIViewGenPrefabUtil.IsComponentBelongSelf(root.gameObject, item))
                    continue;

                ret.Add(new FieldResolverItem()
                {
                    TargetObject = item.ExportObject,
                    TargetType = item.GetExportObjectType(),
                    FieldName = item.GetExportedName(),
                    SubView = false,
                });                 
            }

            return ret;
        }         
    }
}
