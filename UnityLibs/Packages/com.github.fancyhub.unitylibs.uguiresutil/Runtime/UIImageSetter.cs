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
    public static class UIImageSetterExt
    {

        public static void ExtPreloadSprite(this IResInstHolder holder, string name)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (holder == null)
            {
                Log.E("param holder is null, {0}", name);
                return;
            }

            var path = UIResMapConfig.FindSprite(name);
            if (string.IsNullOrEmpty(path))
            {
                Log.E("UIResMapConfig 找不到 Sprite: {0}", name);
                return;
            }

            holder.PreLoad(path, EResPathType.Sprite);
        }

        public static void ExtSyncSetSprite(this Image img, string name)
        {
            ExtSetSprite(img, name, true);
        }

        public static void ExtAsyncSetSprite(this Image img, string name)
        {
            ExtSetSprite(img, name, false);
        }

        public static void ExtSetSprite(this Image img, string name, bool sync)
        {
            if (img == null)
                return;
            string path = null;
            if (!string.IsNullOrEmpty(name))
            {
                path = UIResMapConfig.FindSprite(name);
                if (string.IsNullOrEmpty(path))
                {
                    Log.E("UIResMapConfig 找不到 Sprite: {0}", name);
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                    return;
                img.sprite = sprite;
                return;
            }
#endif

            UIImageSetter img_loader = img.GetComponent<UIImageSetter>();
            if (img_loader == null)
                img_loader = img.gameObject.AddComponent<UIImageSetter>();


            img_loader.SetSprite(path, sync);
        }

        [RequireComponent(typeof(Image))]
        internal sealed class UIImageSetter : MonoBehaviour
        {
            private const int CPriority = 0;

            private Image _Image;
            private ResRef _ResRef;
            private int _JobId;
            private string _Path;

            public void Awake()
            {
                _Image = GetComponent<Image>();
            }

            public void OnDestroy()
            {
                _ResRef.RemoveUser(this);
                _ResRef = default;
                _Path = null;

                _CancelJob();
            }

            public void SetSprite(string path, bool sync)
            {
                //1. 判断是否相同
                //这里不会因为 sync: true or false 发生变化
                if (_Image == null || _Path == path)
                    return;

                //2.只是清除
                if (string.IsNullOrEmpty(path))
                {
                    _Path = null;
                    _CancelJob();

                    _ResRef.RemoveUser(this);
                    _ResRef = default;

                    _Image.overrideSprite = null;
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
                    _Image.overrideSprite = null;

                    _ResRef = ResMgr.LoadSprite(path, true);
                    _Image.overrideSprite = _ResRef.Get<Sprite>();
                    if (_Image.overrideSprite != null)
                        _ResRef.AddUser(this);
                    else
                        _ResRef = default;
                }
                else //异步加载
                {
                    //先同步加载
                    ResRef new_res_ref = ResMgr.TryLoadExistSprite(path);
                    Sprite new_sprite = new_res_ref.Get<Sprite>();
                    if (new_sprite != null)
                    {
                        _ResRef.RemoveUser(this);
                        _ResRef = default;

                        _Image.overrideSprite = new_sprite;
                        _ResRef = new_res_ref;
                        _ResRef.AddUser(this);
                    }
                    else
                    {
                        bool result = ResMgr.AsyncLoad(_Path, EResPathType.Sprite,  _OnAsyncLoaded, out _JobId, CPriority);
                        if (!result)
                        {
                            _ResRef.RemoveUser(this);
                            _ResRef = default;
                            _Image.overrideSprite = null;
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

            private void _OnAsyncLoaded(int job_id, EResError error, ResRef res_ref)
            {
                if (job_id != _JobId)
                    return;
                _JobId = 0;

                Sprite new_sprite = null;
                if (res_ref.IsValid())
                {
                    new_sprite = res_ref.Get<Sprite>();
                }

                _Image.overrideSprite = new_sprite;
                _ResRef.RemoveUser(this);
                _ResRef = default;

                if (new_sprite != null)
                {
                    _ResRef = res_ref;
                    _ResRef.AddUser(this);
                }
            }
        }
    }
}
