
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/WebViewTab.prefab", ParentPrefabPath:"", CsClassName:"UIWebViewTabView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UIWebViewTabView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/WebViewTab.prefab";

		public UnityEngine.UI.Toggle _WebViewTab;
		public UnityEngine.UI.Text _Name;
		public UIButtonView _Close;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("WebViewTab");
            if (refs == null)
                return;

			_WebViewTab = refs.GetComp<UnityEngine.UI.Toggle>("_WebViewTab");
			_Name = refs.GetComp<UnityEngine.UI.Text>("_Name");
			_Close = _CreateSub<UIButtonView>(refs.GetObj("_Close"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_Close.Destroy();

        }


        #endregion
    }

}
