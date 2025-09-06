
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/PermissionItem.prefab", ParentPrefabPath:"", CsClassName:"UIPermissionItemView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UIPermissionItemView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/PermissionItem.prefab";

		public UnityEngine.RectTransform _PermissionItem;
		public UnityEngine.UI.Text _Name;
		public UnityEngine.UI.Text _Status;
		public UIButtonView _BtnRequest;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("PermissionItem");
            if (refs == null)
                return;

			_PermissionItem = refs.GetComp<UnityEngine.RectTransform>("_PermissionItem");
			_Name = refs.GetComp<UnityEngine.UI.Text>("_Name");
			_Status = refs.GetComp<UnityEngine.UI.Text>("_Status");
			_BtnRequest = _CreateSub<UIButtonView>(refs.GetObj("_BtnRequest"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnRequest.Destroy();

        }


        #endregion
    }

}
