using FH;
using FH.Omi;
using FH.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Game
{
    public class AppLauncher : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            FH.Log.AutoInit();

            //if (!Application.isEditor)
            FH.DebugConnectionServer.StartServer();

            new MyUISceneMgr().Init();
            FH.UI.UIMgr.ChangeScene<UISceneExtractAsset>();

            FH.TaskQueue.Init();

            FH.UI.UIRedDotMgr.Init(FH.ELogLvl.Info);
            FH.UI.UIMgr.UpdateList += FH.UI.UIRedDotMgr.Update;

            FH.TimerMgr.Init();
            FH.UI.UIMgr.UpdateList += FH.TimerMgr.Update;
        }
    }


    public class MyUISceneMgr : FH.UI.UIMgr
    {
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


    public class UIScene3D : FH.UI.UISceneBase
    {
        private const string CScenePath = "Assets/Scenes/3DDemoScene.unity";
        private FH.SceneRef _Scene;
        private CPtr<IResHolder> _ResHolder;
        public override void OnSceneEnter(IUIScene lastScene)
        {
            _Scene = FH.SceneMgr.LoadScene(CScenePath, UnityEngine.SceneManagement.LoadSceneMode.Single);
            this.OpenUI<UI3DSceneMainPage>(Tag: EUITagIndex.BG);
            _ResHolder = ResMgr.CreateHolder(false, false).CPtr();
        }

        private bool _Inited = false;
        public override void OnUpdate()
        {
            if (_Scene.IsDone && !_Inited)
            {
                _Inited = true;
                var obj = _ResHolder.Val?.Create("Assets/Res/TestAB/TestCharAB.prefab");
                if(obj!=null)
                {
                    obj.transform.SetParent(_Scene.SceneRoot);
                }
            }
        }

        public override void OnSceneExit(IUIScene nextScene)
        {
            _Scene.Unload();
            _ResHolder.Destroy();
        }
    }
}