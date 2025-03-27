
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/SceneItem.prefab", ParentPrefabPath:"", CsClassName:"UISceneItemView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UISceneItemView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/SceneItem.prefab";

		public UnityEngine.RectTransform _SceneItem;
		public UnityEngine.UI.Text _Name;
		public UIButtonView _Unload;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("SceneItem");
            if (refs == null)
                return;

			_SceneItem = refs.GetComp<UnityEngine.RectTransform>("_SceneItem");
			_Name = refs.GetComp<UnityEngine.UI.Text>("_Name");
			_Unload = _CreateSub<UIButtonView>(refs.GetObj("_Unload"));

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();

			_Unload.Destroy();

        }


        #endregion
    }

}
