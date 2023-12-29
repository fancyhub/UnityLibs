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
    [Serializable]
    public class UISafeAreaData
    {
        public Vector2 AnchorMin = Vector2.zero;
        public Vector2 AnchorMax = Vector2.one;
        public Vector2 Pivot = Vector2.one * 0.5f;

        public Vector2 AnchoredPos= Vector2.zero;
        public Vector2 SizeDelta= Vector2.zero;
    }

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
            Adjust(root.SafeAreaData);
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

