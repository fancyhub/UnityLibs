
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestPermission.prefab", ParentPrefabPath:"", CsClassName:"UITestPermissionView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestPermissionView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestPermission.prefab";

		public UnityEngine.UI.Image _TestPermission;
		public UnityEngine.UI.ScrollRect _ScrollView;
		public UIButtonView _BtnClose;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestPermission");
            if (refs == null)
                return;

			_TestPermission = refs.GetComp<UnityEngine.UI.Image>("_TestPermission");
			_ScrollView = refs.GetComp<UnityEngine.UI.ScrollRect>("_ScrollView");
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
