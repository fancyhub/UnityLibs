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


    public abstract partial class UIBaseView
    {
        public static T CreateView<T>(Transform parent, IResInstHolder res_holder = null) where T : UIBaseView, new()
        {
            T ret = new T();
            string path = ret.GetAssetPath();
            if (string.IsNullOrEmpty(path))
                return null;
            if (parent == null)
                throw new Exception("");

            EUIBaseViewCreateMode create_mode = EUIBaseViewCreateMode.RootWithoutHolder;
            if (res_holder == null)
            {
                res_holder = ResMgr.CreateHolder(true);
                create_mode = EUIBaseViewCreateMode.RootWithHolder;
            }

            GameObject obj = res_holder.Create(path);
            if (null == obj)
            {
                Log.E("Create {0} Faield, 可能资源不存在 {1}", typeof(T), path);
                return null;
            }

            obj.transform.SetParent(parent, false);
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
