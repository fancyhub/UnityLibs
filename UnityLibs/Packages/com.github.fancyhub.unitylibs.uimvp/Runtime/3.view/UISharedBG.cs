/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/24
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
    public sealed class UISharedBG : IUISharedBG
    {
        private const string CResPath = "UISharedBG";

        private static UISharedBG _;
        private GameObject _root;
        private Canvas _mask;
        private Canvas _click;

        public static UISharedBG Inst
        {
            get
            {
                if (_ == null)
                {
                    _ = new UISharedBG();
                    _._init();
                }
                return _;
            }
        }


        public void DisableMask()
        {
            //1. check
            if (_mask != null)
                _mask.gameObject.SetActive(false);
        }

        public void EnableMask(int order)
        {
            if (_mask != null)
            {
                _mask.gameObject.SetActive(true);
                _mask.sortingOrder = order;
            }
        }

        public void EnableClick(int order)
        {
            //1. check     
            if (_click == null)
                return;

            //2. set
            _click.gameObject.SetActive(true);
            _click.sortingOrder = order;
        }

        public void DisableClick()
        {
            //1. check     
            if (_click == null)
                return;

            //2. set
            _click.gameObject.SetActive(false);
        }

        public int GetClickOrder()
        {
            if (_click == null)
                return int.MinValue;
            return _click.sortingOrder;
        }

        public bool IsClickEnable()
        {
            if (_click == null)
                return false;
            return _click.gameObject.activeSelf;
        }

        private void _init()
        {
            if (_root != null)
                return;

            GameObject prefab = Resources.Load<GameObject>(CResPath);
            _root = GameObject.Instantiate<GameObject>(prefab, FH.UI.UIRoot.Root2D);
            _root.name = "UISharedBG";

            UISharedBGMono mono = _root.GetComponent<UISharedBGMono>();
            _mask = mono.Mask;

            _click = mono.Click;
            
            EventTrigger.Entry myclick = new EventTrigger.Entry();
            myclick.eventID = EventTriggerType.PointerClick;
            myclick.callback.AddListener(new UnityEngine.Events.UnityAction<BaseEventData>(_on_click));
            mono.ClickTrigger.triggers.Add(myclick);
        }

        private void _on_click(BaseEventData data)
        {
            UIBGEvent.GlobalBGClick?.Invoke();
        }
    }
}
