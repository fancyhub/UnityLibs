
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestDeviceInfo.prefab", ParentPrefabPath:"", CsClassName:"UITestDeviceInfoView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestDeviceInfoView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestDeviceInfo.prefab";

		public UnityEngine.RectTransform _TestDeviceInfo;
		public UnityEngine.RectTransform _DeviceInfo;
		public UnityEngine.UI.ToggleGroup _TabGroup;
		public UnityEngine.UI.Text _text;
		public UIButtonView _BtnClose;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestDeviceInfo");
            if (refs == null)
                return;

			_TestDeviceInfo = refs.GetComp<UnityEngine.RectTransform>("_TestDeviceInfo");
			_DeviceInfo = refs.GetComp<UnityEngine.RectTransform>("_DeviceInfo");
			_TabGroup = refs.GetComp<UnityEngine.UI.ToggleGroup>("_TabGroup");
			_text = refs.GetComp<UnityEngine.UI.Text>("_text");
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
