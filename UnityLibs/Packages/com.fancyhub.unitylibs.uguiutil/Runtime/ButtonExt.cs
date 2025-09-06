/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/9/5
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FH.UI
{
    public static class ButtonExt
    {
        public static void ExtSetClickCallBack(this UnityEngine.UI.Button self, Action call_back)
        {
            if (self == null)
                return;
            UIClickWrapper.SetClickCallBack(self, call_back, false);
        }

        internal sealed class UIClickWrapper : MonoBehaviour, IPointerClickHandler
        {
            private const float DefaultClickCDTime = 0.5f;//
            private Selectable _SelectableTarget;
            private Action _Action;
            public float ClickCDTime = DefaultClickCDTime;//500毫秒
            private double _NextClickTime = 0;

            public static void SetClickCallBack(Selectable target, Action call_back, bool use_cd)
            {
                SetClickCallBack(target, call_back, use_cd ? DefaultClickCDTime : 0);
            }

            public static void SetClickCallBack(Selectable target, Action call_back, float cd_time)
            {
                if (target == null)
                    return;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    return;
#endif
                var comp = target.GetComponent<UIClickWrapper>();
                if (comp == null)
                {
                    if (call_back == null)
                        return;

                    comp = target.gameObject.AddComponent<UIClickWrapper>();
                    comp._SelectableTarget = target;
                }
                comp.ClickCDTime = cd_time;
                comp._Action = call_back;
            }

            void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
            {
                if (eventData.button != PointerEventData.InputButton.Left)
                    return;
                if (_NextClickTime > Time.timeAsDouble) //CD 检查
                    return;
                if (_Action == null)
                    return;

                if (_SelectableTarget != null && _SelectableTarget.IsActive() && _SelectableTarget.IsInteractable())
                {
                    _NextClickTime = Time.timeAsDouble + ClickCDTime; //赋值CD
                    _Action();
                }
            }
        }
    }
}
