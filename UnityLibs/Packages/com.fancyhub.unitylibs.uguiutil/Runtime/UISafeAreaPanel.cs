/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/3 12:04:25
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;

namespace FH.UI
{
    /// <summary>
    /// 要和 UISafeAreaRoot 配合使用
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UISafeAreaPanel : MonoBehaviour
    {
        private RectTransform _RectTran;

        public void Awake()
        {
            _RectTran = GetComponent<RectTransform>();
        }
        public void OnEnable()
        {
            UISafeAreaRoot root = GetComponentInParent<UISafeAreaRoot>();
            if (root == null)
            {
                Adjust(UISafeAreaRoot.UISafeAreaRectTranInfo.Default);
            }
            else
            {
                Adjust(root.ResultRectTranInfo);
            }
        }

        public void Adjust(UISafeAreaRoot.UISafeAreaRectTranInfo data)
        {
            _RectTran.anchorMin = data.AnchorMin;
            _RectTran.anchorMax = data.AnchorMax;
            _RectTran.pivot = data.Pivot;
            _RectTran.anchoredPosition = data.AnchoredPos;
            _RectTran.sizeDelta = data.SizeDelta;            
        }
    }
}

