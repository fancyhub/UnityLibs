using System;
using System.Collections.Generic;

namespace FH.UI
{
    public struct PageOpenInfo
    {
        public static PageOpenInfo Default = new PageOpenInfo()
        {
            GroupChannel = EUIPageGroupChannel.Free,
            GroupUniquePage = false,

            AddToScene = true,
            ViewLayer = EUIViewLayer.Normal,

            Tag = EUITagIndex.None,

            ResHolder = null,
        };

        public static PageOpenInfo CreateDefaultSubPage(UnityEngine.RectTransform parent, IResInstHolder resHolder)
        {
            return new PageOpenInfo()
            {
                GroupChannel = EUIPageGroupChannel.None,
                GroupUniquePage = false,
                AddToScene = false,

                ViewParent = parent,
                ViewLayer = EUIViewLayer.Normal,

                Tag = EUITagIndex.None,
                ResHolder = resHolder,
            };
        }

        public EUIPageGroupChannel GroupChannel;
        public bool GroupUniquePage;

        public bool AddToScene;

        public UnityEngine.RectTransform ViewParent;
        public EUIViewLayer ViewLayer;

        public EUITagIndex Tag;

        public IResInstHolder ResHolder;
    }

    public abstract class UISceneMgr : IUISceneMgr
    {
        private static UISceneMgr _;
        public static UISceneMgr Inst
        {
            get
            {
                return _;
            }
        }

        protected CPtr<IUIScene> _CurrentScene;
        private PtrList _CurrentScenePages;
        protected CPtr<IUIScene> _NextScene;

        protected UIUpdaterMgr _UpdaterMgr;
        protected UIViewLayerMgr _ViewLayerMgr;
        protected UIPageTagMgr _TagMgr;
        protected UIPageGroupMgr _GroupMgr;

        public UISceneMgr()
        {
            _ = this;
            SceneMgrUpdater.CreateUpdater(_Update);
            _UpdaterMgr = new UIUpdaterMgr();
            FH.UI.UIObjFinder.Show();
        }

        public virtual UISceneMgr Init()
        {
            _ViewLayerMgr = new UIViewLayerMgr(UIRoot.Root2D);
            _ViewLayerMgr.AddLayer(EUIViewLayer.Normal.ToString(), true);
            _ViewLayerMgr.AddLayer(EUIViewLayer.Dialog.ToString(), true);
            _ViewLayerMgr.AddLayer(EUIViewLayer.Top.ToString(), false);

            List<UITagItem> tagItems = new List<UITagItem>()
            {
                new UITagItem(EUITagIndex.BG, EUITag.BG),
                new UITagItem(EUITagIndex.FullScreenDialog,new BitEnum64<EUITag>(EUITag.BG)),
                new UITagItem(EUITagIndex.Dialog),
            };

            _TagMgr = new UIPageTagMgr(tagItems);

            _GroupMgr = new UIPageGroupMgr();

            return this;
        }

        public void AddPage(IUIScenePage page, bool add_to_scene)
        {
            if (page == null)
                return;

            page.SetUIScenePageInfo(new UIScenePageInfo()
            {
                Mgr = this
            });

            if (add_to_scene)
                _CurrentScenePages += page;
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

        public static T OpenUI<T>(PageOpenInfo pageOpenInfo) where T : UIPageBase, new()
        {
            if (_ == null)
                return null;

            T ret = new T();

            _._GroupMgr?.AddPage(ret, pageOpenInfo.GroupChannel);
            _._TagMgr?.AddTag(ret, pageOpenInfo.Tag);
            _.AddPage(ret, pageOpenInfo.AddToScene);
            _._ViewLayerMgr?.AddPage(ret, pageOpenInfo.ViewLayer, pageOpenInfo.ViewParent);
            ((IUIResPage)ret).SetResHolder(pageOpenInfo.ResHolder);

            ret.UIOpen();
            return ret;
        }

        public static T OpenUI<T>(
               EUIPageGroupChannel GroupChannel = EUIPageGroupChannel.Free,
               bool GroupUniquePage = false,
               bool AddToScene = true,
               EUIViewLayer ViewLayer = EUIViewLayer.Normal,
               UnityEngine.RectTransform Parent = null,
               EUITagIndex Tag = EUITagIndex.None,
               IResInstHolder ResHolder = null) where T : UIPageBase, new()
        {
            PageOpenInfo defaultInfo = PageOpenInfo.Default;
            defaultInfo.GroupChannel = GroupChannel;
            defaultInfo.GroupUniquePage = GroupUniquePage;
            defaultInfo.AddToScene = AddToScene;
            defaultInfo.ViewLayer = ViewLayer;
            defaultInfo.ViewParent = Parent;
            defaultInfo.Tag = Tag;
            defaultInfo.ResHolder = ResHolder;
            return OpenUI<T>(PageOpenInfo.Default);
        }

        public static void ChangeScene<T>() where T : class, IUIScene
        {
            if (_ == null)
                return;

            _._NextScene.Destroy();
            _._NextScene = new CPtr<IUIScene>(_.CreateScene<T>());
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

        public static int AddUpdateForever(Action action)
        {
            if (_ == null)
                return 0;
            if (action == null)
                return 0;

            return _._UpdaterMgr.AddUpdate(() =>
            {
                action();
                return EUIUpdateResult.Continue;
            });
        }

        public static bool RemoveUpdate(int id)
        {
            if (_ == null)
                return false;
            return _._UpdaterMgr.RemoveUpdate(id);
        }

        protected abstract IUIScene CreateScene<T>();


        private void _Update()
        {
            _SwitchScene();
            _CurrentScene.Val?.OnUpdate();
            _UpdaterMgr.Update();
        }

        private void _SwitchScene()
        {
            IUIScene nextScene = _NextScene.Val;
            if (nextScene == null)
                return;

            IUIScene currentScene = _CurrentScene.Val;
            _CurrentScene = new CPtr<IUIScene>(nextScene);
            _NextScene = null;

            //退出旧的
            currentScene?.OnSceneExit(nextScene);
            _CurrentScenePages?.Destroy();
            _CurrentScenePages = null;

            //进入新的
            nextScene.OnSceneEnter(currentScene);

            currentScene?.Destroy();
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
