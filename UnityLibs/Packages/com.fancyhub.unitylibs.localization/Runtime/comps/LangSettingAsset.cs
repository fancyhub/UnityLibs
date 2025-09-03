/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/9/3
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    [CreateAssetMenu(fileName = "Lang Setting", menuName = "fancyhub/LangSetting")]
    public class LangSettingAsset : ScriptableObject
    {
        public const string CPath = "Assets/Res/UI/Config/LangSetting.asset";
        private const string CEdPath = "Assets/Res/UI/Config/LangSetting.asset";

        public LangSetting Setting = new LangSetting();

#if UNITY_EDITOR        
        private static LangSettingAsset _;

        private static LangSetting EdGetLangSetting()
        {
            if (_ == null)
                _ = UnityEditor.AssetDatabase.LoadAssetAtPath<LangSettingAsset>(CEdPath);
            return _.Setting;
        }
        public static List<LangItem> EdGetLangIdList()
        {
            return EdGetLangSetting().Langs;
        }

        public static string[] EdGetLangNameList()
        {
            var list = EdGetLangSetting().Langs;
            string[] ret = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                ret[i] = list[i].Lang;
            }

            return ret;
        }

        public static string EdGetFallbackLangName()
        {
            return EdGetLangSetting().FallbackLang;
        }

        public static int EdIndexOfLang(string lang)
        {
            return EdGetLangSetting().IndexOfLang(lang);            
        }
#endif
    }
}
