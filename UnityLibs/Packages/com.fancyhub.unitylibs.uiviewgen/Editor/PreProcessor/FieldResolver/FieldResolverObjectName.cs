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
        private UIViewGeneratorConfig _Config;
        public FieldResolverObjectName(UIViewGeneratorConfig Config)
        {
            _Config = Config;
        }

        public List<FieldResolverItem> CollectFields(GameObject prefab)
        {
            List<FieldResolverItem> ret = new List<FieldResolverItem>();
            Transform root_tran = prefab.transform;
            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            foreach (Transform tran in transforms)
            {
                string name = tran.name;
                if (tran == root_tran) //自己就是根节点
                {
                    var c = _Config.FieldResolverObjectName.GetComponent(tran, root_tran);
                    ret.Add(new FieldResolverItem()
                    {
                        TargetObject = c,
                        TargetType = c.GetType(),
                        FieldName = _GenFiledName(name),
                        SubView = false,
                    });
                    continue;
                }

                //如果不是根节点，必须要以 下划线开头才能导出 _
                if (!name.StartsWith("_"))
                    continue;

                //这个对象不属于自己
                if (!EdUIViewGenPrefabUtil.IsGameObjectBelongSelf(root_tran.gameObject, tran.gameObject))
                    continue;

                {
                    bool is_sub_view = EdUIViewGenPrefabUtil.IsGameObjectSubViewRoot(root_tran.gameObject, tran.gameObject);
                    var c = _Config.FieldResolverObjectName.GetComponent(tran, root_tran);
                    ret.Add(new FieldResolverItem()
                    {
                        TargetObject = c,
                        TargetType = c.GetType(),
                        FieldName = _GenFiledName(name),
                        SubView = is_sub_view,
                    });
                }

            }
            return ret;
        }

        private static string _GenFiledName(string name)
        {
            if (name.StartsWith("_"))
                return name;
            return "_" + name;
        }
    }
}
