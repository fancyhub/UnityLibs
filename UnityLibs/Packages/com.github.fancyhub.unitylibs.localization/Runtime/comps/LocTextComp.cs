/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZString = System.String;

namespace FH
{
    /// <summary>
    /// 不支持 阿拉伯 语言
    /// </summary>
    [RequireComponent(typeof(Text))]
    public sealed class LocTextComp : LocComp
    {
        [SerializeField] private List<string> _Arguments;
        [SerializeField] private LocTextStyleAsset _Style;

        private Text _Text;
        public Text Text
        {
            get
            {
                if (_Text == null)
                    _Text = GetComponent<Text>();
                return _Text;
            }
        }

        public override void OnLocalize(string lang)
        {
            if (!TryGetTran(out var tran))
                return;

            string content = _Format(tran, _Arguments);

            Text text_comp = Text;
            if (text_comp.text != content)
                text_comp.text = content;

            _ApplyStyle(lang);
        }

#if UNITY_EDITOR
        public override void EdDoLocalize(string lang)
        {
            if (!EdTryGetTran(lang, out var tran))
                return;
            string content = _Format(tran, _Arguments);

            Text text_comp = Text;
            bool changed = false;
            if (text_comp.text != content)
            {
                text_comp.text = content;
                changed = true;
            }

            if (_ApplyStyle(lang))
                changed = true;

            if (changed)
            {
                UnityEditor.EditorUtility.SetDirty(text_comp);
            }
        }
#endif

        public void OnDestroy()
        {
            _FontResRef.RemoveUser(this);
            _FontResRef = default;
        }

        private static string _Format(string translation, List<string> args)
        {
            if (args == null || args.Count == 0)
                return translation;

            switch (args.Count)
            {
                case 1:
                    return ZString.Format(translation, args[0]);
                case 2:
                    return ZString.Format(translation, args[0], args[1]);
                case 3:
                    return ZString.Format(translation, args[0], args[1], args[2]);
                case 4:
                    return ZString.Format(translation, args[0], args[1], args[2], args[3]);
                case 5:
                    return ZString.Format(translation, args[0], args[1], args[2], args[3], args[4]);
                case 6:
                    return ZString.Format(translation, args[0], args[1], args[2], args[3], args[4], args[5]);
                case 7:
                    return ZString.Format(translation, args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                default:
                    LocLog._.E("LocTextComp Format Args's num out of TypeParamMax");
                    return translation;
            }
        }


        private bool _ApplyStyle(string lang)
        {
            if (_Style == null)
                return false;

            LocTextStyleAsset.TextStyle text_style = _Style.Find(lang);
            if (text_style == null)
            {
                LocLog._.Assert(this, false, "找不到Style: {0}", lang);
                return false;
            }

            bool changed = false;
            Font font = _LoadFont(text_style);
            Text text = Text;
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