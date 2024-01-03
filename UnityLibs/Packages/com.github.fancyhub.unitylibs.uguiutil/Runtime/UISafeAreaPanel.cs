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
    /// 不能和 UIBgFullScreen 在一起
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class UISafeAreaPanel : MonoBehaviour
    {        
        public void OnEnable()
        {
            UISafeAreaRoot root = GetComponentInParent<UISafeAreaRoot>();
            if (root == null)
                return;
            Adjust(root._SafeAreaData);
        }

        public void Adjust(UISafeAreaData data)
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null)
                return;

            rect.anchorMin = data.AnchorMin;
            rect.anchorMax = data.AnchorMax;
            rect.pivot = data.Pivot;
            rect.anchoredPosition = data.AnchoredPos;
            rect.sizeDelta = data.SizeDelta;
        }
    }
}

