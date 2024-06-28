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


namespace FH
{
    public sealed class UIBG
    { 
        public const string C_RES_PATH = "ui_bg";
        public static UIBG _inst;

        public Action EvntBgClick;
        private GameObject _root;
        private Canvas _mask;
        private Canvas _click;

        public static UIBG GetInst()
        {
            if (_inst == null)
            {
                _inst = new UIBG();
                _inst._init();
            }
            return _inst;
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

        public int GetOrder()
        {
            return _click.sortingOrder;
        }

        public bool IsEnable()
        {
            return _click.gameObject.activeSelf;
        }       

        private void _init()
        {
            if (_root != null)
                return;

            GameObject prefab = Resources.Load<GameObject>(C_RES_PATH);
            _root = GameObject.Instantiate<GameObject>(prefab, UIRoot.Root2D);
            _root.name = "ui_bg";

            Transform t = _root.transform.Find("_mask");
            _mask = t.GetComponent<Canvas>();

            t = _root.transform.Find("_click");
            _click = t.GetComponent<Canvas>();
            EventTrigger trigger = _click.GetComponent<EventTrigger>();
            UnityEngine.Events.UnityAction<BaseEventData> click = new UnityEngine.Events.UnityAction<BaseEventData>(_on_click);
            EventTrigger.Entry myclick = new EventTrigger.Entry();
            myclick.eventID = EventTriggerType.PointerClick;
            myclick.callback.AddListener(click);
            trigger.triggers.Add(myclick);
        }

        private void _on_click(BaseEventData data)
        {
            EvntBgClick?.Invoke();
        }
    }
}
