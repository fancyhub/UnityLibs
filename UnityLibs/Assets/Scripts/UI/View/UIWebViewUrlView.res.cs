
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/WebViewUrl.prefab", ParentPrefabPath:"", CsClassName:"UIWebViewUrlView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UIWebViewUrlView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/WebViewUrl.prefab";

		public UnityEngine.UI.InputField _WebViewUrl;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("WebViewUrl");
            if (refs == null)
                return;

			_WebViewUrl = refs.GetComp<UnityEngine.UI.InputField>("_WebViewUrl");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
