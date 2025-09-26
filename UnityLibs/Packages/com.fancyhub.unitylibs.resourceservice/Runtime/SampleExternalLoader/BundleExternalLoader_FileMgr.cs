/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FH.SampleExternalLoader
{
    public class BundleExternalLoader_FileMgr : CPtrBase, IBundleMgr.IExternalLoader
    {
        public string _BundleManifestName;
        public CPtr<IFileMgr> _FileMgr;

        public BundleExternalLoader_FileMgr(IFileMgr fileMgr, string manifest_name)
        {
            _FileMgr = new CPtr<IFileMgr>(fileMgr);
            _BundleManifestName = manifest_name;
        }

        public EBundleFileStatus GetBundleFileStatus(string name)
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return EBundleFileStatus.None;
            switch (mgr.FindFile(name, out var _, out var _))
            {
                case EFileStatus.None:
                    return EBundleFileStatus.None;
                case EFileStatus.Ready:
                    return EBundleFileStatus.Ready;
                case EFileStatus.NeedDownload:
                    return EBundleFileStatus.Remote;
                default:
                    return EBundleFileStatus.None;
            }
        }

        public string GetBundleFilePath(string name)
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return null;
            mgr.FindFile(name, out var ret, out var _);
            return ret;
        }

        public IBundleMgr.IExternalRef LoadBundleFile(string name)
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return null;
            mgr.FindFile(name, out var file_path, out var file_location);
            if (file_path == null)
                return null;

            switch (file_location)
            {
                default:
                    return null;
                case EFileLocation.Persistent:
                    {
                        return BundleItem.LoadFromFile(file_path);
                    }

                case EFileLocation.Remote:
                    return null;

                case EFileLocation.StreamingAssets:
                    {
                        var stream = SAFileSystem.OpenRead(file_path);
                        if (stream != null)
                        {
                            if (stream.CanSeek)
                                return BundleItem.LoadFromStream(stream);

                            Log.E("Stream is not seekable: {0}, {1}", stream.CanSeek, file_path);
                            stream?.Close();
                        }

                        return BundleItem.LoadFromFile(file_path);
                    }

            }
        }

        public BundleManifest LoadManifest()
        {
            var mgr = _FileMgr.Val;
            if (mgr == null)
                return null;

            byte[] bytes = mgr.ReadAllBytes(_BundleManifestName);
            if (bytes == null)
                return null;
            string content = System.Text.Encoding.UTF8.GetString(bytes);
            return UnityEngine.JsonUtility.FromJson<BundleManifest>(content);
        }

        protected override void OnRelease()
        {
        }

        public sealed class BundleItem : IBundleMgr.IExternalRef
        {
            private AssetBundle _Bundle;
            private Stream _Stream;
            public static BundleItem Create(UnityEngine.AssetBundle ab, Stream stream = null)
            {
                if (ab == null)
                    return null;
                BundleItem ret = new BundleItem();
                ret._Bundle = ab;
                ret._Stream = stream;
                return ret;
            }

            public static BundleItem LoadFromFile(string file_path)
            {
                UnityEngine.AssetBundle ab = UnityEngine.AssetBundle.LoadFromFile(file_path);
                if (ab == null)
                    return null;
                BundleItem ret = new BundleItem();
                ret._Bundle = ab;
                ret._Stream = null;
                return ret;
            }

            public static BundleItem LoadFromStream(Stream stream)
            {
                UnityEngine.AssetBundle ab = UnityEngine.AssetBundle.LoadFromStream(stream);
                if (ab == null)
                    return null;
                BundleItem ret = new BundleItem();
                ret._Bundle = ab;
                ret._Stream = stream;
                return ret;
            }

            public AssetBundle UnityBundle => _Bundle;


            public UnityEngine.Object LoadAsset(string name, Type unityAssetType)
            {
                return _Bundle.LoadAsset(name, unityAssetType);
            }

            public AssetBundleRequest LoadAssetAsync(string name, Type unityAssetType)
            {
                return _Bundle.LoadAssetAsync(name, unityAssetType);
            }

            public void UnloadBundle(bool unloadAllLoadedObjects)
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

    }
}
