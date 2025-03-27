using System;
using System.Collections.Generic;

namespace FH.SceneManagement
{
    internal sealed class ScenePool : IScenePool
    {
        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        private MyDict<int, SceneItem> _Dict;
        public ScenePool()
        {
            _Dict = new MyDict<int, SceneItem>();
        }

        public SceneItem Get(int sceneId)
        {
            if (sceneId == 0)
                return null;

            _Dict.TryGetValue(sceneId, out var item);
            SceneLog._.Assert(item != null, "can't find scene {0}", sceneId);
            return item;
        }

        public void GetAllScenes(List<SceneRef> out_list)
        {
            foreach (var p in _Dict)
            {
                out_list.Add(new SceneRef(p.Key, this));
            }
        }

        public void UnloadScene(int sceneId)
        {
            if (sceneId == 0)
                return;

            _Dict.TryGetValue(sceneId, out var item);
            SceneLog._.Assert(item != null, "can't find scene {0}", sceneId);
            if (item == null)
                return;

            item.Unload();
        }

        public void Update()
        {
            foreach (var p in _Dict)
            {
                if (p.Value.ShouldBeDestroyed())
                {
                    p.Value.Destroy();
                    _Dict.Remove(p.Key);
                }
            }
        }

        public void Add(SceneItem item)
        {
            if (item == null)
                return;

            if (_Dict.ContainsKey(item.SceneId))
                return;

            if (item.IsSingleLoadMode())
            {
                foreach (var p in _Dict)
                {
                    p.Value.Unload();
                }
            }
            _Dict.Add(item.SceneId, item);
        }

        public void Destroy()
        {
            if (_Dict == null)
                return;

            ___ptr_ver++;

            foreach (var p in _Dict)
            {
                p.Value.Unload();
            }
        }

        public (bool done, float progress) GetSceneStat(int sceneId)
        {
            _Dict.TryGetValue(sceneId, out var item);
            if (item == null)
                return (true, 1);
            return item.GetSceneStat();
        }

        public bool IsValid(int sceneId)
        {
            if (!_Dict.TryGetValue(sceneId, out var item))
            {
                return false;
            }

            return item.IsValid();
        }

        public UnityEngine.SceneManagement.Scene GetUnityScene(int sceneId)
        {
            if (!_Dict.TryGetValue(sceneId, out var item))
            {
                return default;
            }
            return item.UnityScene;
        }
    }
}
