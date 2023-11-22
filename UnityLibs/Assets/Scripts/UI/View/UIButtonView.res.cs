
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    public partial class UIButtonView : FH.UI.UIBaseView2
    {
        public  const string C_AssetPath = "Assets/Resources/UI/Prefab/Button.prefab";
        public  const string C_ResoucePath = "UI/Prefab/Button";

		public UnityEngine.RectTransform _Button;
		public UnityEngine.RectTransform _Text;

        #region AutoGen 1
        public override string GetAssetPath() { return C_AssetPath; }
        public override string GetResoucePath() { return C_ResoucePath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            UIViewReference refs = _FindViewReference("Button");
            if (refs == null)
                return;

			_Button = refs.GetComp<UnityEngine.RectTransform>("_Button");
			_Text = refs.GetComp<UnityEngine.RectTransform>("_Text");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
