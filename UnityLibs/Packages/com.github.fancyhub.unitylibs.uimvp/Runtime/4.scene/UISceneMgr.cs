using System;
using System.Collections.Generic;

namespace FH.UI
{
    public class UISceneMgr : IUISceneMgr
    {
        private static UISceneMgr _;
        public static UISceneMgr Inst
        {
            get
            {
                return _;
            }
        }

        protected SceneSharedData _SceneSharedData;
        protected CPtr<IUIScene> _CurrentScene;
        protected Type _LastSceneType;
        protected Type _CurrentSceneType;
        protected Type _NextSceneType;
        protected UIUpdaterMgr _UpdaterMgr;
        protected Func<Type, IUIScene> _SceneCreator;

        public UISceneMgr()
        {
            _ = this;
            SceneMgrUpdater.CreateUpdater(_Update);
            _UpdaterMgr = new UIUpdaterMgr();
        }

        public virtual UISceneMgr Init()
        {
            UISharedBG shared_bg = UISharedBG.Inst;
            _SceneSharedData = new SceneSharedData()
            {
                PageGroupMgr = new UIPageGroupMgr(),
                PageTagMgr = new UIPageTagMatrix(),
                ViewLayerMgr = new UIViewLayerMgr(shared_bg),
                SceneMgr = this,
            };

            return this;
        }

        public void SetSceneCreator(Func<Type, IUIScene> scene_creator)
        {
            _SceneCreator = scene_creator;
        }


        public static IUIScene CurrentScene
        {
            get
            {
                if (_ == null)
                    return null;
                return Inst._CurrentScene.Val;
            }
        }

        public T OpenPage<T>(PageOpenInfo pageOpenInfo) where T : class, IUIPage, IUIGroupPage, IUITagPage, IUILayerViewPage, IUIScenePage, new()
        {
            IUIScene scene = CurrentScene;
            if (scene == null)
                return null;
            return scene.OpenPage<T>(pageOpenInfo);
        }

        public static T OpenUI<T>(PageOpenInfo pageOpenInfo) where T : UIPageBase, new()
        {
            if (_ == null)
                return null;
            return _.OpenPage<T>(pageOpenInfo);
        }

        public static T OpenUI<T>() where T : UIPageBase, new()
        {

            return OpenUI<T>(PageOpenInfo.Default);
        }

        public static void ChangeScene<T>() where T : IUIScene
        {
            if (_ == null)
                return;

            _._NextSceneType = typeof(T);
        }

        public static bool AddUpdate(IUIUpdater updater)
        {
            if (_ == null)
                return false;
            return _._UpdaterMgr.AddUpdate(updater);
        }

        public static int AddUpdate(ActionUIUpdate action)
        {
            if (_ == null)
                return 0;
            return _._UpdaterMgr.AddUpdate(action);
        }

        public static bool RemoveUpdate(int id)
        {
            if (_ == null)
                return false;
            return _._UpdaterMgr.RemoveUpdate(id);
        }

        private void _Update()
        {
            _SwitchScene();

            _UpdaterMgr.Update();
        }

        private void _SwitchScene()
        {
            if (_NextSceneType == null)
                return;

            //销毁旧的
            _CurrentScene.Val?.OnSceneExit(_NextSceneType);
            _CurrentScene.Destroy();
            _LastSceneType = _CurrentSceneType;

            //创建新的
            _CurrentSceneType = _NextSceneType;
            _NextSceneType = null;

            var newScene = _SceneCreator(_CurrentSceneType);
            newScene.SceneSharedData = _SceneSharedData;
            _CurrentScene = new CPtr<IUIScene>(newScene);

            newScene?.OnSceneEnter(_LastSceneType);
        }


        private class SceneMgrUpdater : UnityEngine.MonoBehaviour
        {
            private Action _ActionUpdate;
            public static void CreateUpdater(Action action)
            {
                if (!UnityEngine.Application.isPlaying || action == null)
                    return;

                UnityEngine.GameObject obj = new UnityEngine.GameObject("SceneMgrUpdater");
                SceneMgrUpdater updater = obj.AddComponent<SceneMgrUpdater>();
                updater._ActionUpdate = action;
                UnityEngine.GameObject.DontDestroyOnLoad(obj);
                obj.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
            }

            public void Update()
            {
                _ActionUpdate();
            }
        }
    }
}
