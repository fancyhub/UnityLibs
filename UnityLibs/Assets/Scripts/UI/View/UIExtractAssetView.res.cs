
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    //PrefabPath:"Assets/Res/UI/Prefab/ExtractAsset.prefab", ParentPrefabPath:"", CsClassName:"UIExtractAssetView", ParentCsClassName:"FH.UI.UIBaseView"
    public partial class UIExtractAssetView : FH.UI.UIBaseView
    {
        public  const string CPath = "Assets/Res/UI/Prefab/ExtractAsset.prefab";

		public UnityEngine.RectTransform _ExtractAsset;
		public UnityEngine.UI.Text _LblDesc;
		public UnityEngine.UI.Slider _progress;

        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference("ExtractAsset");
            if (refs == null)
                return;

			_ExtractAsset = refs.Get<UnityEngine.RectTransform>("_ExtractAsset");
			_LblDesc = refs.Get<UnityEngine.UI.Text>("_LblDesc");
			_progress = refs.Get<UnityEngine.UI.Slider>("_progress");

        }

        protected override void _AutoDestroy()
        {
            base._AutoDestroy();


        }


        #endregion
    }

}
