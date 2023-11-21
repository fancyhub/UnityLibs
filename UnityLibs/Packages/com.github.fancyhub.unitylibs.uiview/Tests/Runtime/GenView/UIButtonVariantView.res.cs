
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    public partial class UIButtonVariantView : UIButtonView
    {
        public  new  const string C_AssetPath = "Packages/com.github.fancyhub.unitylibs.uiview/Tests/Runtime/Prefabs/Button_Variant.prefab";
        public  new  const string C_ResoucePath = "";


        #region AutoGen 1
        public override string GetAssetPath() { return C_AssetPath; }
        public override string GetResoucePath() { return C_ResoucePath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            UIViewReference refs = _FindViewReference("Button_Variant");
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
