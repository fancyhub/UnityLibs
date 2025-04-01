/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH.ABManagement;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace FH
{
    public enum EBundleFileStatus
    {
        None, //不存在
        Ready,
        Remote,
    }

    public partial interface IBundleMgr
    {
        public enum EAssetStatus
        {

        }

        public sealed class ExternalBundle
        {
            private AssetBundle _Bundle;
            private Stream _Stream;
            public static ExternalBundle Create(UnityEngine.AssetBundle ab, Stream stream = null)
            {
                if (ab == null)
                    return null;
                ExternalBundle ret = new ExternalBundle();
                ret._Bundle = ab;
                ret._Stream = stream;
                return ret;
            }

            public static ExternalBundle LoadFromFile(string file_path)
            {
                UnityEngine.AssetBundle ab = UnityEngine.AssetBundle.LoadFromFile(file_path);
                if (ab == null)
                    return null;
                ExternalBundle ret = new ExternalBundle();
                ret._Bundle = ab;
                ret._Stream = null;
                return ret;
            }

            public static ExternalBundle LoadFromStream(Stream stream)
            {
                UnityEngine.AssetBundle ab = UnityEngine.AssetBundle.LoadFromStream(stream);
                if (ab == null)
                    return null;
                ExternalBundle ret = new ExternalBundle();
                ret._Bundle = ab;
                ret._Stream = stream;
                return ret;
            }

            public AssetBundle Bundle => _Bundle;

            public T LoadAsset<T>(string name) where T : UnityEngine.Object
            {
                return _Bundle.LoadAsset<T>(name);
            }

            public UnityEngine.Object LoadAsset(string name, Type unityAssetType)
            {
                return _Bundle.LoadAsset(name, unityAssetType);
            }

            public AssetBundleRequest LoadAssetAsync<T>(string name) where T : UnityEngine.Object
            {
                return _Bundle.LoadAssetAsync<T>(name);
            }

            public AssetBundleRequest LoadAssetAsync(string name, Type unityAssetType)
            {
                return _Bundle.LoadAssetAsync(name, unityAssetType);
            }

            public void Unload(bool unloadAllLoadedObjects)
            {
                if (_Bundle != null)
                {
                    var t = _Bundle;
                    _Bundle = null;
                    t.Unload(unloadAllLoadedObjects);
                }

                if (_Stream != null)
                {
                    var t = _Stream;
                    _Stream = null;
                    t.Close();
                }
            }
        }

        public interface IExternalLoader : ICPtr
        {
            /// <summary>
            /// 如果返回null, 使用 AssetBundle.LoadFromFile
            /// 如果返回正确的值, 使用 AssetBundle.LoadFromStream
            /// </summary>
            public ExternalBundle LoadBundleFile(string name);

            public EBundleFileStatus GetBundleFileStatus(string name);

            public BundleManifest LoadManifest();
        }
    }

    public interface IBundle : ISPtr
    {
        public string Name { get; }

        public UnityEngine.Object LoadAsset(string path, Type unityAssetType);
        public AssetBundleRequest LoadAssetAsync(string path, Type unityAssetType);
    }

    public struct BundleInfo
    {
        public readonly string Name;
        public readonly EBundleFileStatus Status;

        public BundleInfo(string name, EBundleFileStatus status)
        {
            this.Name = name;
            this.Status = status;
        }
    }

    public partial interface IBundleMgr : ICPtr
    {
        public IBundle LoadBundleByAsset(string asset);

        public void GetBundleInfoByAssets(List<string> asset_list, List<BundleInfo> out_bundle_info_list);

        public BundleInfo GetBundleInfoByAsset(string asset);

        public void Upgrade();
    }

    public static class BundleMgr
    {
        private static CPtr<IBundleMgr> _;

        public static IBundleMgr Inst { get { return _.Val; } }

        public static bool InitMgr(IBundleMgr.Config config, IBundleMgr.IExternalLoader external_loader, bool disable_in_editor)
        {
            if (config == null)
            {
                BundleLog.E("BundleMgrConfig is Null");
                return false;
            }

            if (_.Val != null)
            {
                BundleLog.E("BundleMgr 已经创建了");
                return false;
            }

            BundleLog.SetMasks(config.LogLvl);

            if (disable_in_editor && Application.isEditor)
            {
                _ = new ABManagement.BundleMgrImplementEmpty();
                return true;
            }

            if (external_loader == null)
            {
                BundleLog.E("BundlerLoader is Null");
                return false;
            }
            var manifest = external_loader.LoadManifest();
            if (manifest == null)
            {
                BundleLog.E("bundle manifest is Null");
                return false;
            }

            BundleDef.UnloadAllLoadedObjectsDefault = config.UnloadAllLoadedObjects;
            BundleDef.UnloadAllLoadedObjectsCurrent = config.UnloadAllLoadedObjects;

            ABManagement.BundleMgrImplement mgr = new ABManagement.BundleMgrImplement();
            mgr.Init(external_loader, manifest);
            _ = mgr;
            return true;
        }

        public static void Destroy()
        {
            _.Destroy();
        }

        public static IBundle LoadBundleByAsset(string asset)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                BundleLog.E("BundleMgr is null");
                return null;
            }

            return mgr.LoadBundleByAsset(asset);
        }

        private static List<BundleInfo> _S_TempList = new List<BundleInfo>();

        public static void GetAllNeedDownload(List<string> asset_list, List<string> out_bundle_names_list)
        {
            out_bundle_names_list.Clear();
            var mgr = _.Val;
            if (mgr == null)
            {
                BundleLog.E("BundleMgr is null");
                return;
            }

            mgr.GetBundleInfoByAssets(asset_list, _S_TempList);

            foreach (var p in _S_TempList)
            {
                if (p.Status == EBundleFileStatus.Remote)
                {
                    out_bundle_names_list.Add(p.Name);
                }
            }
        }
    }
}
