
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
		public UnityEngine.UI.Text _Info;

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
			_Info = refs.GetComp<UnityEngine.UI.Text>("_Info");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			if( _BtnClose != null ) { _BtnClose.Destroy(); _BtnClose=null;}
			if( _BtnDownload != null ) { _BtnDownload.Destroy(); _BtnDownload=null;}
			if( _BtnShare != null ) { _BtnShare.Destroy(); _BtnShare=null;}
			if( _BtnSimuateCapture != null ) { _BtnSimuateCapture.Destroy(); _BtnSimuateCapture=null;}

        }


        #endregion
    }

}
