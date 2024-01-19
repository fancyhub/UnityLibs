/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace FH
{
    [RequireComponent(typeof(Image))]
    public sealed class LocImageComp : LocComp
    {
        private Image _Image;
        private Image _GetImage()
        {
            if (_Image == null)
                _Image = GetComponent<Image>();
            return _Image;
        }

        public override void OnLocalize(string lang)
        {
            if (!LocMgr.TryGet(this._LocKey, out var tran))
                return;
            Image image = _GetImage();
            image.ExtSetSprite(tran);
        }

#if UNITY_EDITOR
        public override void EdDoLocalize(string lang)
        {
            if (!LocMgr.EdTryGet(this._LocKey, lang, out var tran))
                return;

            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(tran);
            if (sprite == null)
            {
                LocLog._.E("加载Sprite失败 {0}:{1}", this._LocKey.Key, tran);
                return;
            }

            Image image = _GetImage();
            if (image.sprite == sprite)
                return;
            image.sprite = sprite;
            UnityEditor.EditorUtility.SetDirty(image);
        }
#endif         
    }
}