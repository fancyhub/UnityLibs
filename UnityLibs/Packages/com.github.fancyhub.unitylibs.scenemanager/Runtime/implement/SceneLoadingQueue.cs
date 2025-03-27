using System;
using System.Collections.Generic;

namespace FH.SceneManagement
{
    internal sealed class SceneLoadingQueue
    {
        private ScenePool _Pool;
        private Queue<int> _LoadingQueue = new Queue<int>();
        private int _CurrentLoading = 0;

        public SceneLoadingQueue(ScenePool pool)
        {
            _Pool = pool;
        }

        public void Enqueue(SceneItem item)
        {
            if (item == null)
                return;

            if (item.IsSingleLoadMode())
                _LoadingQueue.Clear();

            _LoadingQueue.Enqueue(item.SceneId);
        }

        public void Update()
        {
            SceneItem item = _Pool.Get(_CurrentLoading);
            if (item != null)
            {
                if (item.IsLoading())
                    return;
            }

            for (; ; )
            {
                _CurrentLoading = 0;
                if (_LoadingQueue.Count == 0)
                    return;

                _CurrentLoading = _LoadingQueue.Dequeue();
                item = _Pool.Get(_CurrentLoading);
                if (item == null)
                    continue;

                item.BeginLoad();
                if (item.IsLoading())
                    return;
            }
        }
    }
}
