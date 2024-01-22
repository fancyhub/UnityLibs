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
            if (!TryGetTran(out var tran))
                return;
            Image image = _GetImage();

            LocLog._.D(this, "设置Sprite:{0}", tran);
            image.ExtSetSprite(tran, false);
        }

#if UNITY_EDITOR
        public override void EdDoLocalize(string lang)
        {
            if (!EdTryGetTran(lang, out var tran))
                return;

            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(tran);
            if (sprite == null)
            {
                LocLog._.E(this, "加载Sprite失败 {0}", tran);
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