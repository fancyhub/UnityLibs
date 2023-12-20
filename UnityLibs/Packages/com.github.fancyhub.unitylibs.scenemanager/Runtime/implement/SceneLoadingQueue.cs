using System;
using System.Collections.Generic;

namespace FH.SceneManagement
{
    internal sealed class SceneLoadingQueue
    {
        private ScenePool _Pool;
        private Queue<SceneID> _LoadingQueue = new Queue<SceneID>();
        private SceneID _CurrentLoading = SceneID.Empty;

        public SceneLoadingQueue(ScenePool pool)
        {
            _Pool = pool;
        }

        public void Enqueue(SceneItem item)
        {
            if (item == null)
                return;

            if (item._LoadMode == UnityEngine.SceneManagement.LoadSceneMode.Single)            
                _LoadingQueue.Clear();            

            _LoadingQueue.Enqueue(item._Id);
        }

        public void Update()
        {
            SceneItem item = _Pool.Get(_CurrentLoading);
            if (item != null)
            {
                if (item._Status == ESceneStatus.Loading)
                    return;
            }

            for (; ; )
            {
                _CurrentLoading = SceneID.Empty;
                if (_LoadingQueue.Count == 0)
                    return;

                _CurrentLoading = _LoadingQueue.Dequeue();
                item = _Pool.Get(_CurrentLoading);
                if (item == null)
                    continue;

                item.BeginLoad();
                if (item._Status == ESceneStatus.Loading)
                    return;
            }
        }
    }
}
