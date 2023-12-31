
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI.Sample
{

    //PrefabPath:"Packages/com.github.fancyhub.unitylibs.uiviewgen/Tests/Runtime/Prefabs/Panel_Variant.prefab", ParentPrefabPath:"Packages/com.github.fancyhub.unitylibs.uiviewgen/Tests/Runtime/Prefabs/Panel.prefab", CsClassName:"UIPanelVariantView", ParentCsClassName:"UIPanelView"
    public partial class UIPanelVariantView : UIPanelView
    {
        public  new  const string CPath = "Packages/com.github.fancyhub.unitylibs.uiviewgen/Tests/Runtime/Prefabs/Panel_Variant.prefab";

		public UnityEngine.UI.Image _img_3;
		public UIButton2View _btn_3;
		public UnityEngine.UI.Image _img_4;
		public UIButtonVariantView _btn_4;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("Panel_Variant");
            if (refs == null)
                return;

			_img_3 = refs.GetComp<UnityEngine.UI.Image>("_img_3");
			_btn_3 = _CreateSub<UIButton2View>(refs.GetObj("_btn_3"));
			_img_4 = refs.GetComp<UnityEngine.UI.Image>("_img_4");
			_btn_4 = _CreateSub<UIButtonVariantView>(refs.GetObj("_btn_4"));
			_img_list.Add(_img_3);
			_img_list.Add(_img_4);
			_btn_list.Add(_btn_4);

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_btn_3.Destroy();
			_btn_4.Destroy();
			_img_list.Clear();
			_btn_list.Clear();

        }


        #endregion
    }

}
