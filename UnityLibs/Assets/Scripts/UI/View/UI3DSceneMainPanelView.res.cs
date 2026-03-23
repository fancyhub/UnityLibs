
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/3DSceneMainPanel.prefab", ParentPrefabPath:"", CsClassName:"UI3DSceneMainPanelView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UI3DSceneMainPanelView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/3DSceneMainPanel.prefab";

		public UnityEngine.RectTransform _3DSceneMainPanel;
		public UIButtonView _BtnClose;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("3DSceneMainPanel");
            if (refs == null)
                return;

			_3DSceneMainPanel = refs.GetComp<UnityEngine.RectTransform>("_3DSceneMainPanel");
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
