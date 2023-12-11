/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/12/1 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.Res.SampleAssetLoader
{
    public class AssetLoader_Resource : CPtrBase, IAssetLoader
    {
        public sealed class ResRefDB
        {
            public Dictionary<int, int> _Data = new Dictionary<int, int>();
            public void IncRef(UnityEngine.Object obj)
            {
                if (obj == null)
                    return;
                int id = obj.GetInstanceID();
                if (_Data.TryGetValue(id, out var count))
                {
                    _Data[id] = count + 1;
                }
                else
                    _Data.Add(id, 1);
            }

            public int DecRef(UnityEngine.Object obj)
            {
                if (obj == null)
                    return 0;
                int id = obj.GetInstanceID();
                if (!_Data.TryGetValue(id, out var count))
                    return 0;

                count--;
                if (count <= 0)
                    _Data.Remove(id);
                return count;
            }
        }

        public sealed class AssetRef : CPoolItemBase, IAssetRef
        {
            public UnityEngine.Object _Asset;
            public ResourceRequest _ResRequest;
            public ResRefDB _ResRef;
            public bool IsDone
            {
                get
                {
                    if (_ResRequest == null)
                        return true;
                    if (!_ResRequest.isDone)
                        return false;
                    _Asset = _ResRequest.asset;
                    _ResRef.IncRef(_Asset);
                    _ResRequest = null;
                    return true;
                }
            }

            public static AssetRef Create(ResRefDB res_ref, UnityEngine.Object asset)
            {
                if (res_ref == null || asset == null)
                    return null;
                res_ref.IncRef(asset);
                AssetRef ret = GPool.New<AssetRef>();
                ret._Asset = asset;
                ret._ResRef = res_ref;
                return ret;
            }

            public static AssetRef Create(ResRefDB res_ref, ResourceRequest request)
            {
                if (res_ref == null || request == null)
                    return null;

                AssetRef ret = GPool.New<AssetRef>();
                ret._ResRequest = request;
                ret._ResRef = res_ref;
                return ret;
            }

            public UnityEngine.Object Asset => _Asset;

            protected override void OnPoolRelease()
            {
                _ResRequest = null;
                if (_Asset == null)
                    return;

                var asset = _Asset;
                var resRef = _ResRef;
                _ResRef = null;
                _Asset = null;
                if (resRef.DecRef(asset) >= 0)
                    return;

                //UnloadAsset may only be used on individual assets and can not be used on GameObject's / Components / AssetBundles or GameManagers
                if (asset is GameObject)
                    return;
                Resources.UnloadAsset(asset);
            }
        }

        public ResRefDB _ResRef = new ResRefDB();

        public string AtlasTag2Path(string atlasName)
        {
            return string.Empty;
        }

        public EAssetStatus GetAssetStatus(string path)
        {
            return EAssetStatus.Exist;
        }

        public IAssetRef Load(string path, bool sprite)
        {
            if (!sprite)
                return AssetRef.Create(_ResRef, Resources.Load(path));
            else
                return AssetRef.Create(_ResRef, Resources.Load<Sprite>(path));
        }

        public IAssetRef LoadAsync(string path, bool sprite)
        {
            if (!sprite)
                return AssetRef.Create(_ResRef, Resources.LoadAsync(path));
            else
                return AssetRef.Create(_ResRef, Resources.LoadAsync<Sprite>(path));
        }

        protected override void OnRelease()
        {

        }
    }

}
