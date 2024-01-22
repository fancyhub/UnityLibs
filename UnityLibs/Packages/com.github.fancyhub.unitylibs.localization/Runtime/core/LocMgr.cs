/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

namespace FH
{
    public sealed partial class LocMgr
    {
        public delegate List<(LocId key, string tran)> TranslationLoader(string lang);
        private static LocMgr _ = new LocMgr();

        private Dictionary<LocId, string> _Translation;

        private TranslationLoader _FuncLoader;

        private LocMgr()
        {
            _Translation = new Dictionary<LocId, string>(LocId.EqualityComparer);
        }

        public static void InitLog(ELogLvl log_lvl)
        {
            LocLog._.SetMasks(log_lvl);
        }


        #region  Lang & Load 
        public static void ChangeLang(string lang)
        {
            if (!LocLang.IsValid(lang))
            {
                LocLog._.E("Lang Is Not Valid {0}", lang);
                return;
            }

            if (LocLang.Lang == lang)
                return;

            //加载器还没有好, 等待Loader好了
            if (_._FuncLoader == null)
            {
                LocLang.SetLang(lang);
                return;
            }

            LocLog._.D("加载多语言表: {0}", lang);
            var trans = _._FuncLoader(lang);
            if (trans == null)
            {
                LocLog._.E("Lang {0}, Load Trans failed", lang);
                return;
            }

            _Change(lang, trans);
        }

        public static string Lang
        {
            get { return LocLang.Lang; }
        }

        public static TranslationLoader FuncLoader
        {
            get { return _._FuncLoader; }

            set
            {
                if (value == null)
                    return;

                _._FuncLoader = value;
                string lang = LocLang.Lang;
                if (string.IsNullOrEmpty(lang))
                {
                    LocLog._.D("当前语言为null", lang);
                    return;
                }

                LocLog._.D("加载多语言表: {0}", lang);
                var trans = _._FuncLoader(lang);
                if (trans == null)
                {
                    LocLog._.E("加载多语言表失败: {0}", lang);
                    return;
                }

                _Change(lang, trans);
            }
        }

        public static void Reload()
        {
            if (_._FuncLoader == null)
                return;

            string lang = LocLang.Lang;
            if (string.IsNullOrEmpty(lang))
                return;

            LocLog._.D("加载多语言表: {0}", lang);
            var trans = _._FuncLoader(lang);
            if (trans == null)
            {
                LocLog._.E("加载多语言表失败: {0}", lang);
                return;
            }
            _Change(lang, trans);
        }

        private static void _Change(string lang, List<(LocId key, string tran)> all)
        {
            if (!LocLang.IsValid(lang))
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
            LocLang.SetLang(lang);


            //通知
            _Notify();
        }

        private static void _Notify()
        {
            if (!Application.isPlaying)
                return;

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
        public static bool TryGet(LocId key, out string tran, UnityEngine.Object obj = null)
        {
            if (_._Translation.TryGetValue(key, out tran))
                return true;

            LocLog._.E(obj, "Can't find \"{0}\"", key.Key);
            return false;
        }

        public static bool TryGet(LocKey key, out string tran, UnityEngine.Object obj = null)
        {
            var int_key = key.ToLocId();
            if (_._Translation.TryGetValue(int_key, out tran))
                return true;

            LocLog._.E(obj, "Can't find \"{0}\"", key.Key);
            return false;
        }


        public static string Get(LocId key, UnityEngine.Object obj = null)
        {
            if (TryGet(key, out var ret, obj))
                return ret;
            return null;
        }

        public static string Get(LocKey key, UnityEngine.Object obj = null)
        {
            if (TryGet(key, out var ret, obj))
                return ret;
            return null;
        }
        #endregion
    }
}