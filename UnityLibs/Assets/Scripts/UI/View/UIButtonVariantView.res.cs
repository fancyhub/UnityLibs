
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI
{

    //PrefabPath:"Assets/Res/UI/Prefab/Button_Variant.prefab", ParentPrefabPath:"Assets/Res/UI/Prefab/Button.prefab", CsClassName:"UIButtonVariantView", ParentCsClassName:"UIButtonView"
    public partial class UIButtonVariantView : UIButtonView
    {
        public  new  const string CPath = "Assets/Res/UI/Prefab/Button_Variant.prefab";


        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("{prefab_name}");
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
