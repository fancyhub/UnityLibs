using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.SceneManagement
{
    internal sealed class SceneMgrImplement : ISceneMgr
    {
        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        private ScenePool _Pool;
        private SceneLoadingQueue _LoadingQueue;
        private CPtr<ISceneMgr.IExternalLoader> _ExternalLoader;
        private ScenePlaceHolderItem _PlaceHolderScene;

        static SceneMgrImplement()
        {
        }

        public SceneMgrImplement(ISceneMgr.IExternalLoader external_loader)
        {
            _ExternalLoader = new CPtr<ISceneMgr.IExternalLoader>(external_loader);
            _PlaceHolderScene = new ScenePlaceHolderItem();
            _Pool = new ScenePool();
            _LoadingQueue = new SceneLoadingQueue(_Pool);
        }

        public void GetAllScenes(List<SceneRef> out_list)
        {
            _Pool.GetAllScenes(out_list);
        }

        public SceneRef LoadScene(string scene_path, UnityEngine.SceneManagement.LoadSceneMode loadMode)
        {
            ISceneMgr.IExternalLoader scene_loader = _ExternalLoader.Val;
            if (scene_loader == null)
            {
                SceneLog._.E("SceneLoader Is Null");
                return SceneRef.Empty;
            }

            ISceneMgr.IExternalRef scene_ref = scene_loader.CreateSceneRef(scene_path);

            if (scene_ref == null)
            {
                SceneLog._.Assert(false, "load scene failed: {0}", scene_path);
                return SceneRef.Empty;
            }

            SceneItem sceneItem = SceneItem.Create(_PlaceHolderScene, scene_ref, scene_path, new LoadSceneParameters(loadMode));

            SceneLog._.D("Scene:{0} CreateScene, Mode: {1}, Path: {2}", sceneItem.SceneId, loadMode, scene_path);

            _Pool.Add(sceneItem);
            _LoadingQueue.Enqueue(sceneItem);

            return new SceneRef(sceneItem.SceneId, sceneItem);
        }

        public void Update()
        {
            _LoadingQueue.Update();
            _Pool.Update();
        }

        public void UnloadAll()
        {
            _Pool.UnloadAll();
        }

        public void Destroy()
        {
            throw new Exception("Can't destroy scene manager");
        }
    }
}
