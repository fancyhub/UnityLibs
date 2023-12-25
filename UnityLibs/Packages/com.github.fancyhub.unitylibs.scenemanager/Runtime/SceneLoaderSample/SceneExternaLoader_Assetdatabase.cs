
/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/20
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.SceneManagement.SampleExternalLoader
{
#if UNITY_EDITOR
    public class SceneExternaLoader_Assetdatabase : ISceneMgr.IExternalLoader
    {
        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        private sealed class SceneRef : CPoolItemBase, ISceneMgr.IExternalRef
        {
            public AsyncOperation _AsyncOperation;
            public string _SceneName;
            public LoadSceneParameters _LoadParams;
            public AsyncOperation LoadScene()
            {
                if (_AsyncOperation != null)
                {
                    return _AsyncOperation;
                }
                _AsyncOperation = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(_SceneName, _LoadParams);
                return _AsyncOperation;
            }

            public static SceneRef Create(string scene, LoadSceneParameters load_param)
            {
                var ret = GPool.New<SceneRef>();
                ret._SceneName = scene;
                ret._LoadParams = load_param;
                return ret;
            }

            protected override void OnPoolRelease()
            {
                _AsyncOperation = null;

            }
        }

        public void Destroy()
        {
        }        

        public ISceneMgr.IExternalRef Load(string scene, LoadSceneMode load_mod)
        {
            return SceneRef.Create(scene, new LoadSceneParameters(load_mod));
        }
    }
#endif
}
