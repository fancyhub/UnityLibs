/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/1 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    public abstract class UIElement : IUIElement
    {
        private bool __destroyed;
        private int ___obj_ver = 0;
        private int __element_id;
        int IVersionObj.ObjVersion { get => ___obj_ver; }

        public int UIElementId => __element_id;

        public UIElement()
        {
            __element_id = UIElementID.Next;
            __destroyed = false;
        }

        public bool IsDestroyed()
        {
            return __destroyed;
        }

        public virtual void Destroy()
        {
            Log.Assert(!__destroyed, "can't destroy twice, {0}, {1}", __element_id, GetType());
            __destroyed = true;
            ___obj_ver++;
        }
    }
}
