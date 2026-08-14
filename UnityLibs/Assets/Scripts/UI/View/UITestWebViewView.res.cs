
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

			_TestWebView = refs.Get<UnityEngine.UI.Image>("_TestWebView");
			_Tabs = refs.Get<UnityEngine.UI.ToggleGroup>("_Tabs");
			_BtnAddTab = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnAddTab"));
			_Url = refs.Get<UnityEngine.RectTransform>("_Url");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnClose"));
			_WebViewDummy = refs.Get<UnityEngine.UI.Image>("_WebViewDummy");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			if( _BtnAddTab != null ) { _BtnAddTab.Destroy(); _BtnAddTab=null;}
			if( _BtnClose != null ) { _BtnClose.Destroy(); _BtnClose=null;}

        }


        #endregion
    }

}
