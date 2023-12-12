using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public interface IFileSystem
    {
        public string GetFilePathByName(string name);
        public string IsExistByName(string name);
        public string IsExistByFilePath(string filePath);
    }

    public class ABMgr
    {
        public IFileSystem _FileSystem;
        public List<Bundle> _BundleList;
        public MyDict<string, Bundle> _AssetDict;
        public MyDict<string, Bundle> _SceneDict;

        public ABMgrConfig _Config;
        public void Init(IFileSystem fileSystem, ABMgrConfig config)
        {
            _FileSystem = fileSystem;
            _Config = config;

            _BundleList = new List<Bundle>(config.BundleList.Length);
            _AssetDict = new MyDict<string, Bundle>();

            foreach (var p in config.BundleList)
            {
                Bundle b = new Bundle();
                b._Config = p;
                b._FileSystem = fileSystem;
                _BundleList.Add(b);

                foreach (var a in p.GetAssets())
                {
                    _AssetDict.Add(a, b);
                }

                foreach (var s in p.GetScenes())
                {
                    _SceneDict.Add(s, b);
                }
            }


            List<int> tempList = new List<int>();
            for (int i = 0; i < config.BundleList.Length; i++)
            {
                config.GetAllDeps(i, tempList);

                _BundleList[i]._AllDeps = new Bundle[tempList.Count];
                for (int j = 0; j < tempList.Count; j++)
                {
                    _BundleList[i]._AllDeps[j] = _BundleList[tempList[j]];
                }
            }
        }

        public IBundle LoadBundleByAsset(string asset)
        {
            _AssetDict.TryGetValue(asset, out Bundle b);
            if (b == null)
                return null;

            if (!b.Load())
                return null;

            b.IncRef();
            return b;
        }

        public IBundle LoadBundleByScene(string scene)
        {
            _SceneDict.TryGetValue(scene, out Bundle b);
            if (b == null)
                return null;
            if (!b.Load())
                return null;

            b.IncRef();
            return b;
        }
    }

    [Serializable]
    public class ABMgrConfig
    {
        private static HashSet<int> S_TempSet = new HashSet<int>();

        public BundleConfig[] BundleList;

        [Serializable]
        public class BundleConfig
        {
            public string Name;
            public int[] Deps;
            public string[] Assets;
            public string[] Scenes;

            public int[] GetDeps() { return Deps == null ? System.Array.Empty<int>() : Deps; }
            public string[] GetAssets() { return Assets == null ? System.Array.Empty<string>() : Assets; }
            public string[] GetScenes() { return Scenes == null ? System.Array.Empty<string>() : Scenes; }
        }

        public void GetAllDeps(int index, List<int> out_list)
        {
            out_list.Clear();
            S_TempSet.Clear();

            foreach (var p in BundleList[index].GetDeps())
            {
                if (S_TempSet.Add(p))
                    out_list.Add(p);
            }

            if (out_list.Count == 0)
                return;

            int it_index = 0;
            for (; ; )
            {
                if (it_index >= out_list.Count)
                    return;
                index = out_list[it_index];
                it_index++;

                foreach (var p in BundleList[index].GetDeps())
                {
                    if (S_TempSet.Add(p))
                        out_list.Add(p);
                }
            }
        }
    }

    internal enum EBundleLoadStatus
    {
        None,
        Loaded,
        LoadedByDep, //因为依赖的原因
        Error,
    }

    public enum EBundleFileStatus
    {
        None,
        Exist,
        Downloading,
    }

    public interface IBundle
    {
        public int IncRef();
        public int DecRef();

        public T LoadAsset<T>(string path) where T : UnityEngine.Object;
        public AssetBundleRequest LoadAssetAsync<T>(string path) where T : UnityEngine.Object;
    }


    public class Bundle : IBundle
    {
        internal IFileSystem _FileSystem;
        internal ABMgrConfig.BundleConfig _Config;
        internal string _Path;
        internal AssetBundle _AssetBundle;
        internal Bundle[] _AllDeps;

        private EBundleLoadStatus _LoadStatus = EBundleLoadStatus.None;
        private EBundleFileStatus _FileStatus;
        private int _RefCount = 0;
        private int _DepRefCount = 0;

        public bool IsDownloaded()
        {
            if (!string.IsNullOrEmpty(_Path))
                return true;
            return !string.IsNullOrEmpty(_Path);
        }

        public bool IsLoaded()
        {
            return _LoadStatus == EBundleLoadStatus.Loaded;
        }

        public int IncRef()
        {
            if (_LoadStatus != EBundleLoadStatus.Loaded)
                return 0;
            _RefCount++;
            return _RefCount;
        }

        public void Dispose()
        {
            _AssetBundle?.Unload(false);
        }

        public int DecRef()
        {
            if (_LoadStatus != EBundleLoadStatus.Loaded)
                return 0;

            _RefCount--;
            if (_RefCount > 0)
                return _RefCount;
            _RefCount = 0;

            if (_DepRefCount > 0)
            {
                _LoadStatus = EBundleLoadStatus.LoadedByDep;
                return _RefCount;
            }

            _LoadStatus = EBundleLoadStatus.None;
            this._AssetBundle.Unload(false);
            return _RefCount;
        }


        public bool CheckExist()
        {
            _Path = _FileSystem.GetFilePathByName(_Config.Name);
            return !string.IsNullOrEmpty(_Path);
        }

        public T LoadAsset<T>(string path) where T : UnityEngine.Object
        {
            return _AssetBundle.LoadAsset<T>(path);
        }

        public AssetBundleRequest LoadAssetAsync<T>(string path) where T : UnityEngine.Object
        {
            return _AssetBundle.LoadAssetAsync<T>(path);
        }

        internal bool Load()
        {
            switch (_LoadStatus)
            {
                default:
                    return false;

                case EBundleLoadStatus.Loaded:
                    return true;

                case EBundleLoadStatus.LoadedByDep:
                    _LoadStatus = EBundleLoadStatus.Loaded;
                    return true;

                case EBundleLoadStatus.Error:
                    return false;

                case EBundleLoadStatus.None:
                    if (_FileStatus != EBundleFileStatus.Exist)
                        return false;

                    foreach (var p in _AllDeps)
                    {
                        if (p._FileStatus != EBundleFileStatus.Exist)
                            return false;

                        if (p._LoadStatus == EBundleLoadStatus.Error)
                        {
                            _LoadStatus = EBundleLoadStatus.Error;
                            return false;
                        }
                    }

                    int index = 0;
                    bool has_error = false;
                    for (; index < _AllDeps.Length; index++)
                    {
                        if (!_AllDeps[index]._LoadDep())
                        {
                            has_error = true;
                            break;
                        }
                        _AllDeps[index]._IncDepRef();
                    }

                    if (has_error)
                    {
                        _LoadStatus = EBundleLoadStatus.Error;
                        for (int i = 0; i < index; i++)
                            _AllDeps[i]._DecDepRef();
                        return false;
                    }

                    _AssetBundle = AssetBundle.LoadFromFile(_Path);
                    if (_AssetBundle != null)
                        return true;

                    _LoadStatus = EBundleLoadStatus.Error;
                    for (int i = 0; i < index; i++)
                        _AllDeps[i]._DecDepRef();
                    return false;
            }
        }

        private bool _LoadDep()
        {
            switch (_LoadStatus)
            {
                case EBundleLoadStatus.None:
                    _AssetBundle = AssetBundle.LoadFromFile(_Path);
                    if (_AssetBundle == null)
                    {
                        _LoadStatus = EBundleLoadStatus.Error;
                        return false;
                    }
                    _LoadStatus = EBundleLoadStatus.LoadedByDep;
                    return true;

                case EBundleLoadStatus.LoadedByDep:
                case EBundleLoadStatus.Loaded:
                    return true;

                case EBundleLoadStatus.Error:
                    return false;

                default: return false;
            }
        }

        private void _IncDepRef()
        {
            _DepRefCount++;
        }

        private void _DecDepRef()
        {
            _DepRefCount--;
            if (_DepRefCount > 0)
                return;

            if (_RefCount > 0)
                return;

            _AssetBundle.Unload(false);
            _LoadStatus = EBundleLoadStatus.None;
            return;
        }
    }
}
