
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestWebView.prefab", ParentPrefabPath:"", CsClassName:"UITestWebViewView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestWebViewView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestWebView.prefab";

		public UnityEngine.UI.Image _TestWebView;
		public UnityEngine.UI.ToggleGroup _Tabs;
		public UIButtonView _BtnAddTab;
		public UnityEngine.RectTransform _Url;
		public UIButtonView _BtnClose;
		public UnityEngine.UI.Image _WebViewDummy;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestWebView");
            if (refs == null)
                return;

			_TestWebView = refs.GetComp<UnityEngine.UI.Image>("_TestWebView");
			_Tabs = refs.GetComp<UnityEngine.UI.ToggleGroup>("_Tabs");
			_BtnAddTab = _CreateSub<UIButtonView>(refs.GetObj("_BtnAddTab"));
			_Url = refs.GetComp<UnityEngine.RectTransform>("_Url");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetObj("_BtnClose"));
			_WebViewDummy = refs.GetComp<UnityEngine.UI.Image>("_WebViewDummy");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnAddTab.Destroy();
			_BtnClose.Destroy();

        }


        #endregion
    }

}
