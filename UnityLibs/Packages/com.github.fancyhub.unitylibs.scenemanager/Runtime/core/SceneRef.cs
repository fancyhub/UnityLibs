/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/20
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    internal interface IScenePool : ICPtr
    {
        void UnloadScene(int sceneId);
        (bool done, float progress) GetSceneStat(int sceneId);
        bool IsValid(int sceneId);
        UnityEngine.SceneManagement.Scene GetUnityScene(int sceneId);
    }


    public struct SceneRef
    {
        public static SceneRef Empty = new SceneRef();

        public readonly int SceneId;
        private CPtr<IScenePool> _ScenePool;
        public void Unload()
        {
            if (SceneId == 0)
            {
                SceneManagement.SceneLog._.D("scene id is 0, can't unload");
                return;
            }

            if (_ScenePool.Val == null)
            {
                SceneManagement.SceneLog._.Assert(false, "can't unload scene {0}, scene pool is null", SceneId);
                return;
            }
            _ScenePool.Val?.UnloadScene(SceneId);
        }

        public bool IsDone
        {
            get
            {
                IScenePool pool = _ScenePool.Val;
                if (pool == null)
                    return true;
                return pool.GetSceneStat(SceneId).done;
            }
        }

        public float Progress
        {
            get
            {
                IScenePool pool = _ScenePool.Val;
                if (pool == null)
                    return 1;
                return pool.GetSceneStat(SceneId).progress;
            }
        }

        public bool IsValid
        {
            get
            {
                IScenePool pool = _ScenePool.Val;
                if (pool == null)
                    return false;
                return pool.IsValid(SceneId);
            }
        }

        public UnityEngine.SceneManagement.Scene UnityScene
        {
            get
            {
                IScenePool pool = _ScenePool.Val;
                if (pool == null)
                    return default;
                return pool.GetUnityScene(SceneId);
            }
        }


        

        internal SceneRef(int sceneId, IScenePool scene_pool)
        {
            this.SceneId = sceneId;
            _ScenePool = new CPtr<IScenePool>(scene_pool);
        }


    }
}
