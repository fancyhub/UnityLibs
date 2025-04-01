using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.SceneManagement
{
    internal sealed class SceneMgrImplement : ISceneMgr
    {
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        private ScenePool _pool;
        private SceneLoadingQueue _loading_queue;
        private CPtr<ISceneMgr.IExternalLoader> _external_loader;
        private ScenePlaceHolderItem _place_holder_scene;
        private bool _in_upgrade;

        static SceneMgrImplement()
        {
        }

        public SceneMgrImplement(ISceneMgr.IExternalLoader external_loader)
        {
            _external_loader = new CPtr<ISceneMgr.IExternalLoader>(external_loader);
            _place_holder_scene = new ScenePlaceHolderItem();
            _pool = new ScenePool();
            _loading_queue = new SceneLoadingQueue(_pool);
        }

        public void GetAllScenes(List<SceneRef> out_list)
        {
            _pool.GetAllScenes(out_list);
        }

        public SceneRef LoadScene(string scene_path, UnityEngine.SceneManagement.LoadSceneMode loadMode)
        {
            ISceneMgr.IExternalLoader scene_loader = _external_loader.Val;
            if (scene_loader == null)
            {
                SceneLog._.E("SceneLoader Is Null");
                return SceneRef.Empty;
            }
            if (_in_upgrade)
            {
                SceneLog._.E("InUpgrade");
                return SceneRef.Empty;
            }

            ISceneMgr.IExternalRef scene_ref = scene_loader.CreateSceneRef(scene_path);

            if (scene_ref == null)
            {
                SceneLog._.Assert(false, "load scene failed: {0}", scene_path);
                return SceneRef.Empty;
            }

            SceneItem sceneItem = SceneItem.Create(_place_holder_scene, scene_ref, scene_path, new LoadSceneParameters(loadMode));

            SceneLog._.D("Scene:{0} CreateScene, Mode: {1}, Path: {2}", sceneItem.SceneId, loadMode, scene_path);

            _pool.Add(sceneItem);
            _loading_queue.Enqueue(sceneItem);

            return new SceneRef(sceneItem.SceneId, sceneItem);
        }

        public void Update()
        {
            _loading_queue.Update();
            _pool.Update();
        }

        public void UnloadAll()
        {
            _pool.UnloadAll();
        }

        public void Destroy()
        {
            throw new Exception("Can't destroy scene manager");
        }

        #region upgrade
        public SceneMgrUpgradeOperation BeginUpgrade()
        {
            _in_upgrade = true;
            var (total_count, _) = _GetAsyncJobs();
            SceneMgrUpgradeOperation ret = new SceneMgrUpgradeOperation(total_count);
            ret.FuncGetStat = _GetAsyncJobs;
            return ret;
        }

        public void EndUpgrade(bool result)
        {
            _in_upgrade = false;
        }

        private (int remain_count, bool all_done) _GetAsyncJobs()
        {
            int c = _loading_queue.GetCount();
            if (c == 0)
                return (0, true);
            return (c, false);

        }
        #endregion
    }
}
