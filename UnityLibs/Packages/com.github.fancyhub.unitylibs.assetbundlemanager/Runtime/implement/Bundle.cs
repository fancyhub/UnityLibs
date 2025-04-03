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
        Error,
    }

    internal class Bundle : IBundle
    {
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        public CPtr<IBundleMgr.IExternalLoader> _external_loader;
        public BundleManifest.Item _config;
        public Bundle[] _all_deps;

        private IBundleMgr.ExternalBundle _unity_bundle;

        private EBundleLoadStatus _status = EBundleLoadStatus.None;

        private bool _loaded_by_external_flag = false;
        private int _external_ref_count = 0;

        private bool _loaded_by_dep_flag = false;
        private int _dep_ref_count = 0;

        public string Name { get { return _config.Name; } }

        public int RefCount => _external_ref_count + _dep_ref_count;

        public int IncRef()
        {
            if (_status != EBundleLoadStatus.Loaded || !_loaded_by_external_flag)
            {
                BundleLog.E("Bundle {0}, ({1}, {2}, {3}), is not loaded by external, canot inc ref count", _config.Name, _status, _external_ref_count, _dep_ref_count);
                return RefCount;
            }
            _external_ref_count++;
            BundleLog.D("Bundle {0}, ({1}, {2}, {3}) Inc -> ({4},{3})", _config.Name, _status, _external_ref_count - 1, _dep_ref_count, _external_ref_count);
            return RefCount;
        }

        public int DecRef()
        {
            if (_status != EBundleLoadStatus.Loaded || !_loaded_by_external_flag)
            {
                BundleLog.E("Bundle {0}, ({1}, {2}, {3}), is not loaded by external, canot dec ref count", _config.Name, _status, _external_ref_count, _dep_ref_count);
                return RefCount;
            }

            _external_ref_count--;
            BundleLog.D("Bundle {0}, ({1}, {2}, {3}) Dec -> ({4},{3})", _config.Name, _status, _external_ref_count + 1, _dep_ref_count, _external_ref_count);

            if (_external_ref_count > 0)
                return RefCount;
            _external_ref_count = 0;
            _loaded_by_external_flag = false;

            if (_dep_ref_count > 0)
                return RefCount;

            _UnloadUnityBundle();
            return RefCount;
        }

        public UnityEngine.Object LoadAsset(string path, Type unityAssetType)
        {
            if (_status != EBundleLoadStatus.Loaded || !_loaded_by_external_flag)
            {
                BundleLog.E("Bundle {0}, ({1}, {2}, {3}), is not loaded by external, canot load asset, {4}", _config.Name, _status, _external_ref_count, _dep_ref_count, path);
                return null;
            }
            if (_unity_bundle == null)
            {
                BundleLog.E("Bundle {0} AssetBundle Is Null ", _config.Name);
                return null;
            }

            var ret = _unity_bundle.LoadAsset(path, unityAssetType);
            BundleLog.D("Bundle {0}, ({1}, {2}, {3}), load asset, {4}, {5} ", _config.Name, _status, _external_ref_count, _dep_ref_count, path, ret != null);
            return ret;
        }

        public AssetBundleRequest LoadAssetAsync(string path, Type unityAssetType)
        {
            if (_status != EBundleLoadStatus.Loaded || !_loaded_by_external_flag)
            {
                BundleLog.E("Bundle {0}, ({1}, {2}, {3}), is not loaded by external, canot load asset, {4}", _config.Name, _status, _external_ref_count, _dep_ref_count, path);
                return null;
            }
            if (_unity_bundle == null)
            {
                BundleLog.E("Bundle {0} AssetBundle Is Null ", _config.Name);
                return null;
            }

            var ret = _unity_bundle.LoadAssetAsync(path, unityAssetType);
            BundleLog.D("Bundle {0}, ({1}, {2}, {3}), load asset async, {4}, {5} ", _config.Name, _status, _external_ref_count, _dep_ref_count, path, ret != null);
            return ret;
        }

        public bool LoadByExternal()
        {
            switch (_status)
            {
                default:
                    return false;

                case EBundleLoadStatus.Loaded:
                    _loaded_by_external_flag = true;
                    return true;

                case EBundleLoadStatus.Error:
                    return false;

                case EBundleLoadStatus.None:
                    BundleLog.D("Load Bundle {0} Load", _config.Name);
                    IBundleMgr.IExternalLoader loader = _external_loader.Val;
                    if (loader == null)
                    {
                        BundleLog.E("BundleLoader is null");
                        return false;
                    }

                    EBundleFileStatus bundleStatus = loader.GetBundleFileStatus(_config.Name);
                    if (bundleStatus != EBundleFileStatus.Ready)
                    {
                        BundleLog.Assert(false, "Bundle {0} canot be loaded, file status: {1}", _config.Name, bundleStatus);
                        return false;
                    }

                    if (!_LoadAllDeps())
                        return false;

                    _unity_bundle = loader.LoadBundleFile(_config.Name);

                    if (_unity_bundle == null)
                    {
                        _status = EBundleLoadStatus.Error;
                        foreach (var p in _all_deps)
                            p._UnloadByDep(_config.Name);
                        return false;
                    }
                    else
                    {
                        BundleLog.D("Bundle {0} Load Succ", _config.Name);
                        _status = EBundleLoadStatus.Loaded;
                        _loaded_by_external_flag = true;
                        return true;
                    }
            }
        }

        public void Destroy()
        {
            ___obj_ver++;
            _UnloadUnityBundle(true);
        }

        private bool _LoadAllDeps()
        {
            //1. 先检查是否都下载了
            foreach (var p in _all_deps)
            {
                if (!p._IsDownloaded())
                {
                    BundleLog.E("Bundle {0}, ({1}, {2}, {3}) 加载失败, 因为有依赖的bundle没有下载 {4}", _config.Name, _status, _external_ref_count, _dep_ref_count, p.Name);
                    return false;
                }
            }

            //2. 加载
            for (int i = 0; i < _all_deps.Length; i++)
            {
                if (_all_deps[i]._LoadByDep(_config.Name))
                    continue;

                for (int j = i - 1; j >= 0; j--)
                {
                    _all_deps[j]._UnloadByDep(_config.Name);
                }

                return false;
            }

            return true;
        }

        private bool _LoadByDep(string parent_bundle_name)
        {
            switch (_status)
            {
                case EBundleLoadStatus.None:
                    BundleLog.D("Load Dep Bundle {0} Load", _config.Name);

                    IBundleMgr.IExternalLoader loader = _external_loader.Val;
                    if (loader == null)
                    {
                        BundleLog.E("BundleLoader is null");
                        return false;
                    }

                    _unity_bundle = loader.LoadBundleFile(_config.Name);
                    if (_unity_bundle == null)
                    {
                        BundleLog.E("Bundle {0} Load Dep Fail", _config.Name);
                        _status = EBundleLoadStatus.Error;
                        return false;
                    }

                    BundleLog.D("Bundle {0} Load Dep Succ", _config.Name);
                    _status = EBundleLoadStatus.Loaded;
                    _loaded_by_dep_flag = true;
                    _dep_ref_count = 1;
                    BundleLog.D("Bundle {0}, ({1}, {2}, {3}) Inc Dep -> ({2},{4})", _config.Name, _status, _external_ref_count, _dep_ref_count - 1, _dep_ref_count);
                    return true;

                case EBundleLoadStatus.Loaded:
                    _loaded_by_dep_flag = true;
                    _dep_ref_count++;
                    BundleLog.D("Bundle {0}, ({1}, {2}, {3}) Inc Dep -> ({2},{4})", _config.Name, _status, _external_ref_count, _dep_ref_count - 1, _dep_ref_count);
                    return true;

                case EBundleLoadStatus.Error:
                    return false;

                default:
                    return false;
            }
        }

        private void _UnloadByDep(string parent_bundle_name)
        {
            if (_status != EBundleLoadStatus.Loaded || !_loaded_by_dep_flag)
            {
                BundleLog.E("Bundle {0}, ({1}, {2}, {3}), is not loaded by dep, canot dec dep ref count", _config.Name, _status, _external_ref_count, _dep_ref_count);
                return;
            }

            _dep_ref_count--;
            BundleLog.D("Bundle {0}, ({1}, {2}, {3}) Dec Dep -> ({2},{4})", _config.Name, _status, _external_ref_count, _dep_ref_count + 1, _dep_ref_count);
            if (_dep_ref_count > 0)
                return;
            _loaded_by_dep_flag = false;
            if (_external_ref_count > 0)
                return;
            _UnloadUnityBundle();
        }

        private void _UnloadUnityBundle(bool destroyed = false)
        {
            if (destroyed)
            {
                BundleLog.D("Bundle {0}, ({1}, {2}, {3}) unload unity bundle by destroy", _config.Name, _status, _external_ref_count, _dep_ref_count);
            }
            else
            {
                if (_status != EBundleLoadStatus.Loaded || _unity_bundle == null)
                    BundleLog.E("Bundle {0}, ({1}, {2}, {3}) unload unity bundle", _config.Name, _status, _external_ref_count, _dep_ref_count);
                else
                    BundleLog.D("Bundle {0}, ({1}, {2}, {3}) unload unity bundle", _config.Name, _status, _external_ref_count, _dep_ref_count);
            }

            _status = EBundleLoadStatus.None;
            _external_ref_count = 0;
            _dep_ref_count = 0;
            _loaded_by_dep_flag = false;
            _loaded_by_external_flag = false;

            if (_unity_bundle != null)
            {
                var t = _unity_bundle;
                _unity_bundle = null;
                t.Unload(BundleDef.UnloadAllLoadedObjectsCurrent);
            }
        }

        private bool _IsDownloaded()
        {
            if (_status != EBundleLoadStatus.None)
                return true;

            IBundleMgr.IExternalLoader loader = _external_loader.Val;
            if (loader == null)
                return false;

            return loader.GetBundleFileStatus(_config.Name) == EBundleFileStatus.Ready;
        }
    }
}
