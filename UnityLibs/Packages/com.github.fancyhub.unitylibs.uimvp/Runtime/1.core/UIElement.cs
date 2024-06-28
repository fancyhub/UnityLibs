/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/1 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH
{
    public abstract class UIElement : IUIElement
    {
        private bool __destroyed;
        private int __ptr_ver = 0;
        private int __element_id;
        int ICPtr.PtrVer { get => __ptr_ver; }

        public int Id => __element_id;

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
            Log.Assert(!__destroyed, "不能destroy 两次");
            __destroyed = true;
            __ptr_ver++;
        }
    }
}
