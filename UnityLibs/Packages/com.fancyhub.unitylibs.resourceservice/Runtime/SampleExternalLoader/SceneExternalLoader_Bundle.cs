/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.SampleExternalLoader
{
    public sealed class SceneExternalLoader_Bundle : ISceneMgr.IExternalLoader
    {
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        private CPtr<IBundleMgr> _BundleMgr;

        public SceneExternalLoader_Bundle(IBundleMgr bundleMgr)
        {
            _BundleMgr = new CPtr<IBundleMgr>(bundleMgr);
        }

        public void Destroy()
        {

        }

        public ISceneMgr.IExternalRef CreateSceneRef(string scene)
        {
            IBundleMgr bundle_mgr = _BundleMgr.Val;
            if (bundle_mgr == null)
            {
                Log.E("BundleMgr is null");
                return null;
            }

            ICSPtr<IBundle> bundle = bundle_mgr.LoadBundleByAsset(scene);
            if (bundle == null)
                return null;

            return SceneRef.Create(bundle, scene);
        }

        private sealed class SceneRef : CPoolItemBase, ISceneMgr.IExternalRef
        {
            public AsyncOperation _AsyncOperation;
            public string _SceneName;
            public SPtr<IBundle> _Bundle;

            public AsyncOperation Load(LoadSceneParameters load_param)
            {
                if (_AsyncOperation != null)
                    return _AsyncOperation;
                _AsyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(_SceneName, load_param);
                return _AsyncOperation;
            }

            public static SceneRef Create(ICSPtr<IBundle> bundle, string scene)
            {
                if (bundle == null)
                    return null;

                var ret = GPool.New<SceneRef>();
                ret._Bundle = new SPtr<IBundle>(bundle);
                ret._SceneName = scene;
                return ret;
            }

            protected override void OnPoolRelease()
            {
                _Bundle.Destroy();
                _AsyncOperation = null;
            }
        }
    }
}
