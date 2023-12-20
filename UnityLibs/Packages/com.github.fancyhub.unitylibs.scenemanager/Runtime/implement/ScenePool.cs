using System;
using System.Collections.Generic;

namespace FH.SceneManagement
{
    internal sealed class ScenePool : IScenePool
    {
        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        private MyDict<SceneID, SceneItem> _Dict;
        public ScenePool()
        {
            _Dict = new MyDict<SceneID, SceneItem>();
        }

        public SceneItem Get(SceneID id)
        {
            _Dict.TryGetValue(id, out var item);
            return item;
        }

        public void UnloadScene(SceneID id)
        {
            _Dict.TryGetValue(id, out var item);
            if (item == null)
                return;
            item.Unload();
        }

        public void Update()
        {
            foreach (var p in _Dict)
            {
                if (p.Value._Status == ESceneStatus.Unloaded || p.Value._Status == ESceneStatus.Failed)
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

            if (_Dict.ContainsKey(item._Id))
                return;

            if (item._LoadMode == UnityEngine.SceneManagement.LoadSceneMode.Single)
            {
                foreach (var p in _Dict)
                {
                    p.Value.Unload();
                }
            }
            _Dict.Add(item._Id, item);
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

        public (bool Done, float Progress) GetSceneStat(SceneID id)
        {
            _Dict.TryGetValue(id, out var item);
            if (item == null)
                return (true, 1);
            return item.GetSceneStat();
        }
    }
}
