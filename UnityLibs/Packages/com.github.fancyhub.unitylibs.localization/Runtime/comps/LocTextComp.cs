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
    [RequireComponent(typeof(Text))]
    public sealed class LocTextComp : LocComp
    {
        public LocTextStyleAsset Style;

        private Text _Text;
        private Text _GetText()
        {
            if (_Text == null)
                _Text = GetComponent<Text>();
            return _Text;
        }

        public override void OnLocalize(string lang)
        {
            if (!LocMgr.TryGet(this._LocKey, out var tran))
                return;
            Text text = _GetText();
            if (text.text != tran)
                text.text = tran;

            _ApplyStyle(lang);
        }

#if UNITY_EDITOR
        public override void EdDoLocalize(string lang)
        {
            if (!LocMgr.EdTryGet(this._LocKey, lang, out var tran))
                return;

            Text text = _GetText();
            bool changed = false;
            if (text.text != tran)
            {
                text.text = tran;
                changed = true;
            }

            if (_ApplyStyle(lang))
                changed = true;

            if (changed)
            {
                UnityEditor.EditorUtility.SetDirty(text);
            }
        }
#endif

        public void OnDestroy()
        {
            _FontResRef.RemoveUser(this);
            _FontResRef = default;
        }


        private bool _ApplyStyle(string lang)
        {
            if (Style == null)
                return false;

            LocTextStyleAsset.TextStyle text_style = Style.Find(lang);
            if (text_style == null)
            {
                LocLog._.Assert(this, false, "找不到Style: {0}", lang);
                return false;
            }

            bool changed = false;
            Font font = _LoadFont(text_style);
            Text text = _GetText();
            if (text.font != font)
            {
                text.font = font;
                changed = true;
            }

            if (text.fontSize != text_style.FontSize)
            {
                text.fontSize = text_style.FontSize;
                changed = true;
            }

            if (text.fontStyle != text_style.FontStyle)
            {
                text.fontStyle = text_style.FontStyle;
                changed = true;
            }

            if (text.lineSpacing != text_style.LineSpace)
            {
                text.lineSpacing = text_style.LineSpace;
                changed = true;
            }

            return changed;
        }

        private ResRef _FontResRef;
        private Font _LoadFont(LocTextStyleAsset.TextStyle style)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return style.Font.EdLoad();
            }
#endif

            _FontResRef.RemoveUser(this);
            _FontResRef = ResMgr.Load(style.Font.Path);
            _FontResRef.AddUser(this);
            return _FontResRef.Get<Font>();
        }
    }
}