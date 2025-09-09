
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestScroller.prefab", ParentPrefabPath:"", CsClassName:"UITestScrollerView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestScrollerView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestScroller.prefab";

		public UnityEngine.UI.Image _TestScroller;
		public UIButtonView _BtnClose;
		public UIScrollListView _scroll_row_2;
		public UIScrollListView _scroll_center;
		public UIScrollListView _scroll_loop;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestScroller");
            if (refs == null)
                return;

			_TestScroller = refs.GetComp<UnityEngine.UI.Image>("_TestScroller");
			_BtnClose = _CreateSub<UIButtonView>(refs.GetObj("_BtnClose"));
			_scroll_row_2 = _CreateSub<UIScrollListView>(refs.GetObj("_scroll_row_2"));
			_scroll_center = _CreateSub<UIScrollListView>(refs.GetObj("_scroll_center"));
			_scroll_loop = _CreateSub<UIScrollListView>(refs.GetObj("_scroll_loop"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_BtnClose.Destroy();
			_scroll_row_2.Destroy();
			_scroll_center.Destroy();
			_scroll_loop.Destroy();

        }


        #endregion
    }

}
