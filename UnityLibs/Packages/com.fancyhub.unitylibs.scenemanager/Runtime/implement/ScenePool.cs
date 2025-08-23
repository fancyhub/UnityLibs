using System;
using System.Collections.Generic;

namespace FH.SceneManagement
{
    internal sealed class ScenePool
    {
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
                out_list.Add(new SceneRef(p.Key, p.Value));
            }
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

        public void UnloadAll()
        {
            if (_Dict == null)
                return;


            foreach (var p in _Dict)
            {
                p.Value.Unload();
            }             
        }
    }
}
