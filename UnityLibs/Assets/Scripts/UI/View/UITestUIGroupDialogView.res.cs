
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

			_TestUIGroupDialog = refs.Get<UnityEngine.RectTransform>("_TestUIGroupDialog");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnClose"));
			_BtnOpenFree = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnOpenFree"));
			_BtnOpenFreeUnique = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnOpenFreeUnique"));
			_BtnOpenStack = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnOpenStack"));
			_BtnOpenStackUnique = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnOpenStackUnique"));
			_BtnOpenQueue = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnOpenQueue"));
			_BtnOpenQueueUnique = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnOpenQueueUnique"));
			_Title = refs.Get<UnityEngine.UI.Text>("_Title");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			if( _BtnClose != null ) { _BtnClose.Destroy(); _BtnClose=null;}
			if( _BtnOpenFree != null ) { _BtnOpenFree.Destroy(); _BtnOpenFree=null;}
			if( _BtnOpenFreeUnique != null ) { _BtnOpenFreeUnique.Destroy(); _BtnOpenFreeUnique=null;}
			if( _BtnOpenStack != null ) { _BtnOpenStack.Destroy(); _BtnOpenStack=null;}
			if( _BtnOpenStackUnique != null ) { _BtnOpenStackUnique.Destroy(); _BtnOpenStackUnique=null;}
			if( _BtnOpenQueue != null ) { _BtnOpenQueue.Destroy(); _BtnOpenQueue=null;}
			if( _BtnOpenQueueUnique != null ) { _BtnOpenQueueUnique.Destroy(); _BtnOpenQueueUnique=null;}

        }


        #endregion
    }

}
