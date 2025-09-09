
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FH;
using FH.UI;

namespace Game
{ 
    public partial class UITestScrollItemView : IUISelectable, IVoSetter<string>  // : FH.UI.UIBaseView 
    {
        public Action<IUISelectable, long> _select_cb;
        public bool _selected = false;
        public override void OnCreate()
        {
            base.OnCreate();

            _TestScrollItem.onClick.AddListener(_on_btn_click);
            Selected = false;
        }

        //public override void OnDestroy()
        //{
        //    base.OnDestroy();    
        //}

        public void SetData(string data)
        {
            _Name.text = data;
        }

        public bool Selected
        {
            get { return _selected; }
            set
            {

                _selected = value;
                _img_selected.enabled = _selected;
            }
        }

        public void SetBtnClickCb(Action<IUISelectable, long> cb)
        {
            _select_cb = cb;
        }

        public void _on_btn_click()
        {
            _select_cb?.Invoke(this, 0);
        }
    }

}
