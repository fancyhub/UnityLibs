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
    public static class ImageLoaderExt
    {
        public static bool ExtSetSprite(this Image img, string path)
        {
            if (img == null)
                return false;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                    return false;
                img.sprite = sprite;
                return true;
            }
#endif

            ImageLoader img_loader = img.GetComponent<ImageLoader>();
            if (img_loader == null)
                img_loader = img.gameObject.AddComponent<ImageLoader>();
            return img_loader.SetSprite(path);
        }

        [RequireComponent(typeof(Image))]
        internal sealed class ImageLoader : MonoBehaviour
        {
            private Image _Image;
            private ResRef _ResRef;
            public void Awake()
            {
                _Image = GetComponent<Image>();
            }

            public void OnDestroy()
            {
                _ResRef.RemoveUser(this);
            }

            public bool SetSprite(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    _ResRef.RemoveUser(this);
                    _Image.overrideSprite = null;
                    return true;
                }

                var res_ref = ResMgr.LoadSprite(path, true);
                Sprite s = res_ref.Get<Sprite>();
                if (s == null)
                    return false;

                _ResRef.RemoveUser(this);
                _ResRef = res_ref;
                _Image.overrideSprite = s;
                _ResRef.AddUser(this);
                return true;
            }
        }

    }
}
