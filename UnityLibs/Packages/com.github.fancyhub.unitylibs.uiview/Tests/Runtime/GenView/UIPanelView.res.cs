
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    public partial class UIPanelView : FH.UI.UIBaseView
    {
        public  const string C_AssetPath = "Packages/com.github.fancyhub.unitylibs.uiview/Tests/Runtime/Prefabs/Panel.prefab";
        public  const string C_ResoucePath = "";

		public UnityEngine.RectTransform _Panel;
		public UnityEngine.RectTransform _bg;
		public UnityEngine.RectTransform _img_0;
		public UIButtonView _btn_0;
		public UnityEngine.RectTransform _img_1;
		public UIButtonView _btn_1;
		public UnityEngine.RectTransform _img_2;
		public UIButtonVariantView _btn_2;
		public List<UnityEngine.RectTransform> _img_list = new List<UnityEngine.RectTransform>();
		public List<UIButtonView> _btn_list = new List<UIButtonView>();

        #region AutoGen 1
        public override string GetAssetPath() { return C_AssetPath; }
        public override string GetResoucePath() { return C_ResoucePath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            UIViewReference refs = _FindViewReference("Panel");
            if (refs == null)
                return;

			_Panel = refs.GetComp<UnityEngine.RectTransform>("_Panel");
			_bg = refs.GetComp<UnityEngine.RectTransform>("_bg");
			_img_0 = refs.GetComp<UnityEngine.RectTransform>("_img_0");
			_btn_0 = CreateSub<UIButtonView>(refs.GetObj("_btn_0"), ResHolder);
			_img_1 = refs.GetComp<UnityEngine.RectTransform>("_img_1");
			_btn_1 = CreateSub<UIButtonView>(refs.GetObj("_btn_1"), ResHolder);
			_img_2 = refs.GetComp<UnityEngine.RectTransform>("_img_2");
			_btn_2 = CreateSub<UIButtonVariantView>(refs.GetObj("_btn_2"), ResHolder);
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
