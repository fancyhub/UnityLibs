
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestLocalization.prefab", ParentPrefabPath:"", CsClassName:"UITestLocalizationView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestLocalizationView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestLocalization.prefab";

		public UnityEngine.RectTransform _TestLocalization;
		public UnityEngine.UI.RawImage _bg;
		public UIButtonView _btn_0;
		public UIButtonView _btn_1;
		public List<UIButtonView> _btn_list = new List<UIButtonView>();

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestLocalization");
            if (refs == null)
                return;

			_TestLocalization = refs.GetComp<UnityEngine.RectTransform>("_TestLocalization");
			_bg = refs.GetComp<UnityEngine.UI.RawImage>("_bg");
			_btn_0 = _CreateSub<UIButtonView>(refs.GetObj("_btn_0"));
			_btn_1 = _CreateSub<UIButtonView>(refs.GetObj("_btn_1"));
			_btn_list.Add(_btn_0);
			_btn_list.Add(_btn_1);

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_btn_0.Destroy();
			_btn_1.Destroy();
			_btn_list.Clear();

        }


        #endregion
    }

}
