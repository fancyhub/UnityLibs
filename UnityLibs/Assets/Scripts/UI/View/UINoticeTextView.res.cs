
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/notice/notice_text.prefab", ParentPrefabPath:"", CsClassName:"UINoticeTextView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UINoticeTextView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/notice/notice_text.prefab";

		public UnityEngine.UI.Image _notice_text;
		public UnityEngine.UI.Text _txt;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("notice_text");
            if (refs == null)
                return;

			_notice_text = refs.GetComp<UnityEngine.UI.Image>("_notice_text");
			_txt = refs.GetComp<UnityEngine.UI.Text>("_txt");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
