
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/Common/ScrollList.prefab", ParentPrefabPath:"Assets/Res/UI/Prefab/Common/ScrollBase.prefab", CsClassName:"UIScrollListView", ParentCsClassName:"UIScrollBaseView"
    public partial class UIScrollListView : UIScrollBaseView
    {
        public  new  const string CPath = "Assets/Res/UI/Prefab/Common/ScrollList.prefab";


        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("ScrollList");
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
