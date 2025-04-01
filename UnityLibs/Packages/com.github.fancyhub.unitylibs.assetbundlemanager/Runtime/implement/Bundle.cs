/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.ABManagement
{
    internal static class BundleDef
    {
        public static bool UnloadAllLoadedObjectsDefault = true;

        public static bool UnloadAllLoadedObjectsCurrent = UnloadAllLoadedObjectsDefault;
    }

    internal enum EBundleLoadStatus
    {
        None,
        Loaded,
        LoadedByDep, //因为依赖的原因
        Error,
    }

    internal class Bundle : IBundle
    {
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        public CPtr<IBundleMgr.IExternalLoader> _ExternalLoader;
        public BundleManifest.Item _Config;
        public Bundle[] _AllDeps;

        private IBundleMgr.ExternalBundle _AssetBundle;

        private EBundleLoadStatus _LoadStatus = EBundleLoadStatus.None;
        private int _RefCount = 0;
        private int _DepRefCount = 0;

        public string Name { get { return _Config.Name; } }

        public void GetAllDeps(List<Bundle> deps)
        {
            deps.AddRange(_AllDeps);
        }

        public bool IsDownloaded()
        {
            if (_LoadStatus != EBundleLoadStatus.None)
                return true;

            IBundleMgr.IExternalLoader loader = _ExternalLoader.Val;
            if (loader == null)
                return false;

            return loader.GetBundleFileStatus(_Config.Name) == EBundleFileStatus.Ready;
        }

        public int IncRef()
        {
            if (_LoadStatus != EBundleLoadStatus.Loaded)
            {
                BundleLog.E("Bundle {0}:{1} Is Not Load, Can't Inc ref count", _Config.Name, _LoadStatus);
                return 0;
            }
            _RefCount++;
            BundleLog.D("Bundle {0} RefCount {1} -> {2}", _Config.Name, _RefCount - 1, _RefCount);
            return _RefCount;
        }

        public int RefCount => _RefCount;

        public int DecRef()
        {
            if (_LoadStatus != EBundleLoadStatus.Loaded)
            {
                BundleLog.E("Bundle {0}:{1} Is Not Load, Can't Dec ref count", _Config.Name, _LoadStatus);
                return 0;
            }

            BundleLog.D("Bundle {0} RefCount {1} -> {2}", _Config.Name, _RefCount, _RefCount - 1);
            _RefCount--;
            if (_RefCount > 0)
                return _RefCount;
            _RefCount = 0;

            if (_DepRefCount > 0)
            {
                BundleLog.D("Bundle {0} DepRefCount {1}>0, Don't Need Unload", _Config.Name, _DepRefCount);
                _LoadStatus = EBundleLoadStatus.LoadedByDep;
                return _RefCount;
            }

            _LoadStatus = EBundleLoadStatus.None;
            _Unload();
            return _RefCount;
        }

        public UnityEngine.Object LoadAsset(string path, Type unityAssetType)
        {
            if (_LoadStatus != EBundleLoadStatus.Loaded)
            {
                BundleLog.E("Bundle {0} Is Not Loaded {1}, Load Asset Fail {2}  ", _Config.Name, _LoadStatus, path);
                return null;
            }
            if (_AssetBundle == null)
            {
                BundleLog.E("Bundle {0} AssetBundle Is Null ", _Config.Name);
                return null;
            }

            return _AssetBundle.LoadAsset(path, unityAssetType);
        }

        public AssetBundleRequest LoadAssetAsync(string path, Type unityAssetType)
        {
            if (_LoadStatus != EBundleLoadStatus.Loaded)
            {
                BundleLog.E("Bundle {0} Is Not Loaded {1}, Load Asset Fail {2}  ", _Config.Name, _LoadStatus, path);
                return null;
            }
            if (_AssetBundle == null)
            {
                BundleLog.E("Bundle {0} AssetBundle Is Null ", _Config.Name);
                return null;
            }

            return _AssetBundle.LoadAssetAsync(path, unityAssetType);
        }

        public void Destroy()
        {
            ___obj_ver++;
            _Unload();
        }         

        private void _Unload()
        {
            if (_AssetBundle == null)
                return;
            BundleLog.D("Bundle {0} Unload ", _Config.Name);
            _AssetBundle.Unload(BundleDef.UnloadAllLoadedObjectsCurrent);
            _AssetBundle = null;
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
                    BundleLog.D("Load Bundle {0} Load", _Config.Name);
                    IBundleMgr.IExternalLoader loader = _ExternalLoader.Val;
                    if (loader == null)
                    {
                        BundleLog.E("BundleLoader is null");
                        return false;
                    }

                    EBundleFileStatus bundleStatus = loader.GetBundleFileStatus(_Config.Name);
                    if (bundleStatus != EBundleFileStatus.Ready)
                    {
                        BundleLog.Assert(false, "Bundle {0} Is {1}", _Config.Name, bundleStatus);
                        return false;
                    }

                    foreach (var p in _AllDeps)
                    {
                        switch (p._LoadStatus)
                        {
                            case EBundleLoadStatus.Loaded:
                            case EBundleLoadStatus.LoadedByDep:
                                continue;
                            case EBundleLoadStatus.Error:
                                _LoadStatus = EBundleLoadStatus.Error;
                                return false;

                            case EBundleLoadStatus.None:
                                if (!p.IsDownloaded())
                                {
                                    BundleLog.Assert(false, "Bundle {0} Dep {1} Is not Downloaded", _Config.Name, p._Config.Name);
                                    return false;
                                }
                                break;
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

                    _AssetBundle = loader.LoadBundleFile(_Config.Name);

                    if (_AssetBundle != null)
                    {
                        BundleLog.D("Bundle {0} Load Succ", _Config.Name);
                        _LoadStatus = EBundleLoadStatus.Loaded;
                        return true;
                    }

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
                    BundleLog.D("Load Dep Bundle {0} Load", _Config.Name);

                    IBundleMgr.IExternalLoader loader = _ExternalLoader.Val;
                    if (loader == null)
                    {
                        BundleLog.E("BundleLoader is null");
                        return false;
                    }

                    EBundleFileStatus bundleStatus = loader.GetBundleFileStatus(_Config.Name);
                    if (bundleStatus != EBundleFileStatus.Ready)
                    {
                        BundleLog.D("Bundle {0} Is {1}", _Config.Name, bundleStatus);
                        return false;
                    }

                    _AssetBundle = loader.LoadBundleFile(_Config.Name);

                    if (_AssetBundle == null)
                    {
                        BundleLog.E("Bundle {0} Load Dep Fail", _Config.Name);
                        _LoadStatus = EBundleLoadStatus.Error;
                        return false;
                    }

                    BundleLog.D("Bundle {0} Load Dep Succ", _Config.Name);
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
            
            _LoadStatus = EBundleLoadStatus.None;
            _Unload();
            return;
        }
    }
}
