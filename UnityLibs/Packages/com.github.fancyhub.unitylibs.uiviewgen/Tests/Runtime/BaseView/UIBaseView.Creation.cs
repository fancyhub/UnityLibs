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

namespace FH.UI.Sample
{
    public abstract partial class UIBaseView
    {
        private class UIResHolderSimple : IUIResHolder
        {
            public GameObject Create(string res_path, Transform parent)
            {
                GameObject prefab = Resources.Load<GameObject>(res_path);
                GameObject ret = GameObject.Instantiate<GameObject>(prefab, parent);
                return ret;
            }

            public void Destroy()
            {

            }

            public void Release(GameObject obj)
            {
                GameObject.Destroy(obj);
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
            obj.transform.SetParent(parent, false);
            if (!ret.Init(obj, res_holder, create_mode))
                return null;

            return ret;
        }
    }
}
