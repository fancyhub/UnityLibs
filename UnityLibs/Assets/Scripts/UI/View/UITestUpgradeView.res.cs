
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
		public UnityEngine.UI.Text _VersionInfo;
		public UnityEngine.UI.InputField _CDNInput;
		public UnityEngine.UI.InputField _VersionInput;
		public UIButtonView _BtnClose;
		public UIButtonView _BtnUpgrade;
		public UIButtonView _BtnBackToBase;
		public UnityEngine.UI.Slider _Progress;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestUpgrade");
            if (refs == null)
                return;

			_TestUpgrade = refs.Get<UnityEngine.RectTransform>("_TestUpgrade");
			_VersionInfo = refs.Get<UnityEngine.UI.Text>("_VersionInfo");
			_CDNInput = refs.Get<UnityEngine.UI.InputField>("_CDNInput");
			_VersionInput = refs.Get<UnityEngine.UI.InputField>("_VersionInput");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnClose"));
			_BtnUpgrade = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnUpgrade"));
			_BtnBackToBase = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnBackToBase"));
			_Progress = refs.Get<UnityEngine.UI.Slider>("_Progress");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			if( _BtnClose != null ) { _BtnClose.Destroy(); _BtnClose=null;}
			if( _BtnUpgrade != null ) { _BtnUpgrade.Destroy(); _BtnUpgrade=null;}
			if( _BtnBackToBase != null ) { _BtnBackToBase.Destroy(); _BtnBackToBase=null;}

        }


        #endregion
    }

}
