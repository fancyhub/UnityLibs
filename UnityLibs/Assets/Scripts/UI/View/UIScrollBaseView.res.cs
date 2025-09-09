
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/Common/ScrollBase.prefab", ParentPrefabPath:"", CsClassName:"UIScrollBaseView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UIScrollBaseView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/Common/ScrollBase.prefab";

		public UnityEngine.UI.ScrollRect _ScrollBase;
		public UnityEngine.RectTransform _mycontent;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("ScrollBase");
            if (refs == null)
                return;

			_ScrollBase = refs.GetComp<UnityEngine.UI.ScrollRect>("_ScrollBase");
			_mycontent = refs.GetComp<UnityEngine.RectTransform>("_mycontent");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
