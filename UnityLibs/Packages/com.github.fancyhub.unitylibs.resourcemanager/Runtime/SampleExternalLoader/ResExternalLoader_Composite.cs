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
    public sealed class ResExternalLoader_Composite : CPtrBase, IResMgr.IExternalLoader
    {
        private IResMgr.IExternalLoader _AbLoader;
        private IResMgr.IExternalLoader _ResLoader;
        public ResExternalLoader_Composite(IResMgr.IExternalLoader ab_loader)
        {
            _AbLoader = ab_loader;
            _ResLoader = new ResExternalLoader_Resource();
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

        public IResMgr.IExternalRef Load(string path, EResPathType resPathType)
        {
            if (_UseAB(path))
                return _AbLoader.Load(path, resPathType);
            return _ResLoader.Load(path, resPathType);
        }

        public IResMgr.IExternalRef LoadAsync(string path, EResPathType resPathType)
        {
            if (_UseAB(path))
                return _AbLoader.LoadAsync(path, resPathType);
            return _ResLoader.LoadAsync(path, resPathType);
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
