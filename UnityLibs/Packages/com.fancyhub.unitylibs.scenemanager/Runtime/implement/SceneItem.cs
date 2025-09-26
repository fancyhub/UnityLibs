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

    internal class SceneItem : CPoolItemBase, IScene
    {
        private static System.Collections.Generic.List<GameObject> _STemp = new();
        private static int _SceneIdGen = 1;

        private string _ScenePath;
        private LoadSceneParameters _LoadParam;
        private ESceneStatus _Status;
        private AsyncOperation _LoadingAsyncOperation;
        private AsyncOperation _UnloadingAsyncOperation;
        private CPtr<ISceneMgr.IExternalRef> _SceneRef;
        private ScenePlaceHolderItem _PlaceHolderItem;

        private bool _NeedSceneRoot = false;
        private Transform _SceneRoot;
        private Vector3 _ScenePos;
        private bool _SceneVisible;
        private int _SceneId;
        private Scene _UnityScene;

        public int SceneId => _SceneId;

        public ESceneStatus Status
        {
            get { return _Status; }
            private set
            {
                if (_Status == value)
                    return;

                SceneLog._.D("{0}:{1},{2}, Status {3} -> {4}", _SceneId, _ScenePath, UnityScenePath, _Status, value);
                _Status = value;
            }
        }




        public static SceneItem Create(ScenePlaceHolderItem placeHolderItem, ISceneMgr.IExternalRef scene_ref, string scene_path, LoadSceneParameters load_param)
        {
            SceneItem ret = new SceneItem();
            ret._SceneId = _SceneIdGen++;
            ret._ScenePath = scene_path;
            ret._LoadParam = load_param;
            ret.Status = ESceneStatus.None;
            ret._SceneRef = new CPtr<ISceneMgr.IExternalRef>(scene_ref);
            ret._PlaceHolderItem = placeHolderItem;
            ret._NeedSceneRoot = false;
            ret._ScenePos = Vector3.zero;
            ret._SceneRoot = null;
            ret._SceneVisible = true;
            return ret;
        }

        public bool IsLoading()
        {
            return Status == ESceneStatus.Loading || Status == ESceneStatus.Loading2Unloading;
        }

        public bool ShouldBeDestroyed()
        {
            return Status == ESceneStatus.Failed || Status == ESceneStatus.Unloaded;
        }

        public bool IsSingleLoadMode()
        {
            return _LoadParam.loadSceneMode == LoadSceneMode.Single;
        }

        public LoadSceneMode GetLoadMode()
        {
            return _LoadParam.loadSceneMode;
        }

        public (bool done, float progress) Stat
        {
            get
            {
                switch (Status)
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
        }

        public Transform SceneRoot => _SceneRoot;

        public Transform CreateSceneRoot()
        {
            if (_SceneRoot != null)
                return _SceneRoot;
            _NeedSceneRoot = true;
            if (Status == ESceneStatus.Loaded)
                _CreateSceneRoot();
            return _SceneRoot;
        }

        public Vector3 ScenePos
        {
            get
            {
                return _ScenePos;
            }
            set
            {
                if (_ScenePos.Equals(value))
                    return;

                _ScenePos = value;
                _NeedSceneRoot = true;

                if (_SceneRoot != null)
                {
                    _SceneRoot.localPosition = value;
                }
                else if (Status == ESceneStatus.Loaded)
                {
                    _CreateSceneRoot();
                }
            }
        }

        public bool SceneVisible
        {
            get
            {
                return _SceneVisible;
            }

            set
            {
                if (_SceneVisible == value)
                    return;
                _SceneVisible = value;
                _NeedSceneRoot = true;

                if (_SceneRoot != null)
                {
                    _SceneRoot.ExtSetGameObjectActive(value);
                }
                else if (Status == ESceneStatus.Loaded)
                {
                    _CreateSceneRoot();
                }
            }
        }

        public void BeginLoad()
        {
            switch (Status)
            {
                case ESceneStatus.None:
                    _LoadingAsyncOperation = _SceneRef.Val.Load(_LoadParam);
                    if (_LoadingAsyncOperation == null)
                    {
                        SceneLog._.E("Scene:{0} load failed, Mode: {1}, Path: {2}", SceneId, _LoadParam.loadSceneMode, _ScenePath);
                        Status = ESceneStatus.Failed;
                    }
                    else
                    {
                        SceneLog._.D("Scene:{0} begin loading, Mode: {1}, Path: {2}", SceneId, _LoadParam.loadSceneMode, _ScenePath);
                        _LoadingAsyncOperation.completed += _OnSceneLoaded;
                        Status = ESceneStatus.Loading;
                    }
                    break;

                case ESceneStatus.Loading:
                case ESceneStatus.Loaded:
                case ESceneStatus.Unloading:
                case ESceneStatus.Loading2Unloading:
                case ESceneStatus.Unloaded:
                case ESceneStatus.Failed:
                    SceneLog._.D("Scene:{0} cant load because status is incorrect, Mode: {1}, Path: {2}, Status:{3}", SceneId, _LoadParam.loadSceneMode, _ScenePath, Status);
                    break;

                default:
                    SceneLog._.Assert(false, "unkown status {0}", Status);
                    break;
            }
        }

        public Scene UnityScene => _UnityScene;

        public string UnityScenePath => _UnityScene.IsValid() ? _UnityScene.path : null;

        public bool Valid
        {
            get
            {
                switch (Status)
                {
                    default:
                        return false;

                    case ESceneStatus.None:
                        return true;

                    case ESceneStatus.Loading:
                        return true;

                    case ESceneStatus.Loaded:
                        return _UnityScene.IsValid();

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
        }

        public void Unload()
        {
            SceneLog._.D("{0}:{1},{2}, begin unload scene async, {3} ", SceneId, _ScenePath, UnityScenePath, Status);
            switch (Status)
            {
                case ESceneStatus.None:
                    Status = ESceneStatus.Unloaded;
                    break;

                case ESceneStatus.Loading:
                    Status = ESceneStatus.Loading2Unloading;
                    break;

                case ESceneStatus.Loaded:
                    if (!_UnityScene.IsValid())
                    {
                        SceneLog._.D("{0}:{1},{2},  unload, scene is invalid", SceneId, _ScenePath, UnityScenePath);
                        Status = ESceneStatus.Unloaded;
                    }
                    else
                    {
                        _PlaceHolderItem.CreateForUnload();
                        _UnloadingAsyncOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_UnityScene);
                        if (_UnloadingAsyncOperation == null)
                        {
                            SceneLog._.D("{0}:{1},{2},  unload async operation is null", SceneId, _ScenePath, UnityScenePath);
                            Status = ESceneStatus.Unloaded;
                        }
                        else
                        {
                            Status = ESceneStatus.Unloading;
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
                    Status = ESceneStatus.Unloaded;
                    break;

                default:
                    SceneLog._.Assert(false, "unkown status {0}", Status);
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

            switch (Status)
            {
                case ESceneStatus.None:
                case ESceneStatus.Loading:
                case ESceneStatus.Loaded:
                case ESceneStatus.Loading2Unloading:
                case ESceneStatus.Unloaded:
                case ESceneStatus.Failed:
                default:
                    SceneLog._.E("Scene:{0} on scene unloaded,{1}, Error status: {2}", SceneId, _ScenePath, Status);
                    break;

                case ESceneStatus.Unloading:
                    Status = ESceneStatus.Unloaded;
                    break;
            }
        }

        private void _OnSceneLoaded(AsyncOperation asyncOperation)
        {
            if (asyncOperation != _LoadingAsyncOperation)
            {
                SceneLog._.E("{0}:{1},{2},  on scene loaded, has error", SceneId, _ScenePath, UnityScenePath);
                return;
            }
            _LoadingAsyncOperation = null;

            switch (Status)
            {
                case ESceneStatus.Loading:
                    {
                        int scene_count = SceneManager.sceneCount;
                        _UnityScene = SceneManager.GetSceneAt(scene_count - 1);
                        Status = ESceneStatus.Loaded;
                        SceneLog._.Assert(_UnityScene.IsValid() && _UnityScene.path == _ScenePath, "{0}:{1},{2}, loaded has error", SceneId, _ScenePath, UnityScenePath);
                        _PlaceHolderItem.CheckForActive();
                        if (_NeedSceneRoot)
                            _CreateSceneRoot();
                    }
                    break;

                case ESceneStatus.Loading2Unloading:
                    {
                        int scene_count = SceneManager.sceneCount;
                        _UnityScene = SceneManager.GetSceneAt(scene_count - 1);

                        SceneLog._.Assert(_UnityScene.IsValid() && _UnityScene.path == _ScenePath, "Scene:{0}, loaded has error, Valid:{1}, {2} : {3}", _UnityScene.IsValid(), _UnityScene.path, _ScenePath);

                        if (_UnityScene.IsValid())
                        {
                            _PlaceHolderItem.CreateForUnload();
                            SceneLog._.D("Scene:{0},Status: {1}, begin unload {2}", SceneId, Status, _ScenePath);
                            _UnloadingAsyncOperation = SceneManager.UnloadSceneAsync(_UnityScene);
                            if (_UnloadingAsyncOperation == null)
                            {
                                Status = ESceneStatus.Unloaded;
                                SceneLog._.E("Scene:{0} unload failed, Status: {1} -> {2}, {3} ", SceneId, ESceneStatus.Loading2Unloading, ESceneStatus.Unloaded, _ScenePath);
                            }
                            else
                            {
                                SceneLog._.E("Scene:{0} begin unloading, Status: {1} -> {2},{3} ", SceneId, ESceneStatus.Loading2Unloading, ESceneStatus.Unloading, _ScenePath);
                                Status = ESceneStatus.Unloading;
                                _UnloadingAsyncOperation.completed += _OnSceneUnloaded;
                            }
                        }
                        else
                        {
                            Status = ESceneStatus.Unloaded;
                        }
                    }
                    break;

                case ESceneStatus.None:
                case ESceneStatus.Loaded:
                case ESceneStatus.Unloading:
                case ESceneStatus.Unloaded:
                case ESceneStatus.Failed:
                default:
                    SceneLog._.E("Scene:{0} on scene loaded,{1}, Error status: {2}", SceneId, _ScenePath, Status);
                    break;
            }
        }

        protected override void OnPoolRelease()
        {
            _SceneRef.Destroy();
            _LoadingAsyncOperation = null;
            _UnloadingAsyncOperation = null;
            _UnityScene = default;
            Status = ESceneStatus.None;
            _SceneId = 0;
            _PlaceHolderItem = null;
            _SceneRoot = null;
        }


        private void _CreateSceneRoot()
        {
            if (!_UnityScene.IsValid() || _SceneRoot != null)
                return;

            _STemp.Clear();
            _UnityScene.GetRootGameObjects(_STemp);
            string root_name = "SceneRoot";

            if (Application.isEditor)
                root_name += SceneId;

            GameObject obj = new GameObject(root_name);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(obj, _UnityScene);
            _SceneRoot = obj.transform;

            _SceneRoot.localPosition = _ScenePos;
            _SceneRoot.ExtSetGameObjectActive(_SceneVisible);

            foreach (var p in _STemp)
                p.transform.SetParent(_SceneRoot, false);
        }
    }
}
