
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    //PrefabPath:"Assets/Res/UI/Prefab/Button.prefab", ParentPrefabPath:"", CsClassName:"UIButtonView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UIButtonView : FH.UI.UIBaseView
    {
        public  const string CPrefabName = "Button";
        public  const string CAssetPath = "Assets/Res/UI/Prefab/Button.prefab";
        public  const string CResoucePath = "";

		public UnityEngine.UI.Button _Button;
		public UnityEngine.UI.Text _Text;

        #region AutoGen 1
        public override string GetAssetPath() { return CAssetPath; }
        public override string GetResoucePath() { return CResoucePath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("Button");
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
