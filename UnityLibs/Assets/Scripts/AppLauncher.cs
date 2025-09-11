using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AppLauncher : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            FH.Log.AutoInit();

            new MyUISceneMgr().Init();
            FH.UI.UIMgr.ChangeScene<UISceneExtractAsset>();

            FH.TaskQueue.Init(10);

            FH.UI.UIRedDotMgr.Init(FH.ELogLvl.Info);
            FH.UI.UIMgr.UpdateList += FH.UI.UIRedDotMgr.Update;

            FH.TimerMgr.Init();
            FH.UI.UIMgr.UpdateList += FH.TimerMgr.Update;
        }
    }


    public class MyUISceneMgr : FH.UI.UIMgr
    {
        protected override FH.UI.IUIScene CreateScene<T>()
        {
            var sceneType = typeof(T);

            if (sceneType == typeof(UISceneExtractAsset))
                return new UISceneExtractAsset();
            else if (sceneType == typeof(UISceneMain))
                return new UISceneMain();

            return null;
        }
    }

    public class UISceneExtractAsset : FH.UI.UISceneBase
    {

        public override void OnSceneEnter(FH.UI.IUIScene lastScene)
        {
            this.OpenUI<UIExtractAssetPage>();
        }

        public override void OnUpdate()
        {
        }
    }

    public class UISceneMain : FH.UI.UISceneBase
    {
        public override void OnSceneEnter(FH.UI.IUIScene lastScene)
        {
            NoticeApi.Init();
            this.OpenUI<UIMainPage>(Tag: FH.UI.EUITagIndex.BG);

        }

        public override void OnUpdate()
        {
        }
    }
}