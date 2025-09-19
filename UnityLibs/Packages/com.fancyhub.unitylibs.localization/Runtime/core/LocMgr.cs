/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
#define USE_LOC_KEY_ID

using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using FH.UI;
using LocKey = FH.LocKeyInt;

namespace FH
{
    public enum ELocKeyMode
    {
        Str,
        Int,
    }

    public sealed partial class LocMgr
    {
        public delegate List<(LocKey key, string tran)> TranslationLoader(string lang);
        private static LocMgr _ = new LocMgr();

        public const ELocKeyMode KeyMode = ELocKeyMode.Int;

        public static Action EventLangChanged = _SampleNotify;
        private static string _CurrentLang = null;
        public static string CurrentLang => _CurrentLang;

        private Dictionary<LocKey, string> _Translation;
        private LangSetting _LangSetting;

        private TranslationLoader _FuncLoader;

        private LocMgr()
        {
            _Translation = new Dictionary<LocKey, string>(LocKey.EqualityComparer);
        }

        public static void Init(ELogLvl log_lvl, LangSettingAsset langSettingAsset)
        {
            LocLog._.SetMasks(log_lvl);
            _._LangSetting = langSettingAsset.Setting;
            string lang = _LoadLang();
            _CurrentLang = _._LangSetting.GetLang(lang);
        }


        #region  Lang & Load        
        public static void ChangeLang(string lang)
        {
            if (string.IsNullOrEmpty(lang))
            {
                LocLog._.E("Lang Is Not Valid {0}", lang);
                return;
            }

            if (_CurrentLang == lang)
                return;

            _CurrentLang = lang;

            //加载器还没有好, 等待Loader好了
            if (_._FuncLoader == null)
            {
                return;
            }

            LocLog._.D("加载多语言表: {0}", _CurrentLang);
            var trans = _._FuncLoader(_CurrentLang);
            if (trans == null)
            {
                LocLog._.E("Lang {0}, Load Trans failed", _CurrentLang);
                return;
            }

            _Change(lang, trans);
        }

        public static TranslationLoader FuncLoader
        {
            get { return _._FuncLoader; }

            set
            {
                if (value == null)
                    return;

                _._FuncLoader = value;
                if (string.IsNullOrEmpty(_CurrentLang))
                {
                    LocLog._.D("当前语言为null", _CurrentLang);
                    return;
                }

                LocLog._.D("加载多语言表: {0}", _CurrentLang);
                var trans = _._FuncLoader(_CurrentLang);
                if (trans == null)
                {
                    LocLog._.E("加载多语言表失败: {0}", _CurrentLang);
                    return;
                }

                _Change(_CurrentLang, trans);
            }
        }

        public static void Reload()
        {
            if (_._FuncLoader == null)
                return;

            if (string.IsNullOrEmpty(_CurrentLang))
                return;

            LocLog._.D("加载多语言表: {0}", _CurrentLang);
            var trans = _._FuncLoader(_CurrentLang);
            if (trans == null)
            {
                LocLog._.E("加载多语言表失败: {0}", _CurrentLang);
                return;
            }
            _Change(_CurrentLang, trans);
        }

        public static void NotiLangChanged(Canvas rootCanvas)
        {
            if (rootCanvas == null)
                return;

            rootCanvas = rootCanvas.rootCanvas;
            if (!rootCanvas.ExtIsEnable())
                return;
            rootCanvas.BroadcastMessage(nameof(LocComp.DoLocalize), SendMessageOptions.DontRequireReceiver);
        }

        private static void _Change(string lang, List<(LocKey key, string tran)> all)
        {
            if (string.IsNullOrEmpty(lang))
            {
                LocLog._.E("Lang Is Not Valid {0}", lang);
                return;
            }

            _._Translation.Clear();
            foreach (var p in all)
            {
                if (p.tran == null)
                    continue;

                _._Translation[p.key] = p.tran;
            }
            _Save(lang);


            //通知
            _Notify();
        }

        private const string CSaveKey = "LOC_SELECTED_LANG_ID";
        private static void _Save(string lang)
        {
            PlayerPrefs.SetString(CSaveKey, lang);
        }

        private static string _LoadLang()
        {
            return PlayerPrefs.GetString(CSaveKey);
        }

        private static void _Notify()
        {
            if (!Application.isPlaying)
                return;
            EventLangChanged?.Invoke();
        }

        private static void _SampleNotify()
        {
            string func_name = nameof(LocComp.DoLocalize);
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                var objs = scene.GetRootGameObjects();

                foreach (var p in objs)
                {
                    var canvas = p.GetComponent<UnityEngine.Canvas>();
                    if (canvas != null && canvas.isActiveAndEnabled)
                    {
                        canvas.BroadcastMessage(func_name, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }
        }


        #endregion

        #region Get 

        public static bool TryGet(LocKeyInt key, out string tran, UnityEngine.Object obj = null)
        {
#if USE_LOC_KEY_ID
            if (_._Translation.TryGetValue(key, out tran))
                return true;
            LocLog._.E(obj, "Can't find \"{0}\"", key.Key);
            return false;
#else 
            LocLog._.E(obj, "not implement");
            tran = null;
            return false;
#endif
        }

        public static bool TryGet(LocKeyStr key, out string tran, UnityEngine.Object obj = null)
        {
#if USE_LOC_KEY_ID
            LocKeyInt int_key = key.ToLocId();
            if (_._Translation.TryGetValue(int_key, out tran))
                return true;

#else
            if (_._Translation.TryGetValue(key, out tran))
                return true;

#endif
            LocLog._.E(obj, "Can't find \"{0}\"", key.Key);
            return false;
        }

        public static string Get(LocKeyInt key, UnityEngine.Object obj = null)
        {
            if (TryGet(key, out var ret, obj))
                return ret;
            return null;
        }

        public static string Get(LocKeyStr key, UnityEngine.Object obj = null)
        {
            if (TryGet(key, out var ret, obj))
                return ret;
            return null;
        }

        #endregion
    }
}