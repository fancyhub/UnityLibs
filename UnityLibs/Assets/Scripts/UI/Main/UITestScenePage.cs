using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;

namespace Game
{
    public class UITestScenePage : FH.UI.UIPageBase<UITestSceneView>
    {
        public List<SceneRef> _SceneRefList = new List<SceneRef>();
        public List<UIServerItemPage> _ItemViews = new List<UIServerItemPage>();

        public DynamicCoroutineComp _Comp;
        protected override void OnUI2Init()
        {
            base.OnUI2Init();

            BaseView._BtnClose.OnClick = this.UIClose;
            BaseView._BtnLoadSceneSingle.OnClick = _OnLoadSingle;
            BaseView._BtnLoadSceneAdditive.OnClick = _OnLoadAdditive;

            _Comp = BaseView.SelfRoot.ExtGetComp<DynamicCoroutineComp>(true);
        }

        protected override void OnUI3Show()
        {
            base.OnUI3Show();

            FH.UI.UISceneMgr.AddUpdate(_Update);
        }

        protected override void OnUI5Close()
        {
            foreach (var p in _SceneRefList)
            {
                p.Unload();
            }
            _SceneRefList.Clear();


            FH.UI.UIRedDotMgr.Set("root.test.scene", 1);
        }

        private FH.UI.EUIUpdateResult _Update()
        {
            if (!IsVisible || IsDestroyed())
                return FH.UI.EUIUpdateResult.Stop;

            bool isDirty = false;
            for (int i = _SceneRefList.Count - 1; i >= 0; i--)
            {
                if (!_SceneRefList[i].IsValid)
                {

                    _SceneRefList.RemoveAt(i);
                    isDirty = true;
                }
            }

            if (isDirty)
                FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);

            //1. 删除多余的
            for (int i = _ItemViews.Count - 1; i >= _SceneRefList.Count; i--)
            {
                _ItemViews[i].Destroy();
                _ItemViews.RemoveAt(i);
            }

            //2. 创建新的
            for (int i = _ItemViews.Count; i < _SceneRefList.Count; i++)
            {
                var openInfo = FH.UI.PageOpenInfo.CreateDefaultSubPage(BaseView._ItemList, ResHolder);
                var item = OpenChildPage<UIServerItemPage>(openInfo);
                _ItemViews.Add(item);
            }

            //3. 数据同步
            for (int i = 0; i < _SceneRefList.Count; i++)
            {
                _ItemViews[i].SetData(_SceneRefList[i]);
            }
            return FH.UI.EUIUpdateResult.Continue;
        }

        private void _OnLoadSingle()
        {
            var scene_a = SceneMgr.LoadScene("Assets/Scenes/a.unity", UnityEngine.SceneManagement.LoadSceneMode.Single);
            var scene_b = SceneMgr.LoadScene("Assets/Scenes/b.unity", UnityEngine.SceneManagement.LoadSceneMode.Single);

            _SceneRefList.Add(scene_a);
            _SceneRefList.Add(scene_b);

            FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
        }

        private void _OnLoadAdditive()
        {
            var scene_a = SceneMgr.LoadScene("Assets/Scenes/a.unity", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            var scene_b = SceneMgr.LoadScene("Assets/Scenes/b.unity", UnityEngine.SceneManagement.LoadSceneMode.Additive);

            Log.I("LoadScene {0}", scene_a.SceneId);
            _SceneRefList.Add(scene_a);
            _SceneRefList.Add(scene_b);

            FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
        }
    }


    public class UIServerItemPage : FH.UI.UIPageBase<Game.UISceneItemView>
    {
        private FH.SceneRef _Data;
        public UIServerItemPage()
        {
        }

        protected override void OnUI2Init()
        {
            base.OnUI2Init();

            BaseView._Unload.OnClick = () =>
                {
                    _Data.Unload();
                };
        }

        protected override void OnUI3Show()
        {
            base.OnUI3Show();
            _Apply();
        }

        public void SetData(FH.SceneRef scene)
        {
            _Data = scene;
            _Apply();
        }

        private void _Apply()
        {
            if (BaseView == null)
                return;

            BaseView._Name.text = $"{_Data.SceneId}, Name:{_Data.UnityScene.name}_Valid:{_Data.IsValid}_Done:{_Data.IsDone} ";
        }
    }
}
