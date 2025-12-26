
using FH;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    public partial class UIButtonView // : FH.UI.UIBaseView 
    {
        public Action OnClick;
        public Action<UIButtonView> OnClick2;
        public override void OnCreate()
        {
            base.OnCreate();
            this._Button.onClick.AddListener(_OnClick);
        }

        public override void OnDestroy()
        {
            this._Button.onClick.RemoveListener(_OnClick);
            base.OnDestroy();
            OnClick = null;
            OnClick2 = null;
        }

        public string ButtonName
        {
            get
            {
                return _TextName.text;
            }
            set
            {
                _TextName.text = value;
            }
        }

        public bool Visible
        {
            get
            {
                return _Button.gameObject.activeSelf;
            }
            set
            {
                _Button.ExtSetGameObjectActive(value);
            }
        }

        public bool Enable
        {
            get
            {
                return _Button.enabled;
            }
            set
            {
                _Button.enabled = value;
            }
        }

        private void _OnClick()
        {
            OnClick2?.Invoke(this);
            OnClick?.Invoke();
        }
    }

}
