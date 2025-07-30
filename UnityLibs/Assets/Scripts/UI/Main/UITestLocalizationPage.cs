using FH;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FH.UI;

namespace Game
{
    public sealed class UITestLocalizationPage : UIPageBase<UITestLocalizationView>
    {
        public static List<(string langCode, string langName)> _LangList = new List<(string, string)>
        {
            ("en","English"),
            ("zh-Hans","中文"),
        };

        protected override void OnUI2Open()
        {
            base.OnUI2Open();

            LocMgr.EventLangChanged = () =>
            {
                LocMgr.NotiLangChanged(UIRoot.Canvas2D);
            };

            var opIndex = _FindOptionIndex(LocMgr.Lang, BaseView._Selector);
            if (opIndex >= 0)
                BaseView._Selector.value = opIndex;

            BaseView._Selector.onValueChanged.AddListener(_OnSelectorChanged);
            BaseView._BtnClose.OnClick = this.UIClose;
        }

        protected override void OnUI5Close()
        {
            BaseView._Selector.onValueChanged.RemoveListener(_OnSelectorChanged);
            base.OnUI5Close();
        }

        private void _OnSelectorChanged(int index)
        {
            string langCode = _FindLangCode(BaseView._Selector, index);
            if (langCode != null)
                LocMgr.ChangeLang(langCode);
        }


        private static string _FindLangCode(Dropdown selector, int index)
        {
            var op = selector.options[index];

            foreach (var p in _LangList)
            {
                if (op.text == p.langName)
                {
                    return p.langCode;
                }
            }
            return null;
        }

        private static int _FindOptionIndex(string langCode, Dropdown selector)
        {
            string langName = null;
            foreach (var p in _LangList)
            {
                if (p.langCode == langCode)
                {
                    langName = p.langName;
                    break;
                }
            }

            if (langName == null)
                return -1;

            for (int i = 0; i < selector.options.Count; i++)
            {
                if (selector.options[i].text == langName)
                    return i;
            }
            return -1;

        }
    }
}