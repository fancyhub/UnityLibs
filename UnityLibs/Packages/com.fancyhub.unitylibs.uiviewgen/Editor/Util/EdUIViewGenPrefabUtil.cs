/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FH.UI.ViewGenerate.Ed
{
    public enum EPrefabComponentReleation
    {
        /// <summary>
        /// 属于prefab 自己的, 需要导出
        /// </summary>
        CurrentPrefab,

        /// <summary>
        /// 属于 内部prefab的 的root 节点, 需要导出(但是 SubView)
        /// </summary>
        CurrentPrefab_NestedPrefabRoot,

        /// <summary>
        /// 属于内部 prefab的 节点, 不要导出(属于SubView自己的Field)
        /// </summary>
        NestedPrefab,

        /// <summary>
        /// 整个prefab的根节点，但是该节点也是 一个 variant变体, 是父prefab的对象,不需要导出
        /// </summary>
        ParentPrefab,
    }

    public static class EdUIViewGenPrefabUtil
    {
        /// <summary>
        /// 添加脚本到asset prefab上。
        /// 添加过程中得到的脚本是实例化的对象上的，add方法结束后会删掉实例化的prefab，这个脚本也会变成空。
        /// 所以如果想获取脚本可以再调用一次get方法
        /// </summary>        
        public static void AddComponent(GameObject asset_prefab, string asset_path)
        {
            //目前增加component必须要实例化才可以
            var inst_obj = PrefabUtility.InstantiatePrefab(asset_prefab) as GameObject;
            var t = inst_obj.AddComponent<UIViewCompReference>();
            PrefabUtility.ApplyAddedComponent(t, asset_path, InteractionMode.AutomatedAction);
            UnityEngine.Object.DestroyImmediate(inst_obj);
        }

        /// <summary>
        /// 脚本可以直接在asset里面直接删除而不必要实例化         
        /// </summary>      
        public static bool RemoveComponent(GameObject prefab_root, bool belong_self)
        {
            var comp = GetViewReference(prefab_root, belong_self);
            if (null == comp)
            {
                Debug.LogErrorFormat("component type [{0}] not exist in prefab_root [{1}]", typeof(UIViewCompReference), prefab_root);
                return false;
            }

            UnityEngine.Object.DestroyImmediate(comp, true);
            return true;
        }

        /// <summary>
        /// 获取prefab上的脚本。
        /// 如果belong_self为false，那么默认会返回第一个。
        /// prefab_root: inst prefab或者asset prefab都可以        
        /// </summary>        
        public static UIViewCompReference GetViewReference(GameObject prefab_root, bool belong_self)
        {
            UIViewCompReference ret = null;
            UIViewCompReference[] comp_list = prefab_root.GetComponents<UIViewCompReference>();
            if (!belong_self)
            {
                if (comp_list.Length == 0)
                    return null;
                return comp_list[0];
            }

            foreach (UIViewCompReference comp in comp_list)
            {
                //拿到mono的原始版本，这时候再去获取gameobject name才是他本来的名字
                UIViewCompReference ee = PrefabUtility.GetCorrespondingObjectFromOriginalSource(comp);
                if (!string.Equals(ee.gameObject.name, prefab_root.name))
                    continue;

                //这里不能用origin，origin是资源上的对象，并不是实例化出来的prefab上面的对象。可以从instance id上看出来
                //修改origin是不会被保存下来的，所以这里用mono
                ret = comp;
                break;
            }

            return ret;
        }

        public static EPrefabComponentReleation GetComponentRelation(Transform prefab_root, Component comp)
        {
            if (prefab_root == null || comp == null)
                return EPrefabComponentReleation.CurrentPrefab;

            GameObject comp_obj = comp.gameObject;
            GameObject prefab_outer = PrefabUtility.GetOutermostPrefabInstanceRoot(comp_obj);

            if (prefab_outer == null)
                return EPrefabComponentReleation.CurrentPrefab;

            if (prefab_outer != comp_obj)
                return EPrefabComponentReleation.NestedPrefab;

            if (comp_obj == prefab_root.gameObject)
                return EPrefabComponentReleation.ParentPrefab;

            return EPrefabComponentReleation.CurrentPrefab_NestedPrefabRoot;
        }        
         

        public static GameObject GetOrigPrefabWithVariant(GameObject obj)
        {
            if (PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.Variant)
                return null;

            return PrefabUtility.GetCorrespondingObjectFromSource(obj);
        }

         
        public static string GetInnerPrefabAssetPath(GameObject obj)
        {
            GameObject prefab_inner = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab_inner);
        }

        #region Hierachy Path
        private static System.Text.StringBuilder _string_builder = new ();
        /// <summary>
        /// 不包括root
        /// </summary>
        public static string GetHierarchyPath(Transform self, Transform root)
        {
            if (null == self)
                return string.Empty;

            _string_builder.Length = 0;
            _GetHierarchyPath(self, self, root, _string_builder);
            return _string_builder.ToString();
        }

        private static void _GetHierarchyPath(Transform target, Transform obj, Transform root, System.Text.StringBuilder sb)
        {
            if (obj == root)
            {
                //不包括root节点
                //if (null != obj)
                //{
                // sb.Append(obj.name);
                //}
                return;
            }

            if (null == obj)
            {
                Debug.LogErrorFormat("Root {0} 不是 obj {1} 的 根节点", root, target);
                return;
            }

            _GetHierarchyPath(target, obj.parent, root, sb);

            if (sb.Length > 0)
            {
                sb.Append('/');
            }
            sb.Append(obj.name);
        }
        #endregion

    }
}
