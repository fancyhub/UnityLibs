
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestUIRes.prefab", ParentPrefabPath:"", CsClassName:"UITestUIResView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestUIResView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestUIRes.prefab";

		public UnityEngine.RectTransform _TestUIRes;
		public UIButtonView _BtnClose;
		public UnityEngine.UI.Image _Img1;
		public UnityEngine.UI.Image _Img2;
		public UIButtonView _BtnLoad;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestUIRes");
            if (refs == null)
                return;

			_TestUIRes = refs.GetComp<UnityEngine.RectTransform>("_TestUIRes");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetObj("_BtnClose"));
			_Img1 = refs.GetComp<UnityEngine.UI.Image>("_Img1");
			_Img2 = refs.GetComp<UnityEngine.UI.Image>("_Img2");
			_BtnLoad = _CreateSub<UIButtonView>(refs.GetObj("_BtnLoad"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnClose.Destroy();
			_BtnLoad.Destroy();

        }


        #endregion
    }

}
