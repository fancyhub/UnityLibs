
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    //PrefabPath:"Assets/Res/UI/Prefab/Panel.prefab", ParentPrefabPath:"", CsClassName:"UIPanelView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UIPanelView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/Panel.prefab";

		public UnityEngine.RectTransform _Panel;
		public UnityEngine.UI.RawImage _bg;
		public UnityEngine.UI.Image _img_0;
		public UIButtonView _btn_0;
		public UnityEngine.UI.Image _img_1;
		public UIButtonView _btn_1;
		public UnityEngine.UI.Image _img_2;
		public UIButtonVariantView _btn_2;
		public List<UnityEngine.UI.Image> _img_list = new List<UnityEngine.UI.Image>();
		public List<UIButtonView> _btn_list = new List<UIButtonView>();

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("{prefab_name}");
            if (refs == null)
                return;

			_Panel = refs.GetComp<UnityEngine.RectTransform>("_Panel");
			_bg = refs.GetComp<UnityEngine.UI.RawImage>("_bg");
			_img_0 = refs.GetComp<UnityEngine.UI.Image>("_img_0");
			_btn_0 = _CreateSub<UIButtonView>(refs.GetObj("_btn_0"));
			_img_1 = refs.GetComp<UnityEngine.UI.Image>("_img_1");
			_btn_1 = _CreateSub<UIButtonView>(refs.GetObj("_btn_1"));
			_img_2 = refs.GetComp<UnityEngine.UI.Image>("_img_2");
			_btn_2 = _CreateSub<UIButtonVariantView>(refs.GetObj("_btn_2"));
			_img_list.Add(_img_0);
			_img_list.Add(_img_1);
			_img_list.Add(_img_2);
			_btn_list.Add(_btn_0);
			_btn_list.Add(_btn_1);
			_btn_list.Add(_btn_2);

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_btn_0.Destroy();
			_btn_1.Destroy();
			_btn_2.Destroy();
			_img_list.Clear();
			_btn_list.Clear();

        }


        #endregion
    }

}
