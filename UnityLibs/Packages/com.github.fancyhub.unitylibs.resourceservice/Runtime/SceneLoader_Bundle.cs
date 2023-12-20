/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH
{
    public sealed class SceneLoader_Bundle : ISceneLoader
    {
        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        private CPtr<IBundleMgr> _BundleMgr;

        public SceneLoader_Bundle(IBundleMgr bundleMgr)
        {
            _BundleMgr = new CPtr<IBundleMgr>(bundleMgr);
        }

        public void Destroy()
        {

        }

        public ISceneRef Load(string scene, LoadSceneMode mode)
        {
            IBundleMgr bundle_mgr = _BundleMgr.Val;
            if (bundle_mgr == null)
            {
                Log.E("BundleMgr is null");
                return null;
            }

            IBundle bundle = bundle_mgr.LoadBundleByAsset(scene);
            if (bundle == null)
                return null;

            return SceneRef.Create(bundle, scene, new LoadSceneParameters(mode));
        }

        private sealed class SceneRef : CPoolItemBase, ISceneRef
        {
            public AsyncOperation _AsyncOperation;
            public string _SceneName;
            public LoadSceneParameters _LoadParams;
            public IBundle _Bundle;

            public AsyncOperation LoadScene()
            {
                if (_AsyncOperation != null)
                    return _AsyncOperation;
                _AsyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_SceneName, _LoadParams);
                return _AsyncOperation;
            }

            public static SceneRef Create(IBundle bundle, string scene, LoadSceneParameters load_param)
            {
                if (bundle == null)
                    return null;

                var ret = GPool.New<SceneRef>();
                ret._Bundle = bundle;
                ret._SceneName = scene;
                ret._LoadParams = load_param;
                return ret;
            }

            protected override void OnPoolRelease()
            {
                _Bundle?.DecRefCount();
                _Bundle = null;
                _AsyncOperation = null;
            }
        }
    }
}
