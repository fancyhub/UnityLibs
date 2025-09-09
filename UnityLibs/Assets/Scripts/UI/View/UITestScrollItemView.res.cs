
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/TestScrollItem.prefab", ParentPrefabPath:"", CsClassName:"UITestScrollItemView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UITestScrollItemView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/TestScrollItem.prefab";

		public UnityEngine.UI.Button _TestScrollItem;
		public UnityEngine.UI.Image _img_selected;
		public UnityEngine.UI.Text _Name;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("TestScrollItem");
            if (refs == null)
                return;

			_TestScrollItem = refs.GetComp<UnityEngine.UI.Button>("_TestScrollItem");
			_img_selected = refs.GetComp<UnityEngine.UI.Image>("_img_selected");
			_Name = refs.GetComp<UnityEngine.UI.Text>("_Name");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
