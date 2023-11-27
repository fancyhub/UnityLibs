/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;

namespace FH.UI.View.Gen.ED
{ 
    /// <summary>
    /// 数据类，用来处理 prefab_path 和 class的对应关系
    /// </summary>
    public class EdUIViewData
    {
        public enum EMode
        {
            /// <summary>
            /// 如果该prefab 依赖另外一个子prefab
            /// 如果 子prefab的代码生成过，就不再生成了，即使该子prefab 生成过了，也不在生成
            /// </summary>
            include_dep_auto,

            /// <summary>
            /// 不管子prefab 是否生成过，都生成
            /// </summary>
            include_dep_all,
        }

        private EMode _mode;
        private EdUIViewPathPool _path_pool;
        private EdUIViewConfDb _conf_db;
        public UIViewGenConfig Config;

        public EdUIViewData(EdUIViewConfDb conf_db, EMode mode = EMode.include_dep_auto)
        {
            this.Config = conf_db.Config;
            _mode = mode;
            _conf_db = conf_db;
            _path_pool = new EdUIViewPathPool();
        }

        public EdUIViewConfDb ConfigDB =>_conf_db;

        public void AddInitPaths(IEnumerable<string> prefab_path)
        {
            foreach (string p in prefab_path)
            {
                AddInitPath(p);
            }
        }

        public void AddInitPath(string prefab_path)
        {
            EdUIViewConf conf = _conf_db.FindConfWithPrefabPath(prefab_path);
            if (null == conf)
            {
                conf = _CreateConfWithPath(Config,prefab_path);
                _conf_db.AddConf(conf);
            }

            _path_pool.Push(prefab_path);
        }

        public EdUIViewConf AddDependPath(string prefab_path)
        {
            return _FindOrCreateConf(Config,_conf_db, _path_pool, prefab_path, _mode);
        }

        public EdUIViewConf AddDependPath_Variant(string prefab_path)
        {
            return _FindOrCreateConf(Config,_conf_db, _path_pool, prefab_path, EMode.include_dep_all);
        }


        /// <summary>
        /// 获取下一个 prefab path，用来生成代码
        /// </summary>
        /// <returns></returns>
        public EdUIViewConf GetNextPrefabConf()
        {
            string path = _path_pool.Pop();
            if (null == path)
                return null;

            return _conf_db.FindConfWithPrefabPath(path);
        }

        private static EdUIViewConf _FindOrCreateConf(UIViewGenConfig config, EdUIViewConfDb db, EdUIViewPathPool pool, string prefab_path, EMode mode)
        {            
            EdUIViewConf conf = db.FindConfWithPrefabPath(prefab_path);
            switch (mode)
            {
                case EMode.include_dep_auto:
                    {
                        if (conf == null)
                            pool.Push(prefab_path);
                    }
                    break;

                case EMode.include_dep_all:
                    pool.Push(prefab_path);
                    break;

                default:
                    UnityEngine.Debug.LogError("Error : " + mode);
                    break;
            }

            if (null == conf)
            {
                conf = _CreateConfWithPath(config,prefab_path);
                db.AddConf(conf);
            }
            return conf;
        }


        private static EdUIViewConf _CreateConfWithPath(UIViewGenConfig config, string prefab_path)
        {
            string class_name = EdUIViewGenPrefabUtil.GenClassNameFromPrefabPath(config,prefab_path);
            string parent_class_name = config.BaseClassName;

            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefab_path);
                UnityEngine.GameObject orig_prefab = EdUIViewGenPrefabUtil.GetOrigPrefabWithVariant(prefab);
                if (orig_prefab != null)
                {
                    string parent_path = AssetDatabase.GetAssetPath(orig_prefab);
                    parent_class_name = EdUIViewGenPrefabUtil.GenClassNameFromPrefabPath(config,parent_path);
                }
            }

            EdUIViewConf ret = new EdUIViewConf();
            ret.ClassName = class_name;
            ret.PrefabPath = prefab_path;
            ret.ParentClassName = parent_class_name;
            return ret;
        }
    }
}