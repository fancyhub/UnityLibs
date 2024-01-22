/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/19
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;

namespace FH.UI
{
    public static class UIRawImageSetterExt
    {
        public static void ExtSyncSetTexture(this RawImage img, string name)
        {
            ExtSetTexture(img, name, true);
        }

        public static void ExtAsyncSetTexture(this RawImage img, string name)
        {
            ExtSetTexture(img, name, false);
        }

        public static void ExtSetTexture(this RawImage img, string name, bool sync)
        {
            if (img == null)
                return;

            string path = null;
            if (!string.IsNullOrEmpty(name))
            {
                path = UIResMapConfig.FindTexture(name);
                if (string.IsNullOrEmpty(path))
                {
                    Log.E("UIResMapConfig 找不到 Texture: {0}", name);
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (texture == null)
                    return;
                img.texture = texture;
                return;
            }
#endif
            UIRawImageSetter img_loader = img.GetComponent<UIRawImageSetter>();
            if (img_loader == null)
                img_loader = img.gameObject.AddComponent<UIRawImageSetter>();
            img_loader.SetTexture(path, sync);
        }

        [RequireComponent(typeof(RawImage))]
        private sealed class UIRawImageSetter : MonoBehaviour
        {
            private const int CPriority = 0;

            private RawImage _Image;
            private ResRef _ResRef;
            private Texture _Base;
            private Texture _OverrideTexture;
            private int _JobId;
            private string _Path;

            public void Awake()
            {
                _Image = GetComponent<RawImage>();
                _Base = _Image.texture;
                _OverrideTexture = null;
            }

            public Texture OverrideTexture
            {
                get { return _OverrideTexture; }
                set
                {
                    if (_OverrideTexture == value)
                        return;
                    _OverrideTexture = value;
                    _Image.texture = _OverrideTexture == null ? _Base : _OverrideTexture;
                }
            }


            public void OnDestroy()
            {
                _ResRef.RemoveUser(this);
                _ResRef = default;
                _Path = null;

                _CancelJob();
            }

            public void SetTexture(string path, bool sync)
            {
                //1. 判断是否相同
                //这里不会因为 sync: true or false 发生变化
                if (_Path == path)
                    return;

                //2.只是清除
                if (string.IsNullOrEmpty(path))
                {
                    _Path = null;
                    _CancelJob();

                    _ResRef.RemoveUser(this);
                    OverrideTexture = null;
                    return;
                }

                //3.Revert
                _CancelJob();
                _Path = path;

                //4. 同步加载
                if (sync)
                {
                    _ResRef.RemoveUser(this);
                    _ResRef = default;
                    OverrideTexture = null;

                    _ResRef = ResMgr.Load(path, true);
                    OverrideTexture = _ResRef.Get<Texture>();
                    if (OverrideTexture != null)
                        _ResRef.AddUser(this);
                    else
                        _ResRef = default;
                }
                else //异步加载
                {
                    //先同步加载
                    ResRef new_res_ref = ResMgr.TryLoadExist(path);
                    Texture new_texture = new_res_ref.Get<Texture>();
                    if (new_texture != null)
                    {
                        _ResRef.RemoveUser(this);
                        _ResRef = default;

                        OverrideTexture = new_texture;
                        _ResRef = new_res_ref;
                        _ResRef.AddUser(this);
                    }
                    else
                    {
                        EResError error = ResMgr.AsyncLoad(_Path, false, CPriority, _OnAsyncLoaded, out _JobId);
                        if (error != EResError.OK)
                        {
                            _ResRef.RemoveUser(this);
                            _ResRef = default;
                            OverrideTexture = null;
                            _JobId = 0;
                        }
                    }
                }
            }

            private void _CancelJob()
            {
                if (_JobId == 0)
                    return;
                ResMgr.CancelJob(_JobId);
                _JobId = 0;
            }

            private void _OnAsyncLoaded(EResError code, string path, EResType resType, int job_id)
            {
                if (job_id != _JobId)
                    return;
                _JobId = 0;

                if (code != EResError.OK)
                {
                    _ResRef.RemoveUser(this);
                    _ResRef = default;
                    OverrideTexture = null;
                    return;
                }

                ResRef new_res_ref = ResMgr.Load(path, false);
                Texture new_texture = new_res_ref.Get<Texture>();
                OverrideTexture = new_texture;

                _ResRef.RemoveUser(this);
                _ResRef = default;
                _ResRef = new_res_ref;
                _ResRef.AddUser(this);
            }

        }
    }
}
