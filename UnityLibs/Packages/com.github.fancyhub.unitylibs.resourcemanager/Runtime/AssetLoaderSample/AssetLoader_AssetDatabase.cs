/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/12/1 
 * Title   : 
 * Desc    : 
*************************************************************************************/
#if UNITY_EDITOR
#define AssetDatabaseAssetLoader
using UnityEditor;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FH.Res.SampleAssetLoader
{

    public class AssetLoader_AssetDatabase : CPtrBase, IAssetLoader
    {

#if AssetDatabaseAssetLoader
        public sealed class EditorResRequestComp : MonoBehaviour
        {
            public static EditorResRequestComp _;
            public static void StartReq(IEnumerator req)
            {
                if (_ == null)
                {
                    GameObject obj = new GameObject("EditorResRequestComp");
                    obj.hideFlags = HideFlags.HideAndDontSave;
                    GameObject.DontDestroyOnLoad(obj);
                    _ = obj.AddComponent<EditorResRequestComp>();
                }

                _.StartCoroutine(req);
            }
        }

        public sealed class EditorResRequest
        {
            private string _resPath;
            private bool _sprite;
            public bool isDone;
            public UnityEngine.Object asset;

            private IEnumerator _LoadAsync()
            {
                yield return null;

                if (_sprite)
                {
                    asset = AssetDatabase.LoadAssetAtPath<Sprite>(_resPath);
                }
                else
                {
                    asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_resPath);
                }
                isDone = true;
                if (asset != null)
                {
                    string real_path = AssetDatabase.GetAssetPath(asset);
                    if (real_path != _resPath)
                    {
                        ResLog._.E("资源路径大小写不对 \n {0} -> \n{1}", real_path, _resPath);
                        asset = null;
                    }
                }
                yield return null;
            }

            public static EditorResRequest LoadAsync(string path, bool sprite)
            {
                EditorResRequest req = new EditorResRequest();
                req._resPath = path;
                req._sprite = sprite;
                EditorResRequestComp.StartReq(req._LoadAsync());

                return req;
            }
        }

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
            public EditorResRequest _ResRequest;
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

            public static AssetRef Create(ResRefDB res_ref, EditorResRequest request)
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

        public bool IsResExist(string path)
        {
            return true;
        }

        public IAssetRef Load(string path, bool sprite)
        {
            UnityEngine.Object asset = null;
            if (!sprite)
                asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            else
                asset = AssetDatabase.LoadAssetAtPath<Sprite>(path);

            if (asset != null)
            {
                string real_path = AssetDatabase.GetAssetPath(asset);
                if (real_path != path)
                {
                    ResLog._.E("资源路径大小写不对 \n {0} -> \n{1}", real_path, path);
                    asset = null;
                }
            }

            return AssetRef.Create(_ResRef, asset);
        }

        public IAssetRef LoadAsync(string path, bool sprite)
        {
            if (!Application.isPlaying)
            {
                return Load(path, sprite);
            }

            return AssetRef.Create(_ResRef, EditorResRequest.LoadAsync(path,sprite));
        }

        protected override void OnRelease()
        {

        }



#else
        public string AtlasTag2Path(string atlasName)
        {
            throw new NotImplementedException();
        }

        public bool IsResExist(string path)
        {
            throw new NotImplementedException();
        }

        public IAssetRef Load(ResPath path)
        {
            throw new NotImplementedException();
        }

        public IAssetRef LoadAsync(ResPath path)
        {
            throw new NotImplementedException();
        }
        protected override void OnRelease()
        {
            throw new NotImplementedException();
        }
#endif

    }
}
