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

        public DynamicCoroutineComp _Comp;
        protected override void OnUI2Init()
        {
            base.OnUI2Init();

            BaseView._BtnClose.OnClick = this.UIClose;
            BaseView._BtnLoadScene.OnClick = _OnTestSceneClick;
            BaseView._BtnUnloadFirstScene.OnClick = _OnCloseFirstSceneClick;
            BaseView._BtnUnloadLastScene.OnClick = _OnCloseLastSceneClick;
            _Comp = BaseView.SelfRoot.ExtGetComp<DynamicCoroutineComp>(true);
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

        private void _OnCloseFirstSceneClick()
        {
            if (_SceneRefList.Count == 0)
                return;
            _SceneRefList[0].Unload();
            _SceneRefList.RemoveAt(0);

            FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
        }


        private void _OnCloseLastSceneClick()
        {
            if (_SceneRefList.Count == 0)
                return;
            _SceneRefList[_SceneRefList.Count - 1].Unload();
            _SceneRefList.RemoveAt(_SceneRefList.Count - 1);
            FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
        }

        private void _OnTestSceneClick()
        {
            _Comp.StartCoroutine(_TestSceneLoad());
        }

        private IEnumerator _TestSceneLoad()
        {
            yield return new WaitForSeconds(1.0f);

            var scene_a = SceneMgr.LoadScene("Assets/Scenes/a.unity", true);
            var scene_b = SceneMgr.LoadScene("Assets/Scenes/a.unity", true);

            for (; ; )
            {
                if (scene_a.IsDone)
                {
                    _SceneRefList.Add(scene_a);
                    FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
                    break;
                }
                yield return string.Empty;
            }

            for (; ; )
            {
                if (scene_b.IsDone)
                {
                    _SceneRefList.Add(scene_b);
                    FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
                    break;
                }
                yield return string.Empty;
            }
        }
    }
}
