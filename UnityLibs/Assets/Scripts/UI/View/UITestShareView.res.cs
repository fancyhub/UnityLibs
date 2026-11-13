//AutoGen, Don't modify this file

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
		public UIButtonView _BtnSimuateCapture;
		public UIButtonView _BtnDownload;
		public UIButtonView _BtnShare;
		public UIButtonView _BtnShareWechat;
		public UIButtonView _BtnShareDouyin;
		public UIButtonView _BtnShareFacebook;
		public UIButtonView _BtnShareTiktok;
		public UnityEngine.UI.Text _Info;

        #region AutoGen
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestShare");
            if (refs == null)
                return;

			_TestShare = refs.Get<UnityEngine.RectTransform>("_TestShare");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnClose"));
			_Img = refs.Get<UnityEngine.UI.RawImage>("_Img");
			_BtnSimuateCapture = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnSimuateCapture"));
			_BtnDownload = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnDownload"));
			_BtnShare = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnShare"));
			_BtnShareWechat = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnShareWechat"));
			_BtnShareDouyin = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnShareDouyin"));
			_BtnShareFacebook = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnShareFacebook"));
			_BtnShareTiktok = _CreateSub<UIButtonView>(refs.GetGameObject("_BtnShareTiktok"));
			_Info = refs.Get<UnityEngine.UI.Text>("_Info");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			if( _BtnClose != null ) { _BtnClose.Destroy(); _BtnClose=null;}
			if( _BtnSimuateCapture != null ) { _BtnSimuateCapture.Destroy(); _BtnSimuateCapture=null;}
			if( _BtnDownload != null ) { _BtnDownload.Destroy(); _BtnDownload=null;}
			if( _BtnShare != null ) { _BtnShare.Destroy(); _BtnShare=null;}
			if( _BtnShareWechat != null ) { _BtnShareWechat.Destroy(); _BtnShareWechat=null;}
			if( _BtnShareDouyin != null ) { _BtnShareDouyin.Destroy(); _BtnShareDouyin=null;}
			if( _BtnShareFacebook != null ) { _BtnShareFacebook.Destroy(); _BtnShareFacebook=null;}
			if( _BtnShareTiktok != null ) { _BtnShareTiktok.Destroy(); _BtnShareTiktok=null;}

        }


        #endregion
    }

}
