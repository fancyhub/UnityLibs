
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestShare.prefab", ParentPrefabPath:"", CsClassName:"UITestShareView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestShareView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestShare.prefab";

		public UnityEngine.RectTransform _TestShare;
		public UIButtonView _BtnClose;
		public UnityEngine.UI.RawImage _Img;
		public UIButtonView _BtnDownload;
		public UIButtonView _BtnShare;
		public UIButtonView _BtnSimuateCapture;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestShare");
            if (refs == null)
                return;

			_TestShare = refs.GetComp<UnityEngine.RectTransform>("_TestShare");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetObj("_BtnClose"));
			_Img = refs.GetComp<UnityEngine.UI.RawImage>("_Img");
			_BtnDownload = _CreateSub<UIButtonView>(refs.GetObj("_BtnDownload"));
			_BtnShare = _CreateSub<UIButtonView>(refs.GetObj("_BtnShare"));
			_BtnSimuateCapture = _CreateSub<UIButtonView>(refs.GetObj("_BtnSimuateCapture"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnClose.Destroy();
			_BtnDownload.Destroy();
			_BtnShare.Destroy();
			_BtnSimuateCapture.Destroy();

        }


        #endregion
    }

}
