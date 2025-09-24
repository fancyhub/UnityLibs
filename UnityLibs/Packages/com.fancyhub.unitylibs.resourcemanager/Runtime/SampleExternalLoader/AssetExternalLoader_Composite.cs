/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/10/16
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.SampleExternalLoader
{
    public sealed class AssetExternalLoader_Composite : CPtrBase, IResMgr.IExternalAssetLoader
    {
        private IResMgr.IExternalAssetLoader _AbLoader;
        private IResMgr.IExternalAssetLoader _ResLoader;
        public AssetExternalLoader_Composite(IResMgr.IExternalAssetLoader ab_loader)
        {
            _AbLoader = ab_loader;
            _ResLoader = new AssetExternalLoader_Resource();
        }

        public string AtlasTag2Path(string atlasName)
        {
            //resource 不会触发 atlas的加载
            return _AbLoader.AtlasTag2Path(atlasName);
        }

        public EAssetStatus GetAssetStatus(string path)
        {
            if (_UseAB(path))
                return _AbLoader.GetAssetStatus(path);
            return _ResLoader.GetAssetStatus(path);
        }

        public IResMgr.IExternalAssetRef Load(string path, Type unityAssetType)
        {
            if (_UseAB(path))
                return _AbLoader.Load(path, unityAssetType);
            return _ResLoader.Load(path, unityAssetType);
        }

        public IResMgr.IExternalAssetRef LoadAsync(string path, Type unityAssetType)
        {
            if (_UseAB(path))
                return _AbLoader.LoadAsync(path, unityAssetType);
            return _ResLoader.LoadAsync(path, unityAssetType);
        }

        private static bool _UseAB(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            return path.StartsWith("Assets/") || path.StartsWith("Packages/");
        }

        protected override void OnRelease()
        {
            _AbLoader.Destroy();
            _ResLoader.Destroy();
        }
    }
}
