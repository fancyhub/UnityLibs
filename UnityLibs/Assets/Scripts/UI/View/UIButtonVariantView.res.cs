
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    //PrefabPath:"Assets/Res/UI/Prefab/Button_Variant.prefab", ParentPrefabPath:"", CsClassName:"UIButtonVariantView", ParentCsClassName:"UIButtonView"
    public partial class UIButtonVariantView : UIButtonView
    {
        public  new  const string CPrefabName = "Button_Variant";
        public  new  const string CAssetPath = "Assets/Res/UI/Prefab/Button_Variant.prefab";
        public  new  const string CResoucePath = "";


        #region AutoGen 1
        public override string GetAssetPath() { return CAssetPath; }
        public override string GetResoucePath() { return CResoucePath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("Button_Variant");
            if (refs == null)
                return;


        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
