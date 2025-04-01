
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestUpgrade.prefab", ParentPrefabPath:"", CsClassName:"UITestUpgradeView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestUpgradeView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestUpgrade.prefab";

		public UnityEngine.RectTransform _TestUpgrade;
		public UnityEngine.UI.Text _CurVersion;
		public UnityEngine.UI.InputField _Version;
		public UIButtonView _BtnClose;
		public UIButtonView _BtnUpgrade;
		public UnityEngine.UI.Slider _Progress;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestUpgrade");
            if (refs == null)
                return;

			_TestUpgrade = refs.GetComp<UnityEngine.RectTransform>("_TestUpgrade");
			_CurVersion = refs.GetComp<UnityEngine.UI.Text>("_CurVersion");
			_Version = refs.GetComp<UnityEngine.UI.InputField>("_Version");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetObj("_BtnClose"));
			_BtnUpgrade = _CreateSub<UIButtonView>(refs.GetObj("_BtnUpgrade"));
			_Progress = refs.GetComp<UnityEngine.UI.Slider>("_Progress");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnClose.Destroy();
			_BtnUpgrade.Destroy();

        }


        #endregion
    }

}
