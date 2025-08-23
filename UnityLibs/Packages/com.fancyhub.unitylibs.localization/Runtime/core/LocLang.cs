/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;

namespace FH
{
    public sealed class LocLang
    {
        public const string LangKey = "KEY";

        public const string FallBack = "zh-Hans";

        /// <summary>
        /// FH.LocStrKeyBrowser.Browser 只取第一个
        /// </summary>
        public static readonly string[] LangList = new string[]
        {
            "zh-Hans",
            "EN",
        };

        private static LocLang _Inst = new LocLang();

        private const string CSavedKey = "LOC_SELECTED_LANG";

        public string _Lang = null;
        public readonly string SystemLang;
        private string _SavedLang = null;

        public LocLang()
        {
            SystemLang = ParseSystemLang();
            _SavedLang = PlayerPrefs.GetString(CSavedKey);

            if (_SetLang(_SavedLang))
                return;

            if (_SetLang(SystemLang))
                return;

            _SetLang(FallBack);
        }

        public static int IndexOf(string lang)
        {
            for (int i = 0; i < LangList.Length; i++)
            {
                if (LangList[i] == lang)
                    return i;
            }
            return -1;
        }

        public static int LangIndex
        {
            get
            {
                for (int i = 0; i < LangList.Length; i++)
                {
                    if (LangList[i] == _Inst._Lang)
                        return i;
                }
                return -1;
            }
        }

        public static string Lang
        {
            get { return _Inst._Lang; }
        }

        public static bool IsValid(string lang_code)
        {
            if (string.IsNullOrEmpty(lang_code))
                return false;
            foreach (var p in LangList)
            {
                if (p == lang_code)
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool SetLang(string lang_code)
        {
            return _Inst._SetLang(lang_code);
        }

        public static string ParseSystemLang()
        {
            string code = null;
            switch (Application.systemLanguage)
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

        private bool _SetLang(string lang_code)
        {
            if (string.IsNullOrEmpty(lang_code))
            {
                LocLog._.E("LangCode Is Null");
                return false;
            }

            if (_Lang == lang_code)
            {
                LocLog._.D("LangCode Is Same");
                return true;
            }

            if (!IsValid(lang_code))
            {
                LocLog._.E("LangCode Is Not Valid: {0}", lang_code);
                return false;
            }

            _Lang = lang_code;
            _SavedLang = _Lang;
            PlayerPrefs.SetString(CSavedKey, _SavedLang);

            LocLog._.D("Set Current Lang: {0}", lang_code);
            return true;
        }

    }
}