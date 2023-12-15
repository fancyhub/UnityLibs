
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
        public  const string C_AssetPath = "Assets/Res/UI/Prefab/Button2.prefab";
        public  const string C_ResoucePath = "UI/Prefab/Button2";

		public UnityEngine.UI.Button _Button2;
		public UnityEngine.UI.Text _Text;

        #region AutoGen 1
        public override string GetAssetPath() { return C_AssetPath; }
        public override string GetResoucePath() { return C_ResoucePath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            UIViewCompReference refs = _FindViewReference("Button2");
            if (refs == null)
                return;

			_Button2 = refs.GetComp<UnityEngine.UI.Button>("_Button2");
			_Text = refs.GetComp<UnityEngine.UI.Text>("_Text");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
