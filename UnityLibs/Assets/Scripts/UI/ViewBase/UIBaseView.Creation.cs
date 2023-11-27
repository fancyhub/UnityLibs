/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/8/8
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using FH;
using System.IO;

namespace FH.UI
{
    public class UIViewCache : MonoBehaviour
    {
        private static List<UIViewCache> _Temp = new List<UIViewCache>();
        public string ViewTypeName;
        public Type ViewType;
        public UIBaseView View;

        public static T Get<T>(GameObject obj) where T : UIBaseView
        {
            _Temp.Clear();
            obj.GetComponents<UIViewCache>(_Temp);
            T ret = null;
            foreach (var p in _Temp)
            {
                if (p.ViewType == typeof(T))
                {
                    ret = p.View as T;
                    Debug.Log(p.ViewTypeName);
                    break;
                }
            }
            _Temp.Clear();
            return ret;
        }

        public static void Add<T>(GameObject obj, T view) where T : UIBaseView
        {
            UIViewCache view_ref = obj.AddComponent<UIViewCache>();
            view_ref.ViewType = view.GetType();
            view_ref.View = view;
            view_ref.ViewTypeName = view_ref.ViewType.FullName;

        }
    }

    public class UIGameObjPath : MonoBehaviour
    {
        public string Path;
    }

    public abstract partial class UIBaseView
    {
        private class UIResHolderSimple : IUIResHolder
        {
            private static Transform _pool_obj;
            public static Dictionary<string, LinkedList<GameObject>> _pool = new Dictionary<string, LinkedList<GameObject>>();

            public GameObject Create(string res_path, Transform parent)
            {
                _pool.TryGetValue(res_path, out var list);

                for (; ; )
                {
                    if (!list.ExtPopFirst(out var obj))
                    {
                        break;
                    }

                    if (obj != null)
                        return obj;
                }

                GameObject prefab = Resources.Load<GameObject>(res_path);
                GameObject ret = GameObject.Instantiate<GameObject>(prefab, parent);

                var path_obj = ret.AddComponent<UIGameObjPath>();
                path_obj.Path = res_path;
                return ret;
            }

            public void Destroy()
            {

            }

            public static Transform PoolObj
            {
                get
                {
                    if (_pool_obj == null)
                    {
                        GameObject obj = new GameObject("PoolObj");
                        obj.SetActive(false);
                        GameObject.DontDestroyOnLoad(obj);
                        _pool_obj = obj.transform;
                    }
                    return _pool_obj;
                }
            }

            public void Release(GameObject obj)
            {
                if (obj == null)
                    return;
                var path_obj = obj.GetComponent<UIGameObjPath>();
                if (path_obj == null)
                {
                    GameObject.Destroy(obj);
                    return;
                }

                string path = path_obj.Path;

                _pool.TryGetValue(path, out var list);
                if (list == null)
                {
                    list = new LinkedList<GameObject>();
                    _pool[path] = list;
                }

                list.ExtAddLast(obj);
                obj.transform.SetParent(PoolObj, false);

            }
        }

        public static T CreateView<T>(Transform parent, IUIResHolder res_holder = null) where T : UIBaseView, new()
        {
            T ret = new T();
            string path = ret.GetResoucePath();
            if (string.IsNullOrEmpty(path))
                return null;
            if (parent == null)
                throw new Exception("");

            EUIBaseViewCreateMode create_mode = EUIBaseViewCreateMode.RootWithoutHolder;
            if (res_holder == null)
            {
                res_holder = new UIResHolderSimple();
                create_mode = EUIBaseViewCreateMode.RootWithHolder;
            }

            GameObject obj = res_holder.Create(path, parent);
            if (null == obj)
            {
                Log.E("Create {0} Faield, 可能资源不存在 {1}", typeof(T), path);
                return null;
            }

            T temp = UIViewCache.Get<T>(obj);
            if (temp != null)
                ret = temp;            

            obj.transform.SetParent(parent, false);
            if (!ret._Init(obj, res_holder, create_mode))
                return null;

            if (temp == null)
                UIViewCache.Add(obj, ret);

            return ret;
        }
    }
    //*/
}
