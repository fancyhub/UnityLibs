
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Game
{

    public partial class UISceneItemView : FH.UI.IListViewSyncerViewer<FH.SceneRef>// : FH.UI.UIBaseView 
    {
        //public override void OnCreate()
        //{
        //    base.OnCreate();
        //}

        //public override void OnDestroy()
        //{
        //    base.OnDestroy();    
        //}

        public FH.SceneRef _Scene;
        public void SetData(FH.SceneRef scene)
        {
            _Scene = scene;
            _Name.text = $"{scene.SceneId}, Name:{scene.UnityScene.name}_Valid:{scene.IsValid}_Done:{scene.IsDone} ";
        }
    }

}
