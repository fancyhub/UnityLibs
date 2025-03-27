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

    internal class SceneItem : CPoolItemBase
    {
        private static int _SceneIdGen = 1;
        private string _ScenePath;
        private LoadSceneParameters _LoadParam;
        private ESceneStatus _Status;
        private AsyncOperation _LoadingAsyncOperation;
        private AsyncOperation _UnloadingAsyncOperation;
        private CPtr<ISceneMgr.IExternalRef> _SceneRef;
        private ScenePlaceHolderItem _PlaceHolderItem;

        private Transform _SceneRoot;


        public int SceneId;
        public Scene UnityScene;

        public static SceneItem Create(ScenePlaceHolderItem placeHolderItem, ISceneMgr.IExternalRef scene_ref, string scene_path, LoadSceneParameters load_param)
        {
            SceneItem ret = new SceneItem();
            ret.SceneId = _SceneIdGen++;
            ret._ScenePath = scene_path;
            ret._LoadParam = load_param;
            ret._Status = ESceneStatus.None;
            ret._SceneRef = new CPtr<ISceneMgr.IExternalRef>(scene_ref);
            ret._PlaceHolderItem = placeHolderItem;
            return ret;
        }

        public bool IsLoading()
        {
            return _Status == ESceneStatus.Loading || _Status == ESceneStatus.Loading2Unloading;
        }

        public bool ShouldBeDestroyed()
        {
            return _Status == ESceneStatus.Failed || _Status == ESceneStatus.Unloaded;
        }

        public bool IsSingleLoadMode()
        {
            return _LoadParam.loadSceneMode == LoadSceneMode.Single;
        }

        public LoadSceneMode GetLoadMode()
        {
            return _LoadParam.loadSceneMode;
        }

        public (bool done, float progress) GetSceneStat()
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

        public Vector3 ScenePos
        {
            get
            {
                if (_SceneRoot != null)
                    return _SceneRoot.position;
                return Vector3.zero;
            }
            set
            {

            }
        }
 

        public void BeginLoad()
        {
            switch (_Status)
            {
                case ESceneStatus.None:
                    _LoadingAsyncOperation = _SceneRef.Val.LoadScene(_LoadParam);
                    if (_LoadingAsyncOperation == null)
                    {
                        SceneLog._.E("Scene:{0} load failed, Mode: {1}, Path: {2}", SceneId, _LoadParam.loadSceneMode, _ScenePath);
                        _Status = ESceneStatus.Failed;
                    }
                    else
                    {
                        SceneLog._.D("Scene:{0} begin loading, Mode: {1}, Path: {2}", SceneId, _LoadParam.loadSceneMode, _ScenePath);
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
                    SceneLog._.D("Scene:{0} cant load because status is incorrect, Mode: {1}, Path: {2}, Status:{3}", SceneId, _LoadParam.loadSceneMode, _ScenePath, _Status);
                    break;

                default:
                    SceneLog._.Assert(false, "unkown status {0}", _Status);
                    break;
            }
        }


        public bool IsValid()
        {
            switch (_Status)
            {
                default:
                    return false;

                case ESceneStatus.None:
                    return true;

                case ESceneStatus.Loading:
                    return true;

                case ESceneStatus.Loaded:
                    return UnityScene.IsValid();

                case ESceneStatus.Unloading:
                    return false;

                case ESceneStatus.Loading2Unloading:
                    return false;

                case ESceneStatus.Unloaded:
                    return false;

                case ESceneStatus.Failed:
                    return false;
            }
        }

        public void Unload()
        {
            switch (_Status)
            {
                case ESceneStatus.None:
                    _Status = ESceneStatus.Unloaded;
                    SceneLog._.D("Scene:{0} unload, Status: {1} -> {2}", SceneId, ESceneStatus.None, ESceneStatus.Unloaded);
                    break;

                case ESceneStatus.Loading:
                    _Status = ESceneStatus.Loading2Unloading;
                    break;

                case ESceneStatus.Loaded:
                    if (!UnityScene.IsValid())
                    {
                        _Status = ESceneStatus.Unloaded;
                        SceneLog._.D("Scene:{0} unload, scene is invalid, Status: {1} -> {2}", SceneId, ESceneStatus.Loaded, ESceneStatus.Unloaded);
                    }
                    else
                    {
                        _PlaceHolderItem.CreateForUnload();
                        SceneLog._.D("Scene:{0} bein unload scene async {1}:{2} ", SceneId, UnityScene.path, _ScenePath);
                        _UnloadingAsyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(UnityScene);
                        if (_UnloadingAsyncOperation == null)
                        {
                            _Status = ESceneStatus.Unloaded;
                            SceneLog._.W("Scene:{0} unload async operation is null, Status: {1} -> {2}, {3}, because there is an other scene is in unloading async ", SceneId, ESceneStatus.Loaded, ESceneStatus.Unloaded, _ScenePath);
                        }
                        else
                        {
                            SceneLog._.D("Scene:{0} unloading, Status: {1} -> {2}, {3} ", SceneId, ESceneStatus.Loaded, ESceneStatus.Unloading, _ScenePath);
                            _Status = ESceneStatus.Unloading;
                            _UnloadingAsyncOperation.completed += _OnSceneUnloaded;
                        }
                    }
                    break;

                case ESceneStatus.Unloading:
                case ESceneStatus.Loading2Unloading:
                case ESceneStatus.Unloaded:
                    //Do nothing
                    break;

                case ESceneStatus.Failed:
                    _Status = ESceneStatus.Unloaded;
                    SceneLog._.D("Scene:{0} unload, Status: {1} -> {2}", SceneId, ESceneStatus.Failed, ESceneStatus.Unloaded);
                    break;

                default:
                    SceneLog._.Assert(false, "unkown status {0}", _Status);
                    break;
            }
        }

        private void _OnSceneUnloaded(AsyncOperation asyncOperation)
        {
            if (asyncOperation != _UnloadingAsyncOperation)
            {
                SceneLog._.E("Scene:{0} on scene unloaded,{1}, has error", SceneId, _ScenePath);
                return;
            }
            _PlaceHolderItem.CheckForActive();
            SceneLog._.D("Scene:{0} on scene unloaded,{1}", SceneId, _ScenePath);
            _UnloadingAsyncOperation = null;

            switch (_Status)
            {
                case ESceneStatus.None:
                case ESceneStatus.Loading:
                case ESceneStatus.Loaded:
                case ESceneStatus.Loading2Unloading:
                case ESceneStatus.Unloaded:
                case ESceneStatus.Failed:
                default:
                    SceneLog._.E("Scene:{0} on scene unloaded,{1}, Error status: {2}", SceneId, _ScenePath, _Status);
                    break;

                case ESceneStatus.Unloading:
                    _Status = ESceneStatus.Unloaded;
                    break;
            }
        }

        private void _OnSceneLoaded(AsyncOperation asyncOperation)
        {
            if (asyncOperation != _LoadingAsyncOperation)
            {
                SceneLog._.E("Scene:{0} on scene loaded,{1}, has error", SceneId, _ScenePath);
                return;
            }
            _LoadingAsyncOperation = null;

            switch (_Status)
            {
                case ESceneStatus.Loading:
                    {
                        int scene_count = SceneManager.sceneCount;
                        UnityScene = SceneManager.GetSceneAt(scene_count - 1);
                        _Status = ESceneStatus.Loaded;
                        SceneLog._.D("Scene:{0} on scene loaded,{1}, {2} -> {3}", SceneId, _ScenePath, ESceneStatus.Loading, ESceneStatus.Loaded);

                        SceneLog._.Assert(UnityScene.IsValid() && UnityScene.path == _ScenePath, "Scene:{0}, loaded has error, Valid:{1}, {2} : {3}", UnityScene.IsValid(), UnityScene.path, _ScenePath);
                        _PlaceHolderItem.CheckForActive();
                    }
                    break;

                case ESceneStatus.Loading2Unloading:
                    {
                        int scene_count = SceneManager.sceneCount;
                        UnityScene = SceneManager.GetSceneAt(scene_count - 1);

                        SceneLog._.Assert(UnityScene.IsValid() && UnityScene.path == _ScenePath, "Scene:{0}, loaded has error, Valid:{1}, {2} : {3}", UnityScene.IsValid(), UnityScene.path, _ScenePath);

                        if (UnityScene.IsValid())
                        {
                            _PlaceHolderItem.CreateForUnload();
                            SceneLog._.D("Scene:{0},Status: {1}, begin unload {2}", SceneId, _Status, _ScenePath);
                            _UnloadingAsyncOperation = SceneManager.UnloadSceneAsync(UnityScene);
                            if (_UnloadingAsyncOperation == null)
                            {
                                _Status = ESceneStatus.Unloaded;
                                SceneLog._.E("Scene:{0} unload failed, Status: {1} -> {2}, {3} ", SceneId, ESceneStatus.Loading2Unloading, ESceneStatus.Unloaded, _ScenePath);
                            }
                            else
                            {
                                SceneLog._.E("Scene:{0} begin unloading, Status: {1} -> {2},{3} ", SceneId, ESceneStatus.Loading2Unloading, ESceneStatus.Unloading, _ScenePath);
                                _Status = ESceneStatus.Unloading;
                                _UnloadingAsyncOperation.completed += _OnSceneUnloaded;
                            }
                        }
                        else
                        {
                            _Status = ESceneStatus.Unloaded;
                        }
                    }
                    break;

                case ESceneStatus.None:
                case ESceneStatus.Loaded:
                case ESceneStatus.Unloading:
                case ESceneStatus.Unloaded:
                case ESceneStatus.Failed:
                default:
                    SceneLog._.E("Scene:{0} on scene loaded,{1}, Error status: {2}", SceneId, _ScenePath, _Status);
                    break;
            }
        }

        protected override void OnPoolRelease()
        {
            _SceneRef.Destroy();
            _LoadingAsyncOperation = null;
            _UnloadingAsyncOperation = null;
            UnityScene = default;
            _Status = ESceneStatus.None;
            SceneId = 0;
            _PlaceHolderItem = null;
        }
    }
}
