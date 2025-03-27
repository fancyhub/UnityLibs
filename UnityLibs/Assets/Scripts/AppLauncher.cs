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
            new MyUISceneMgr().Init();
            FH.UI.UISceneMgr.ChangeScene<UISceneUpgrader>();
            FH.TaskQueue.Init(10);
            FH.UI.UIRedDotMgr.Init(FH.ELogLvl.Info);
            FH.UI.UISceneMgr.AddUpdate(() => { FH.UI.UIRedDotMgr.Update(); return FH.UI.EUIUpdateResult.Continue; });

        }
    }


    public class MyUISceneMgr : FH.UI.UISceneMgr
    {
        protected override FH.UI.IUIScene CreateScene<T>()
        {
            var sceneType = typeof(T);

            if (sceneType == typeof(UISceneUpgrader))
                return new UISceneUpgrader();
            else if (sceneType == typeof(UISceneMain))
                return new UISceneMain();

            return null;
        }
    }

    public class UISceneUpgrader : FH.UI.UISceneBase
    {

        public override void OnSceneEnter(FH.UI.IUIScene lastScene)
        {
            this.OpenUI<UIUpgraderPage>();
        }

        public override void OnUpdate()
        {
        }
    }

    public class UISceneMain : FH.UI.UISceneBase
    {
        public override void OnSceneEnter(FH.UI.IUIScene lastScene)
        {
            this.OpenUI<UIMainPage>(Tag: FH.UI.EUITagIndex.BG);

        }

        public override void OnUpdate()
        {
        }
    }
}