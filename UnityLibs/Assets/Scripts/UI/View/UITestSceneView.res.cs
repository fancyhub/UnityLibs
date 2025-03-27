
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestScene.prefab", ParentPrefabPath:"", CsClassName:"UITestSceneView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestSceneView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestScene.prefab";

		public UnityEngine.RectTransform _TestScene;
		public UnityEngine.RectTransform _ItemList;
		public UIButtonView _BtnClose;
		public UIButtonView _BtnLoadSceneSingle;
		public UIButtonView _BtnLoadSceneAdditive;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestScene");
            if (refs == null)
                return;

			_TestScene = refs.GetComp<UnityEngine.RectTransform>("_TestScene");
			_ItemList = refs.GetComp<UnityEngine.RectTransform>("_ItemList");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetObj("_BtnClose"));
			_BtnLoadSceneSingle = _CreateSub<UIButtonView>(refs.GetObj("_BtnLoadSceneSingle"));
			_BtnLoadSceneAdditive = _CreateSub<UIButtonView>(refs.GetObj("_BtnLoadSceneAdditive"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnClose.Destroy();
			_BtnLoadSceneSingle.Destroy();
			_BtnLoadSceneAdditive.Destroy();

        }


        #endregion
    }

}
