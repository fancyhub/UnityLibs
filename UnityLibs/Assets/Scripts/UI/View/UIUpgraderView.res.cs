
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/Upgrader.prefab", ParentPrefabPath:"", CsClassName:"UIUpgraderView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UIUpgraderView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/Upgrader.prefab";

		public UnityEngine.RectTransform _Upgrader;
		public UnityEngine.UI.Text _LblDesc;
		public UnityEngine.UI.Slider _progress;
		public UIButtonView _BtnUpgrade;
		public UIButtonView _BtnNext;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("Upgrader");
            if (refs == null)
                return;

			_Upgrader = refs.GetComp<UnityEngine.RectTransform>("_Upgrader");
			_LblDesc = refs.GetComp<UnityEngine.UI.Text>("_LblDesc");
			_progress = refs.GetComp<UnityEngine.UI.Slider>("_progress");
			_BtnUpgrade = _CreateSub<UIButtonView>(refs.GetObj("_BtnUpgrade"));
			_BtnNext = _CreateSub<UIButtonView>(refs.GetObj("_BtnNext"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnUpgrade.Destroy();
			_BtnNext.Destroy();

        }


        #endregion
    }

}
