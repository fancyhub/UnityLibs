
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    public partial class UIButton2View : FH.UI.UIBaseView
    {
        public  const string C_AssetPath = "Packages/com.github.fancyhub.unitylibs.uiview/Tests/Runtime/Prefabs/Button2.prefab";
        public  const string C_ResoucePath = "";

		public UnityEngine.RectTransform _Button2;
		public UnityEngine.RectTransform _Text;

        #region AutoGen 1
        public override string GetAssetPath() { return C_AssetPath; }
        public override string GetResoucePath() { return C_ResoucePath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            UIViewReference refs = _FindViewReference("Button2");
            if (refs == null)
                return;

			_Button2 = refs.GetComp<UnityEngine.RectTransform>("_Button2");
			_Text = refs.GetComp<UnityEngine.RectTransform>("_Text");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}