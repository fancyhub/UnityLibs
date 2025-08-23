
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestTime.prefab", ParentPrefabPath:"", CsClassName:"UITestTimeView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestTimeView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestTime.prefab";

		public UnityEngine.RectTransform _TestTime;
		public UnityEngine.UI.Text _CurInfo;
		public UIButtonView _BtnClose;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestTime");
            if (refs == null)
                return;

			_TestTime = refs.GetComp<UnityEngine.RectTransform>("_TestTime");
			_CurInfo = refs.GetComp<UnityEngine.UI.Text>("_CurInfo");
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
