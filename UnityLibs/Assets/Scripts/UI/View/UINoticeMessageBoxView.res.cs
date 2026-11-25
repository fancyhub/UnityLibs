//AutoGen, Don't modify this file

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/notice/notice_message_box.prefab", ParentPrefabPath:"", CsClassName:"UINoticeMessageBoxView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UINoticeMessageBoxView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/notice/notice_message_box.prefab";

		public UnityEngine.UI.Image _notice_message_box;
		public UnityEngine.UI.Text _title;
		public UnityEngine.UI.Text _message;
		public UIButtonView _btnOK;
		public UIButtonView _btnCancel;

        #region AutoGen
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("notice_message_box");
            if (refs == null)
                return;

			_notice_message_box = refs.Get<UnityEngine.UI.Image>("_notice_message_box");
			_title = refs.Get<UnityEngine.UI.Text>("_title");
			_message = refs.Get<UnityEngine.UI.Text>("_message");
			_btnOK = _CreateSub<UIButtonView>(refs.GetGameObject("_btnOK"));
			_btnCancel = _CreateSub<UIButtonView>(refs.GetGameObject("_btnCancel"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			if( _btnOK != null ) { _btnOK.Destroy(); _btnOK=null;}
			if( _btnCancel != null ) { _btnCancel.Destroy(); _btnCancel=null;}

        }


        #endregion
    }

}
