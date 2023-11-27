
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    public partial class UIButtonView : FH.UI.UIBaseView
    {
        public  const string C_AssetPath = "Assets/Resources/UI/Prefab/Button.prefab";
        public  const string C_ResoucePath = "UI/Prefab/Button";

		public UnityEngine.UI.Button _Button;
		public UnityEngine.UI.Text _Text;

        #region AutoGen 1
        public override string GetAssetPath() { return C_AssetPath; }
        public override string GetResoucePath() { return C_ResoucePath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            UIViewCompReference refs = _FindViewReference("Button");
            if (refs == null)
                return;

			_Button = refs.GetComp<UnityEngine.UI.Button>("_Button");
			_Text = refs.GetComp<UnityEngine.UI.Text>("_Text");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
