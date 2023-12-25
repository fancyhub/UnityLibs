using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH.SceneManagement
{
    public enum ESceneStatus
    {
        None,
        Loading,
        Loaded,
        Unloading,
        Loading2Unloading,
        Unloaded,
        Failed,
    }
    /*
     
    switch (_Status)
    {
        case ESceneStatus.None:
            break;

        case ESceneStatus.Loading:
            break;

        case ESceneStatus.Loaded:            
            break;

        case ESceneStatus.Unloading:
            break;

        case ESceneStatus.Loading2Unloading:
            break;

        case ESceneStatus.Unloaded:
            break;

        case ESceneStatus.Failed:
            break;
    }
     
     */

    internal class SceneItem : CPoolItemBase
    {
        public string _ScenePath;
        public LoadSceneMode _LoadMode;
        public SceneID _Id;
        public ESceneStatus _Status;
        public AsyncOperation _LoadingAsyncOperation;
        public AsyncOperation _UnloadingAsyncOperation;
        public CPtr<ISceneMgr.IExternalRef> _SceneRef;
        public Scene _Scene;

        public static SceneItem Create(ISceneMgr.IExternalRef scene_ref, string scene_path, LoadSceneMode mode)
        {
            SceneItem ret = new SceneItem();
            ret._Id = SceneID.Create();
            ret._ScenePath = scene_path;
            ret._LoadMode = mode;
            ret._Status = ESceneStatus.None;
            ret._SceneRef = new CPtr<ISceneMgr.IExternalRef>(scene_ref);
            return ret;
        }

        public (bool Done, float Progress) GetSceneStat()
        {
            switch (_Status)
            {
                default:
                    return (false, 0);
                case ESceneStatus.None:
                    return (false, 0);

                case ESceneStatus.Loading:
                    return (false, _LoadingAsyncOperation.progress);

                case ESceneStatus.Loaded:
                    return (true, 1);

                case ESceneStatus.Unloading:
                    return (true, 1);

                case ESceneStatus.Loading2Unloading:
                    return (false, _LoadingAsyncOperation.progress);

                case ESceneStatus.Unloaded:
                    return (true, 1);

                case ESceneStatus.Failed:
                    return (true, 1);
            }
        }

        public void Unload()
        {
            switch (_Status)
            {
                case ESceneStatus.None:
                    _Status = ESceneStatus.Unloaded;
                    break;

                case ESceneStatus.Loading:
                    _Status = ESceneStatus.Loading2Unloading;
                    break;

                case ESceneStatus.Loaded:
                    if (!_Scene.IsValid())
                        _Status = ESceneStatus.Unloaded;
                    else
                    {
                        _UnloadingAsyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_Scene);
                        if (_UnloadingAsyncOperation == null)
                            _Status = ESceneStatus.Unloaded;
                        else
                        {
                            _Status = ESceneStatus.Unloading;
                            _UnloadingAsyncOperation.completed += _OnSceneUnloaded;
                        }
                    }
                    break;

                case ESceneStatus.Unloading:
                    //Do nothing
                    break;

                case ESceneStatus.Loading2Unloading:
                    //Do nothing
                    break;

                case ESceneStatus.Unloaded:
                    //Do nothing
                    break;

                case ESceneStatus.Failed:
                    _Status = ESceneStatus.Unloaded;
                    break;
            }
        }

        public void BeginLoad()
        {
            switch (_Status)
            {
                case ESceneStatus.None:
                    _LoadingAsyncOperation = _SceneRef.Val.LoadScene();
                    if (_LoadingAsyncOperation == null)
                    {
                        SceneLog._.E("加载场景失败 {0}", _ScenePath);
                        _Status = ESceneStatus.Failed;
                    }
                    else
                    {
                        _LoadingAsyncOperation.completed += _OnSceneLoaded;
                        _Status = ESceneStatus.Loading;
                    }
                    break;

                case ESceneStatus.Loading:
                case ESceneStatus.Loaded:
                case ESceneStatus.Unloading:
                case ESceneStatus.Loading2Unloading:
                case ESceneStatus.Unloaded:
                case ESceneStatus.Failed:
                    //Do Nothing
                    break;

            }
        }

        private void _OnSceneUnloaded(AsyncOperation asyncOperation)
        {
            if (asyncOperation != _UnloadingAsyncOperation)
                return;
            _UnloadingAsyncOperation = null;

            switch (_Status)
            {
                case ESceneStatus.None:
                case ESceneStatus.Loading:
                case ESceneStatus.Loaded:
                case ESceneStatus.Loading2Unloading:
                case ESceneStatus.Unloaded:
                case ESceneStatus.Failed:
                    //Error
                    break;

                case ESceneStatus.Unloading:
                    _Status = ESceneStatus.Unloaded;
                    break;
            }
        }

        private void _OnSceneLoaded(AsyncOperation asyncOperation)
        {
            if (asyncOperation != _LoadingAsyncOperation)
                return;
            _LoadingAsyncOperation = null;

            switch (_Status)
            {
                case ESceneStatus.None:
                    //Error
                    break;

                case ESceneStatus.Loading:
                    {
                        int scene_count = SceneManager.sceneCount;
                        _Scene = SceneManager.GetSceneAt(scene_count - 1);
                        _Status = ESceneStatus.Loaded;
                    }

                    break;

                case ESceneStatus.Loaded:
                    //Error
                    break;

                case ESceneStatus.Unloading:
                    //Error
                    break;

                case ESceneStatus.Loading2Unloading:
                    {
                        int scene_count = SceneManager.sceneCount;
                        _Scene = SceneManager.GetSceneAt(scene_count - 1);
                        _UnloadingAsyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_Scene);
                        if (_UnloadingAsyncOperation == null)
                            _Status = ESceneStatus.Unloaded;
                        else
                        {
                            _Status = ESceneStatus.Unloading;
                            _UnloadingAsyncOperation.completed += _OnSceneUnloaded;
                        }
                    }
                    break;

                case ESceneStatus.Unloaded:
                    //Error
                    break;

                case ESceneStatus.Failed:
                    //Error
                    break;
            }
        }

        protected override void OnPoolRelease()
        {
            _SceneRef.Destroy();
            _LoadingAsyncOperation = null;
            _UnloadingAsyncOperation = null;
            _Scene = default;
            _Status = ESceneStatus.None;
            _Id = SceneID.Empty;
        }
    }
}
