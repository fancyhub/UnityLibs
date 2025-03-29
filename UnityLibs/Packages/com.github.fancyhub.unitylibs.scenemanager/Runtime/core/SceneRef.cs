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
    internal interface IScene : ICPtr
    {
        public void Unload();

        (bool done, float progress) Stat { get; }
        UnityEngine.SceneManagement.Scene UnityScene { get; }
        bool Valid { get; }
        Vector3 ScenePos { get; set; }
        bool SceneVisible { get; set; }

        Transform SceneRoot { get; }
        Transform CreateSceneRoot();
    }

    public struct SceneRef
    {
        public static SceneRef Empty = new SceneRef();

        public readonly int SceneId;
        private CPtr<IScene> _Scene;
        public void Unload()
        {
            if (SceneId == 0)
            {
                SceneManagement.SceneLog._.D("scene id is 0, can't unload");
                return;
            }

            if (_Scene.Val == null)
            {
                SceneManagement.SceneLog._.Assert(false, "can't unload scene {0}, scene ref is null", SceneId);
                return;
            }
            _Scene.Val?.Unload();
        }

        public bool IsDone
        {
            get
            {
                IScene scene = _Scene.Val;
                if (scene == null)
                    return true;
                return scene.Stat.done;
            }
        }

        public float Progress
        {
            get
            {
                IScene scene = _Scene.Val;
                if (scene == null)
                    return 1;
                return scene.Stat.progress;
            }
        }

        public bool IsValid
        {
            get
            {
                IScene scene = _Scene.Val;
                if (scene == null)
                    return false;
                return scene.Valid;
            }
        }

        public UnityEngine.SceneManagement.Scene UnityScene
        {
            get
            {
                IScene scene = _Scene.Val;
                if (scene == null)
                    return default;
                return scene.UnityScene;
            }
        }

        public Transform SceneRoot
        {
            get
            {
                IScene scene = _Scene.Val;
                if (scene == null)
                    return default;
                return scene.SceneRoot;
            }
        }

        public Transform CreateSceneRoot()
        {
            IScene scene = _Scene.Val;
            if (scene == null)
                return default;
            return scene.CreateSceneRoot();
        }

        public Vector3 ScenePos
        {
            get
            {
                IScene scene = _Scene.Val;
                if (scene == null)
                    return default;
                return scene.ScenePos;
            }

            set
            {
                IScene scene = _Scene.Val;
                if (scene == null)
                    return;
                scene.ScenePos = value;
            }
        }

        public bool SceneVisible
        {
            get
            {
                IScene scene = _Scene.Val;
                if (scene == null)
                    return true;
                return scene.SceneVisible;
            }

            set
            {
                IScene scene = _Scene.Val;
                if (scene == null)
                    return;
                scene.SceneVisible = value;
            }
        }

        internal SceneRef(int sceneId, IScene scene)
        {
            this.SceneId = sceneId;
            _Scene = new CPtr<IScene>(scene);
        }
    }
}
