
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
		public UnityEngine.UI.Text _txt;
		public UnityEngine.UI.Button _btn;
		public UnityEngine.UI.Text _txt_btn;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("notice_message_box");
            if (refs == null)
                return;

			_notice_message_box = refs.GetComp<UnityEngine.UI.Image>("_notice_message_box");
			_txt = refs.GetComp<UnityEngine.UI.Text>("_txt");
			_btn = refs.GetComp<UnityEngine.UI.Button>("_btn");
			_txt_btn = refs.GetComp<UnityEngine.UI.Text>("_txt_btn");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
