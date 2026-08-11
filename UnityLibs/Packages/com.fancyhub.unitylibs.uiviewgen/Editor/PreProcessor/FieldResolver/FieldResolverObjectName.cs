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
                        TargetComp = c,
                        TargetType = c.GetType(),
                        FieldName = _GenFiledName(name),
                        SubView = false,
                    });
                    continue;
                }

                //如果不是根节点，必须要以 下划线开头才能导出 _
                if (!name.StartsWith("_"))
                    continue;


                EPrefabComponentReleation obj_type = EdUIViewGenPrefabUtil.GetComponentRelation(root_tran, tran);
                switch (obj_type)
                {
                    case EPrefabComponentReleation.CurrentPrefab:
                        {
                            var c = _Config.FieldResolverObjectName.GetComponent(tran, root_tran);
                            ret.Add(new FieldResolverItem()
                            {
                                TargetComp = c,
                                TargetType = c.GetType(),
                                FieldName = _GenFiledName(name),
                                SubView = false,
                            });
                        }
                        break;


                    case EPrefabComponentReleation.CurrentPrefab_NestedPrefabRoot:
                        {
                            var c = _Config.FieldResolverObjectName.GetComponent(tran, root_tran);
                            ret.Add(new FieldResolverItem()
                            {
                                TargetComp = c,
                                TargetType = c.GetType(),
                                FieldName = _GenFiledName(name),
                                SubView = true,
                            });
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
