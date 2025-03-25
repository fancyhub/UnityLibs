
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
		public UIButtonView _BtnClose;
		public UIButtonView _BtnLoadScene;
		public UIButtonView _BtnUnloadFirstScene;
		public UIButtonView _BtnUnloadLastScene;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestScene");
            if (refs == null)
                return;

			_TestScene = refs.GetComp<UnityEngine.RectTransform>("_TestScene");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetObj("_BtnClose"));
			_BtnLoadScene = _CreateSub<UIButtonView>(refs.GetObj("_BtnLoadScene"));
			_BtnUnloadFirstScene = _CreateSub<UIButtonView>(refs.GetObj("_BtnUnloadFirstScene"));
			_BtnUnloadLastScene = _CreateSub<UIButtonView>(refs.GetObj("_BtnUnloadLastScene"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnClose.Destroy();
			_BtnLoadScene.Destroy();
			_BtnUnloadFirstScene.Destroy();
			_BtnUnloadLastScene.Destroy();

        }


        #endregion
    }

}
