/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/19
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using UnityEngine.UI;
namespace FH
{
    public static class RawImageLoaderExt
    {
        public static bool ExtSetTexture(this RawImage img, string path)
        {
            if (img == null)
                return false;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(path);
                if (texture == null)
                    return false;
                img.texture = texture;
                return true;
            }
#endif
            RawImageLoader img_loader = img.GetComponent<RawImageLoader>();
            if (img_loader == null)
                img_loader = img.gameObject.AddComponent<RawImageLoader>();
            return img_loader.SetTexture(path);
        }

        [RequireComponent(typeof(RawImage))]
        private sealed class RawImageLoader : MonoBehaviour
        {
            private RawImage _Image;
            private ResRef _ResRef;
            private Texture _Orig;
            public void Awake()
            {
                _Image = GetComponent<RawImage>();
                _Orig = _Image.texture;
            }

            public void OnDestroy()
            {
                _ResRef.RemoveUser(this);
            }

            public bool SetTexture(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    _ResRef.RemoveUser(this);
                    _Image.texture = _Orig;
                    return true;
                }

                var res_ref = ResMgr.Load(path, true);
                Texture t = res_ref.Get<Texture>();
                if (t == null)
                    return false;

                _ResRef.RemoveUser(this);
                _ResRef = res_ref;
                _Image.texture = t;
                _ResRef.AddUser(this);
                return true;
            }
        }
    }
}
