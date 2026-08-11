
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
		public UIButtonView _BtnTestPageAsync;
		public UIButtonView _BtnTestLoadScene;
		public UIButtonView _BtnTestDeviceInfo;
		public UIButtonView _BtnLocalization;
		public UIButtonView _BtnUpgrade;
		public UIButtonView _BtnReloadUIScene;
		public UIButtonView _BtnTime;
		public UIButtonView _BtnPermission;
		public UIButtonView _BtnScroller;
		public UIButtonView _BtnWebView;
		public UIButtonView _BtnShare;
		public UIButtonView _Btn3DScene;

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
			_BtnTestPageAsync = _CreateSub<UIButtonView>(refs.GetObj("_BtnTestPageAsync"));
			_BtnTestLoadScene = _CreateSub<UIButtonView>(refs.GetObj("_BtnTestLoadScene"));
			_BtnTestDeviceInfo = _CreateSub<UIButtonView>(refs.GetObj("_BtnTestDeviceInfo"));
			_BtnLocalization = _CreateSub<UIButtonView>(refs.GetObj("_BtnLocalization"));
			_BtnUpgrade = _CreateSub<UIButtonView>(refs.GetObj("_BtnUpgrade"));
			_BtnReloadUIScene = _CreateSub<UIButtonView>(refs.GetObj("_BtnReloadUIScene"));
			_BtnTime = _CreateSub<UIButtonView>(refs.GetObj("_BtnTime"));
			_BtnPermission = _CreateSub<UIButtonView>(refs.GetObj("_BtnPermission"));
			_BtnScroller = _CreateSub<UIButtonView>(refs.GetObj("_BtnScroller"));
			_BtnWebView = _CreateSub<UIButtonView>(refs.GetObj("_BtnWebView"));
			_BtnShare = _CreateSub<UIButtonView>(refs.GetObj("_BtnShare"));
			_Btn3DScene = _CreateSub<UIButtonView>(refs.GetObj("_Btn3DScene"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			if( _BtnTestUIGroupDialog != null ) { _BtnTestUIGroupDialog.Destroy(); _BtnTestUIGroupDialog=null;}
			if( _BtnTestPageAsync != null ) { _BtnTestPageAsync.Destroy(); _BtnTestPageAsync=null;}
			if( _BtnTestLoadScene != null ) { _BtnTestLoadScene.Destroy(); _BtnTestLoadScene=null;}
			if( _BtnTestDeviceInfo != null ) { _BtnTestDeviceInfo.Destroy(); _BtnTestDeviceInfo=null;}
			if( _BtnLocalization != null ) { _BtnLocalization.Destroy(); _BtnLocalization=null;}
			if( _BtnUpgrade != null ) { _BtnUpgrade.Destroy(); _BtnUpgrade=null;}
			if( _BtnReloadUIScene != null ) { _BtnReloadUIScene.Destroy(); _BtnReloadUIScene=null;}
			if( _BtnTime != null ) { _BtnTime.Destroy(); _BtnTime=null;}
			if( _BtnPermission != null ) { _BtnPermission.Destroy(); _BtnPermission=null;}
			if( _BtnScroller != null ) { _BtnScroller.Destroy(); _BtnScroller=null;}
			if( _BtnWebView != null ) { _BtnWebView.Destroy(); _BtnWebView=null;}
			if( _BtnShare != null ) { _BtnShare.Destroy(); _BtnShare=null;}
			if( _Btn3DScene != null ) { _Btn3DScene.Destroy(); _Btn3DScene=null;}

        }


        #endregion
    }

}
