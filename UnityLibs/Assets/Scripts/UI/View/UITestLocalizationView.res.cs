
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestLocalization.prefab", ParentPrefabPath:"", CsClassName:"UITestLocalizationView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestLocalizationView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestLocalization.prefab";

		public UnityEngine.RectTransform _TestLocalization;
		public UnityEngine.UI.Dropdown _Selector;
		public UIButtonView _BtnClose;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestLocalization");
            if (refs == null)
                return;

			_TestLocalization = refs.GetComp<UnityEngine.RectTransform>("_TestLocalization");
			_Selector = refs.GetComp<UnityEngine.UI.Dropdown>("_Selector");
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
