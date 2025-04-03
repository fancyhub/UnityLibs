
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/notice/notice_marquee.prefab", ParentPrefabPath:"", CsClassName:"UINoticeMarqueeView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UINoticeMarqueeView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/notice/notice_marquee.prefab";

		public UnityEngine.UI.Image _notice_marquee;
		public UnityEngine.UI.Text _txt;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("notice_marquee");
            if (refs == null)
                return;

			_notice_marquee = refs.GetComp<UnityEngine.UI.Image>("_notice_marquee");
			_txt = refs.GetComp<UnityEngine.UI.Text>("_txt");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
