
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestUIGroupDialog.prefab", ParentPrefabPath:"", CsClassName:"UITestUIGroupDialogView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestUIGroupDialogView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestUIGroupDialog.prefab";

		public UnityEngine.RectTransform _TestUIGroupDialog;
		public UIButtonView _BtnClose;
		public UIButtonView _BtnOpenFree;
		public UIButtonView _BtnOpenFreeUnique;
		public UIButtonView _BtnOpenStack;
		public UIButtonView _BtnOpenStackUnique;
		public UIButtonView _BtnOpenQueue;
		public UIButtonView _BtnOpenQueueUnique;
		public UnityEngine.UI.Text _Title;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestUIGroupDialog");
            if (refs == null)
                return;

			_TestUIGroupDialog = refs.GetComp<UnityEngine.RectTransform>("_TestUIGroupDialog");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetObj("_BtnClose"));
			_BtnOpenFree = _CreateSub<UIButtonView>(refs.GetObj("_BtnOpenFree"));
			_BtnOpenFreeUnique = _CreateSub<UIButtonView>(refs.GetObj("_BtnOpenFreeUnique"));
			_BtnOpenStack = _CreateSub<UIButtonView>(refs.GetObj("_BtnOpenStack"));
			_BtnOpenStackUnique = _CreateSub<UIButtonView>(refs.GetObj("_BtnOpenStackUnique"));
			_BtnOpenQueue = _CreateSub<UIButtonView>(refs.GetObj("_BtnOpenQueue"));
			_BtnOpenQueueUnique = _CreateSub<UIButtonView>(refs.GetObj("_BtnOpenQueueUnique"));
			_Title = refs.GetComp<UnityEngine.UI.Text>("_Title");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnClose.Destroy();
			_BtnOpenFree.Destroy();
			_BtnOpenFreeUnique.Destroy();
			_BtnOpenStack.Destroy();
			_BtnOpenStackUnique.Destroy();
			_BtnOpenQueue.Destroy();
			_BtnOpenQueueUnique.Destroy();

        }


        #endregion
    }

}
