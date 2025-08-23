
/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/25 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace FH
{
    public static class GoUtil
    {
        private class CompListCach<T> where T : Component
        {
            public static List<T> _list = new List<T>();
        }
        private static StringBuilder _string_builder = new StringBuilder();
        private static Queue<Transform> _temp_tran_queue = new Queue<Transform>(100);

        public static void ExtSetGameObjectActive(this GameObject obj, bool active)
        {
            if (obj == null)
                return;
            if (obj.activeSelf != active)
                obj.SetActive(active);
        }

        public static void ExtSetGameObjectActive(this Component comp, bool active)
        {
            if (comp == null)
                return;
            var obj = comp.gameObject;
            if (obj.activeSelf != active)
                obj.SetActive(active);
        }

        public static GameObject ExtResetTran(this GameObject obj)
        {
            if (obj == null)
                return null;
            Transform t = obj.transform;
            t.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            t.localScale = Vector3.one;
            return obj;
        }

        public static void Destroy(GameObject obj)
        {
            if (obj == null)
                return;
            if (obj.GetInstanceID() > 0) //Res
                return;

            if (Application.isPlaying)
                GameObject.Destroy(obj);
            else
                GameObject.DestroyImmediate(obj);
        }

        public static void Destroy(ref GameObject obj)
        {
            GameObject t = obj;
            obj = null;
            Destroy(t);
        }

        public static T ExtResetTran<T>(this T obj) where T : Component
        {
            if (obj == null)
                return null;
            Transform t = obj.transform;
            t.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            t.localScale = Vector3.one;
            return obj;
        }

        public static bool ExtIsEnable(this Behaviour self)
        {
            if (self == null)
                return false;
            return self.isActiveAndEnabled;
        }

        public static bool ExtIsEnable(this Collider self)
        {
            if (self == null)
                return false;
            if (!self.enabled)
                return false;
            return self.gameObject.activeInHierarchy;
        }

        public static bool ExtIsEnable(this Renderer self)
        {
            if (self == null)
                return false;
            if (!self.enabled)
                return false;
            return self.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 注意 返回的list, 是共享的,不要持有,用完最好做一次 list的clear,防止GC删除不掉
        /// </summary>         
        public static List<T> ExtGetComps<T>(this GameObject self) where T : Component
        {
            var list = CompListCach<T>._list;
            list.Clear();
            if (self == null)
                return list;
            self.GetComponents<T>(list);
            return list;
        }

        /// <summary>
        /// 注意 返回的list, 是共享的,不要持有,用完最好做一次 list的clear,防止GC删除不掉
        /// </summary>        
        public static List<T> ExtGetComps<T>(this Component self) where T : Component
        {
            var list = CompListCach<T>._list;
            list.Clear();
            if (self == null)
                return list;
            self.GetComponents<T>(list);
            return list;
        }

        /// <summary>
        /// 注意 返回的list, 是共享的,不要持有,用完最好做一次 list的clear,防止GC删除不掉
        /// </summary>        
        public static List<T> ExtGetCompsInChildren<T>(this GameObject self, bool includeInactive) where T : Component
        {
            var list = CompListCach<T>._list;
            list.Clear();
            if (self == null)
                return list;
            self.GetComponentsInChildren<T>(includeInactive, list);
            return list;
        }

        /// <summary>
        /// 注意 返回的list, 是共享的,不要持有,用完最好做一次 list的clear,防止GC删除不掉
        /// </summary>        
        public static List<T> ExtGetCompsInChildren<T>(this Component self, bool includeInactive) where T : Component
        {
            var list = CompListCach<T>._list;
            list.Clear();
            if (self == null)
                return list;
            self.GetComponentsInChildren<T>(includeInactive, list);
            return list;
        }

        public static void ExtSetLayer<T>(this GameObject self, int layer) where T : Component
        {
            List<T> comps = ExtGetCompsInChildren<T>(self, true);
            foreach (var p in comps)
            {
                p.gameObject.layer = layer;
            }
            comps.Clear();
        }

        /// <summary>
        /// 按照层优先的获取找到的第一个组件
        /// child_depth_limit &lt;= 0: 只在自身组件上找
        /// </summary>        
        public static T ExtGetCompDepthLimit<T>(this GameObject self, int child_depth_limit) where T : Component
        {
            if (self == null)
                return null;
            return _GetCompWithDepthLimit<T>(self.transform, child_depth_limit);
        }

        /// <summary>
        /// 按照层优先的获取找到的第一个组件
        /// child_depth_limit &lt;= 0: 只在自身组件上找
        /// </summary>        
        public static T ExtGetCompDepthLimit<T>(this Component self, int child_depth_limit) where T : Component
        {
            if (self == null)
                return null;
            return _GetCompWithDepthLimit<T>(self.transform, child_depth_limit);
        }

        private static T _GetCompWithDepthLimit<T>(Transform tran, int depth) where T : Component
        {
            T ret = tran.GetComponent<T>();
            if (ret != null)
                return ret;

            _temp_tran_queue.Clear();
            _temp_tran_queue.Enqueue(tran);

            //按层,一批一批的处理
            for (int i = 0; i < depth; i++)
            {
                //当前层的transform 数量
                int cur_layer_tran_count = _temp_tran_queue.Count;
                for (int j = 0; j < cur_layer_tran_count; j++)
                {
                    Transform t = _temp_tran_queue.Dequeue();
                    for (int k = 0, child_count = t.childCount; k < child_count; k++)
                    {
                        Transform child = t.GetChild(k);
                        ret = child.GetComponent<T>();
                        if (ret != null)
                        {
                            _temp_tran_queue.Clear();
                            return ret;
                        }
                        _temp_tran_queue.Enqueue(child);
                    }
                }
            }

            _temp_tran_queue.Clear();
            return null;
        }

        public static T ExtGetComp<T>(this GameObject self, bool auto_create = true) where T : Component
        {
            if (self == null)
                return null;
            T ret = self.GetComponent<T>();
            if (ret != null)
                return ret;
            if (auto_create)
                ret = self.AddComponent<T>();
            return ret;
        }

        public static T ExtGetComp<T>(this Component self, bool auto_create = true) where T : Component
        {
            if (self == null)
                return null;
            T ret = self.GetComponent<T>();
            if (ret != null)
                return ret;
            if (auto_create)
                ret = self.gameObject.AddComponent<T>();
            return ret;
        }

        #region Hierachy Path
        public static string ExtGetHierarchyPath(this GameObject self)
        {
            if (self == null)
                return string.Empty;
            Transform root = null;
            return ExtGetHierarchyPath(self.transform, root);
        }

        public static string ExtGetHierarchyPath(this GameObject self, GameObject root)
        {
            if (self == null)
                return string.Empty;
            return ExtGetHierarchyPath(self.transform, root);
        }

        public static string ExtGetHierarchyPath(this GameObject self, Transform root)
        {
            if (self == null)
                return string.Empty;

            return ExtGetHierarchyPath(self.transform, root);
        }
        public static string ExtGetHierarchyPath(this Transform self)
        {
            Transform root = null;
            return ExtGetHierarchyPath(self, root);
        }

        public static string ExtGetHierarchyPath(this Transform self, GameObject root)
        {
            Transform root_t = null;
            if (root != null)
                root_t = root.transform;
            return ExtGetHierarchyPath(self, root_t);
        }

        /// <summary>
        /// 不包括root
        /// </summary>
        public static string ExtGetHierarchyPath(this Transform self, Transform root)
        {
            if (null == self)
                return string.Empty;

            _string_builder.Length = 0;
            _GetHierarchyPath(self, self, root, _string_builder);
            return _string_builder.ToString();
        }

        private static void _GetHierarchyPath(Transform target, Transform obj, Transform root, StringBuilder sb)
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
                Log.Assert(false, "Root {0} 不是 obj {1} 的 根节点", root, target);
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
