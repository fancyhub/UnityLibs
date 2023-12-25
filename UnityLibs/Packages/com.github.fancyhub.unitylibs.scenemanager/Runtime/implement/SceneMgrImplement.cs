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
            MyEqualityComparer.Reg(new SceneID());
        }

        public SceneMgrImplement(ISceneMgr.IExternalLoader external_loader)
        {
            _ExternalLoader = new CPtr<ISceneMgr.IExternalLoader>(external_loader);
            _PlaceHolderScene = new ScenePlaceHolderItem();
            _Pool = new ScenePool();
            _LoadingQueue = new SceneLoadingQueue(_Pool);
        }

        public SceneRef LoadScene(string scene_path, bool additive)
        {
            ISceneMgr.IExternalLoader scene_loader = _ExternalLoader.Val;
            if (scene_loader == null)
            {
                SceneLog._.E("SceneLoader Is Null");
                return SceneRef.Empty;
            }

            LoadSceneMode load_mode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            ISceneMgr.IExternalRef scene_ref = scene_loader.Load(scene_path, load_mode);

            if (scene_ref == null)
                return SceneRef.Empty;

            SceneItem sceneItem = SceneItem.Create(scene_ref, scene_path, load_mode);
            if (sceneItem == null)
                return SceneRef.Empty;

            _Pool.Add(sceneItem);
            _LoadingQueue.Enqueue(sceneItem);

            return new SceneRef(sceneItem._Id, _Pool);
        }

        public void Update()
        {
            _PlaceHolderScene.Update();

            _LoadingQueue.Update();
            _Pool.Update();
        }

        public void Destroy()
        {
            _Pool.Destroy();
            _Pool = null;
            _LoadingQueue = null;
        }
    }
}
