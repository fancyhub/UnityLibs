/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace FH.UI
{
    [RequireComponent(typeof(RawImage))]
    public sealed class LocRawImageComp : LocComp
    {
        private RawImage _Image;
        private RawImage _GetImage()
        {
            if (_Image == null)
                _Image = GetComponent<RawImage>();
            return _Image;
        }

        public override void OnLocalize(string lang)
        {
            if (!TryGetTran(out var tran))
                return;
            RawImage image = _GetImage();
            LocLog._.D(this, "设置Texture:{0}", tran);
            image.ExtSetTexture(tran, false);
        }

#if UNITY_EDITOR
        public override void EdDoLocalize(string lang)
        {
            if (!EdTryGetTran(lang, out var tran))
                return;

            Texture texture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture>(tran);
            if (texture == null)
            {
                LocLog._.E(this, "加载Texture失败 {0}", tran);
                return;
            }

            RawImage image = _GetImage();
            if (image.texture == texture)
                return;
            image.texture = texture;
            UnityEditor.EditorUtility.SetDirty(image);
        }
#endif         
    }
}