
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
		public UIButtonView _BtnClose;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestWebView");
            if (refs == null)
                return;

			_TestWebView = refs.GetComp<UnityEngine.UI.Image>("_TestWebView");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetObj("_BtnClose"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnClose.Destroy();

        }


        #endregion
    }

}
