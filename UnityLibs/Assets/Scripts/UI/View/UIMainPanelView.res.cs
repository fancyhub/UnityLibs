
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/MainPanel.prefab", ParentPrefabPath:"", CsClassName:"UIMainPanelView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UIMainPanelView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/MainPanel.prefab";

		public UnityEngine.RectTransform _MainPanel;
		public UIButtonView _BtnTestUIGroupDialog;
		public UIButtonView _BtnTestLoadScene;
		public UIButtonView _BtnTestPageAsync;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("MainPanel");
            if (refs == null)
                return;

			_MainPanel = refs.GetComp<UnityEngine.RectTransform>("_MainPanel");
			_BtnTestUIGroupDialog = _CreateSub<UIButtonView>(refs.GetObj("_BtnTestUIGroupDialog"));
			_BtnTestLoadScene = _CreateSub<UIButtonView>(refs.GetObj("_BtnTestLoadScene"));
			_BtnTestPageAsync = _CreateSub<UIButtonView>(refs.GetObj("_BtnTestPageAsync"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnTestUIGroupDialog.Destroy();
			_BtnTestLoadScene.Destroy();
			_BtnTestPageAsync.Destroy();

        }


        #endregion
    }

}
