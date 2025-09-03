/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    [Serializable]
    public class LangItem
    {
        public string Lang;
        public bool Enable = true;

        public LangItem(string lang, bool enable = true)
        {
            Lang = lang;
            Enable = enable;
        }

        public override string ToString() { return Lang;}
    }

    [Serializable]
    public partial class LangSetting
    {
        public string FallbackLang = "en";
        public List<LangItem> Langs = new()
        {
            new("en"),
            new("zh-Hans"),
        };

        public int IndexOfLang(string lang)
        {
            for (int i = 0; i < Langs.Count; i++)
            {
                if (lang == Langs[i].Lang)
                    return i;
            }
            return -1;
        }

        public string GetLang(string lang)
        {
            if (string.IsNullOrEmpty(lang))
            {
                lang = GetFromSystemLang(Application.systemLanguage);
                LocLog._.D("param lang is null, use system lang: {1}", lang);
            }

            foreach (var p in Langs)
            {
                if (p.Lang == lang && p.Enable)
                    return p.Lang;
            }

            LocLog._.D("can't find lang: {0}, use fallback lang: {1}", lang, FallbackLang);
            return FallbackLang;
        }

        public static string GetFromSystemLang(SystemLanguage systemLang)
        {
            string code = null;
            switch (systemLang)
            {
                case SystemLanguage.Afrikaans: code = "af"; break;
                case SystemLanguage.Arabic: code = "ar"; break;
                case SystemLanguage.Basque: code = "eu"; break;
                case SystemLanguage.Belarusian: code = "be"; break;
                case SystemLanguage.Bulgarian: code = "bg"; break;
                case SystemLanguage.Catalan: code = "ca"; break;
                case SystemLanguage.Chinese: code = "zh-CN"; break;
                case SystemLanguage.ChineseSimplified: code = "zh-Hans"; break;
                case SystemLanguage.ChineseTraditional: code = "zh-Hant"; break;
                case SystemLanguage.SerboCroatian: code = "hr"; break;
                case SystemLanguage.Czech: code = "cs"; break;
                case SystemLanguage.Danish: code = "da"; break;
                case SystemLanguage.Dutch: code = "nl"; break;
                case SystemLanguage.English: code = "en"; break;
                case SystemLanguage.Estonian: code = "et"; break;
                case SystemLanguage.Faroese: code = "fo"; break;
                case SystemLanguage.Finnish: code = "fi"; break;
                case SystemLanguage.French: code = "fr"; break;
                case SystemLanguage.German: code = "de"; break;
                case SystemLanguage.Greek: code = "el"; break;
                case SystemLanguage.Hebrew: code = "he"; break;
                case SystemLanguage.Hungarian: code = "hu"; break;
                case SystemLanguage.Icelandic: code = "is"; break;
                case SystemLanguage.Indonesian: code = "id"; break;
                case SystemLanguage.Italian: code = "it"; break;
                case SystemLanguage.Japanese: code = "ja"; break;
                case SystemLanguage.Korean: code = "ko"; break;
                case SystemLanguage.Latvian: code = "lv"; break;
                case SystemLanguage.Lithuanian: code = "lt"; break;
                case SystemLanguage.Norwegian: code = "no"; break;
                case SystemLanguage.Polish: code = "pl"; break;
                case SystemLanguage.Portuguese: code = "pt"; break;
                case SystemLanguage.Romanian: code = "ro"; break;
                case SystemLanguage.Russian: code = "ru"; break;
                case SystemLanguage.Slovak: code = "sk"; break;
                case SystemLanguage.Slovenian: code = "sl"; break;
                case SystemLanguage.Spanish: code = "es"; break;
                case SystemLanguage.Swedish: code = "sv"; break;
                case SystemLanguage.Thai: code = "th"; break;
                case SystemLanguage.Turkish: code = "tr"; break;
                case SystemLanguage.Ukrainian: code = "uk"; break;
                case SystemLanguage.Vietnamese: code = "vi"; break;
                default: code = ""; break;
            }
            return code;
        }


#if UNITY_EDITOR
        public void EdChangeList(List<string> lang_list)
        {
            for (int i = Langs.Count - 1; i >= 0; i--)
            {
                if (!lang_list.Contains(Langs[i].Lang))
                {
                    Langs.RemoveAt(i);
                }
            }

            foreach (var p in lang_list)
            {
                if (IndexOfLang(p) < 0)
                {
                    Langs.Add(new LangItem(p));
                }
            }

            if (IndexOfLang(FallbackLang) < 0)
            {
                if (Langs.Count > 0)
                    FallbackLang = Langs[0].Lang;
                else
                    FallbackLang = "";
            }
        }
#endif
    }
}
