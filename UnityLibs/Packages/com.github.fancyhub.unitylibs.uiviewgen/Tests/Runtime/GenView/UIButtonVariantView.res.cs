
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace FH.UI.Sample
{

    //PrefabPath:"Packages/com.github.fancyhub.unitylibs.uiviewgen/Tests/Runtime/Prefabs/Button_Variant.prefab", ParentPrefabPath:"Packages/com.github.fancyhub.unitylibs.uiviewgen/Tests/Runtime/Prefabs/Button.prefab", CsClassName:"UIButtonVariantView", ParentCsClassName:"UIButtonView"
    public partial class UIButtonVariantView : UIButtonView
    {
        public  new  const string CPath = "Packages/com.github.fancyhub.unitylibs.uiviewgen/Tests/Runtime/Prefabs/Button_Variant.prefab";


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
