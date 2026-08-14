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

        //判断该组件是否属于自己
        public static bool IsComponentBelongSelf(GameObject self_prefab, Component target)
        {
            if (self_prefab == null || target == null)
                return false;

            Transform target_transform = target.transform;
            if (target_transform != self_prefab.transform && !target_transform.IsChildOf(self_prefab.transform))
            {
                return false;
            }

            // If there is no corresponding source component, this component
            // was added by the current prefab (including an override on a
            // nested prefab root). A non-null source means it belongs to the
            // source/parent prefab instead.
            UnityEngine.Object source = PrefabUtility.GetCorrespondingObjectFromSource(target);
            return source == null;
        }

        //判断目标gameobject 是否属于自己
        public static bool IsGameObjectBelongSelf(GameObject self_prefab, GameObject target)
        {
            if (self_prefab == null || target == null)
                return false;

            if (target == self_prefab)
                return true;

            if (!target.transform.IsChildOf(self_prefab.transform))
                return false;

            GameObject outer_root = PrefabUtility.GetOutermostPrefabInstanceRoot(target);

            // 没有嵌套 Prefab，属于当前 Prefab
            if (outer_root == null)
                return true;

            // 只有嵌套 Prefab 根节点属于当前 Prefab
            return outer_root == target;
        }

        //判断目标是否属于一个内嵌prefab的根节点
        public static bool IsGameObjectSubViewRoot(GameObject self_prefab, GameObject target)
        {
            if (self_prefab == null || target == null)
                return false;

            if (target == self_prefab)
                return false;

            if (!target.transform.IsChildOf(self_prefab.transform))
                return false;

            return PrefabUtility.GetOutermostPrefabInstanceRoot(target) == target;
        }


        //获取父prefab
        public static GameObject GetParentPrefab(GameObject obj)
        {
            if (PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.Variant)
                return null;

            return PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(obj);
        }


        //获取subview 对应的 prefab
        public static GameObject GetSubViewPrefab(GameObject obj)
        {
            if (obj == null)
                return null;

            GameObject prefab_root = PrefabUtility.GetNearestPrefabInstanceRoot(obj);

            if (prefab_root == null)
                return null;

            return PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(prefab_root);

        }

        //获取subview 对应的prefab path
        public static string GetSubViewPrefabPath(GameObject obj)
        {
            GameObject p = GetSubViewPrefab(obj);
            if (p == null)
                return null;
            return AssetDatabase.GetAssetPath(p);
        }


        #region Hierachy Path
        private static System.Text.StringBuilder _string_builder = new();
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
