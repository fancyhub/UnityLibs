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
    public enum EEDUIObjType
    {
        prefab_self, //属于prefab 自己的
        prefab_inner_root, //属于 内部prefab的 的root 节点
        prefab_inner_obj, //属于内部 prefab的 节点
        prefab_variant, //整个prefab的根节点，但是该节点也是 一个 variant变体
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
            UIViewCompReference[]  comp_list = prefab_root.GetComponents<UIViewCompReference>();
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

        public static EEDUIObjType GetTargetType(Transform target, Transform prefab_root)
        {
            if (!_IsInnerPrefab(target.gameObject, prefab_root.gameObject))
                return EEDUIObjType.prefab_self;

            if (!_IsInnerPrefabRoot(target.gameObject))
                return EEDUIObjType.prefab_inner_obj;

            if (target == prefab_root)
                return EEDUIObjType.prefab_variant;

            return EEDUIObjType.prefab_inner_root;
        }

        /// <summary>
        /// 判断对象是否是 prefab 里面的嵌套 prefab节点
        /// 传入的obj一定是没有实例化出来的，才能保证正确，否则参考下面的接口
        /// </summary>
        private static bool _IsInnerPrefab(GameObject asset_obj, GameObject obj_root)
        {
            GameObject prefab_outer = PrefabUtility.GetOutermostPrefabInstanceRoot(asset_obj);

            //如果prefab_outer 为null，说明是属于该对象的
            if (null == prefab_outer)
                return false;
            return true;
            // GameObject prefab_inner = PrefabUtility.GetNearestPrefabInstanceRoot(obj);
            // Debug.LogFormat("Out:{0},Inner:{1},{2}"
            //     , prefab_outer == null ? "null" : prefab_outer.name
            //     , prefab_inner == null ? "null" : prefab_inner.name
            //     , obj.transform.GetPath(null));

            // if (prefab_inner == prefab_outer)
            //     return false;
            // return true;
        }

        /// <summary>
        /// 传入的一定是inst obj，但是inst obj也区分是否unpack。没有unpack应该是对的，unpack需要再验证
        /// </summary>
        public static bool IsInstInnerPrefab(GameObject inst_obj)
        {
            GameObject prefab_outer = PrefabUtility.GetOutermostPrefabInstanceRoot(inst_obj);

            //如果prefab_outer 为null，说明是属于该对象的
            if (null == prefab_outer)
                return false;

            GameObject prefab_inner = PrefabUtility.GetNearestPrefabInstanceRoot(inst_obj);
            // Debug.LogFormat("Out:{0},Inner:{1},{2}"
            //     , prefab_outer == null ? "null" : prefab_outer.name
            //     , prefab_inner == null ? "null" : prefab_inner.name
            //     , obj.transform.GetPath(null));

            if (prefab_inner == prefab_outer)
                return false;
            return true;
        }

        public static GameObject GetOrigPrefabWithVariant(GameObject obj)
        {
            if (PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.Variant)
                return null;

            return PrefabUtility.GetCorrespondingObjectFromSource(obj);
        }

        /// <summary>
        /// 判断对象是否是 prefab 里面的嵌套 prefab节点，是否为嵌套prefab的根节点
        /// </summary>
        private static bool _IsInnerPrefabRoot(GameObject obj)
        {
            GameObject prefab_inner = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            if (obj == prefab_inner)
                return true;

            //不能用 inner，因为asset 里面，属于该prefab的对象，out是null，inner 有值
            //如果是 嵌套prefab里面嵌套prefab的节点，inner 的值是 2级嵌套的对象，这个不需要在最上层做表达
            return false;
        }

        public static string GetInnerPrefabAssetPath(GameObject obj)
        {
            GameObject prefab_inner = PrefabUtility.GetOutermostPrefabInstanceRoot(obj);
            return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(prefab_inner);
        }


        public static GameObject GetParentPrefab(GameObject obj)
        {
            return GetOrigPrefabWithVariant(obj);
        }

        public static void GetSubPrefab(GameObject obj, List<string> out_list)
        {
            Dictionary<GameObject, string> dict = new Dictionary<GameObject, string>();
            GetSubPrefab(obj, dict);
            out_list.Clear();

            foreach (var p in dict)
            {
                string inner_prefab_path = p.Value;
                if (out_list.Contains(inner_prefab_path))
                    continue;
                out_list.Add(inner_prefab_path);
            }
        }

        public static void GetSubPrefab(GameObject obj, Dictionary<GameObject, string> out_list)
        {
            out_list.Clear();

            Transform root = obj.transform;
            Transform[] transform_list = obj.GetComponentsInChildren<Transform>(true);
            foreach (var a in transform_list)
            {
                if (a == root)
                    continue;
                var obj_type = GetTargetType(a, root);

                switch (obj_type)
                {
                    case EEDUIObjType.prefab_self:
                    case EEDUIObjType.prefab_inner_obj:
                    case EEDUIObjType.prefab_variant:
                        //不处理
                        break;

                    case EEDUIObjType.prefab_inner_root:
                        string inner_prefab_path = GetInnerPrefabAssetPath(a.gameObject);
                        out_list.Add(a.gameObject, inner_prefab_path);
                        break;

                    default:
                        Debug.LogError("未知类型:" + obj_type);
                        break;
                }
            }
        }         
    }
}