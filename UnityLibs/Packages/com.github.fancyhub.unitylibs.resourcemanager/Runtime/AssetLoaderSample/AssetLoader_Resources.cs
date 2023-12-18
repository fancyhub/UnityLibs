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
        private sealed class ResRefDB
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

        private sealed class AssetRef : CPoolItemBase, IAssetRef
        {
            public UnityEngine.Object _Asset;
            public ResourceRequest _ResRequest;
            public ResRefDB _ResRefDB;

            public bool IsDone
            {
                get
                {
                    if (_ResRequest == null)
                        return true;
                    if (!_ResRequest.isDone)
                        return false;
                    _Asset = _ResRequest.asset;
                    _ResRefDB.IncRef(_Asset);
                    _ResRequest = null;
                    return true;
                }
            }

            public static AssetRef Create(ResRefDB res_ref_db, UnityEngine.Object asset)
            {
                if (res_ref_db == null || asset == null)
                    return null;
                res_ref_db.IncRef(asset);
                AssetRef ret = GPool.New<AssetRef>();
                ret._Asset = asset;
                ret._ResRefDB = res_ref_db;
                return ret;
            }

            public static AssetRef Create(ResRefDB res_ref_db, ResourceRequest request)
            {
                if (res_ref_db == null || request == null)
                    return null;

                AssetRef ret = GPool.New<AssetRef>();
                ret._ResRequest = request;
                ret._ResRefDB = res_ref_db;
                return ret;
            }

            public UnityEngine.Object Asset => _Asset;

            protected override void OnPoolRelease()
            {
                _ResRequest = null;
                if (_Asset == null)
                {
                    _ResRefDB = null;
                    return;
                }

                var asset = _Asset;
                var resRefDB = _ResRefDB;
                _ResRefDB = null;
                _Asset = null;

                if (resRefDB.DecRef(asset) > 0)
                    return;
                //UnloadAsset may only be used on individual assets and can not be used on GameObject's / Components / AssetBundles or GameManagers
                if (asset is GameObject)
                    return;
                Resources.UnloadAsset(asset);
            }
        }

        private ResRefDB _ResRefDB = new ResRefDB();

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
                return AssetRef.Create(_ResRefDB, Resources.Load(path));
            else
                return AssetRef.Create(_ResRefDB, Resources.Load<Sprite>(path));
        }

        public IAssetRef LoadAsync(string path, bool sprite)
        {
            if (!sprite)
                return AssetRef.Create(_ResRefDB, Resources.LoadAsync(path));
            else
                return AssetRef.Create(_ResRefDB, Resources.LoadAsync<Sprite>(path));
        }

        protected override void OnRelease()
        {

        }
    }

}
