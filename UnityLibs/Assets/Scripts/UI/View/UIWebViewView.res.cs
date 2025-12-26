
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/WebView.prefab", ParentPrefabPath:"", CsClassName:"UIWebViewView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UIWebViewView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/WebView.prefab";

		public UnityEngine.UI.Image _WebView;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("WebView");
            if (refs == null)
                return;

			_WebView = refs.GetComp<UnityEngine.UI.Image>("_WebView");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
