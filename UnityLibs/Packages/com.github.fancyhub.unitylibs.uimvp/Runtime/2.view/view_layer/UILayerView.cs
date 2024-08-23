/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/31 17:09:02
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    public interface IUILayerView : IUIElement
    {
        public void SetOrder(int order);
        public GameObject GetRoot();
    }

    public interface IUILayerViewBGHandler : IUIElement
    {
        public void OnBgClick();        
    }

    public enum EUIBgClickMode
    {
        None,

        /// <summary>
        /// 普通的模式
        /// </summary>
        Common,

        /// <summary>
        /// 是弹tips的模式, 点击到tip外面都会收到click消息
        /// </summary>
        TipClick,

        /// <summary>
        /// 是弹tips的模式, 点击到tip外面都会收到click消息, 和tip一样,但是down的时候触发
        /// </summary>
        TipDown,
    }     
}
