/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System;
using FH.UI;
using LocKey = FH.LocKeyId;
namespace FH
{
    public sealed partial class LocMgr
    {
        private static Dictionary<string, string[]> _EdAllData = new()
        {
            //{ "Hello", new string[]{ "你好",""} },
            //{ "Hello1", new string[]{ "你好1",""} },
            //{ "Hello2", new string[]{ "你好2",""} },
        };

        public static bool EdContainsKey(string key)
        {
            return _EdAllData.ContainsKey(key);
        }

        public static int EdKeyCount => _EdAllData.Count;

        public static Dictionary<string, string[]> EdAllData => _EdAllData;

        public static bool EdTryGetTrans(string key, out string[] all_trans)
        {
            all_trans = null;
            if (string.IsNullOrEmpty(key))
                return false;

            return _EdAllData.TryGetValue(key, out all_trans);
        }

        public static bool EdTryGet(LocKeyStr key, string lang, out string trans)
        {
            if (_EdAllData.Count == 0 && _._FuncLoader != null)
                EdReloadAll();

            trans = null;
            if (string.IsNullOrEmpty(key.Key))
                return false;

            if (!EdTryGetTrans(key.Key, out var all_trans))
            {
                LocLog._.E("找不到 {0}", key.Key);
                return false;
            }

            int index = LangSettingAsset.EdIndexOfLang(lang);
            if (index < 0)
                return false;
            trans = all_trans[index];
            return true;
        }

        public static bool EdReloadAll()
        {
            if (_._FuncLoader == null)
            {
                LocLog._.E("EdReloadAll Faild, 加载函数is null ");
                return false;
            }

            if (typeof(LocKey) == typeof(LocKeyId))
            {
                var key_list = _._FuncLoader("KEY");
                var lang_list = LangSettingAsset.EdGetLangIdList();
                _EdAllData.Clear();
                Dictionary<LocKey, string> temp_dict = new(key_list.Count, LocKey.EqualityComparer);
                foreach (var p in key_list)
                {
                    if (string.IsNullOrEmpty(p.tran))
                    {
                        LocLog._.E("String Key Is null {0}", p.key.Key);
                        continue;
                    }

                    temp_dict[p.key] = p.tran;
                    _EdAllData[p.tran] = new string[lang_list.Count];
                }


                for (int i = 0; i < lang_list.Count; i++)
                {
                    string lang = lang_list[i].Lang;
                    var tran_list = _._FuncLoader(lang);
                    if (tran_list == null)
                    {
                        LocLog._.E("加载语言失败 {0}", lang);
                        continue;
                    }

                    foreach (var p in tran_list)
                    {
                        if (!temp_dict.TryGetValue(p.key, out string str_key))
                        {
                            LocLog._.E("语言{0}, 找不到对应的StringKey {1}", lang, p.key.Key);
                            continue;
                        }

                        _EdAllData.TryGetValue(str_key, out var trans_array);
                        if (trans_array == null)
                        {
                            LocLog._.E("语言{0}, 找不到 {1}", lang, str_key);
                            continue;
                        }

                        trans_array[i] = p.tran;
                    }
                }

                return true;
            }
            else
            {
                var lang_list = LangSettingAsset.EdGetLangIdList();
                var key_list = _._FuncLoader(lang_list[0].Lang);
                _EdAllData.Clear();
                foreach (var p in key_list)
                { 
                    _EdAllData[p.key.Key.ToString()] = new string[lang_list.Count];
                }


                for (int i = 0; i < lang_list.Count; i++)
                {
                    string lang = lang_list[i].Lang;
                    var tran_list = _._FuncLoader(lang);
                    if (tran_list == null)
                    {
                        LocLog._.E("加载语言失败 {0}", lang);
                        continue;
                    }

                    foreach (var p in tran_list)
                    { 

                        _EdAllData.TryGetValue(p.key.Key.ToString(), out var trans_array);
                        if (trans_array == null)
                        {
                            LocLog._.E("语言{0}, 找不到 {1}", lang, p.key.Key);
                            continue;
                        }

                        trans_array[i] = p.tran;
                    }
                }

                return true;
            }
        }
    }
}
#endif
