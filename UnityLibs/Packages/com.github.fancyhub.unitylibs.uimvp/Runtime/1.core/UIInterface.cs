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
    public static class UIElementID
    {
        private static int _id_gen = 1;
        public static int Next => _id_gen++;
    }

    public interface IUIElement : IDestroyable, ICPtr
    {
        int Id { get; }
        bool IsDestroyed();
    }

    /// <summary>
    /// UI Presenter
    /// </summary>
    public interface IUIPage : IUIElement
    {
        public void UIOpen();
        public void UIClose();
    }

    /// <summary>
    /// UI View
    /// </summary>
    public interface IUIView : IUIElement
    {
        void SetOrder(int order, bool relative = false);
    }
}
