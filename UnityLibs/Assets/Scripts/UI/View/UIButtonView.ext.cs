
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    public partial class UIButtonView // : FH.UI.UIBaseView 
    {
        public Action OnClick;
        public override void OnCreate()
        {
            base.OnCreate();
            this._Button.onClick.AddListener(_OnClick);
        }

        public override void OnDestroy()
        {
            this._Button.onClick.RemoveAllListeners();
            base.OnDestroy();
            OnClick = null;
        }

        private void _OnClick()
        {
            OnClick?.Invoke();
        }
    }

}
