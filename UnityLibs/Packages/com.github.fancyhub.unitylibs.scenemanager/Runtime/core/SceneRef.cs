/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/20
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    public struct SceneRef
    {
        public static SceneRef Empty = new SceneRef();

        public readonly SceneID Id;
        private CPtr<IScenePool> _ScenePool;
        public void Unload() { _ScenePool.Val?.UnloadScene(Id); }
        public bool IsDone
        {
            get
            {
                IScenePool pool = _ScenePool.Val;
                if (pool == null)
                    return true;
                return pool.GetSceneStat(Id).Done;
            }
        }
        public float Progress
        {
            get
            {
                IScenePool pool = _ScenePool.Val;
                if (pool == null)
                    return 1;
                return pool.GetSceneStat(Id).Progress;
            }
        }


        internal SceneRef(SceneID id, IScenePool scene_pool)
        {
            Id = id;
            _ScenePool = new CPtr<IScenePool>(scene_pool);
        }
    }
}
