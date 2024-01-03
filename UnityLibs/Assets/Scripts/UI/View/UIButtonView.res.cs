
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
        public  const string CPath = "Assets/Res/UI/Prefab/Button.prefab";

		public UnityEngine.UI.Button _Button;
		public UnityEngine.UI.Text _Text;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

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
