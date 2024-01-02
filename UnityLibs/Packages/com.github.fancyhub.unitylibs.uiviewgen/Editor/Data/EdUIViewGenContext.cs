/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FH.UI.ViewGenerate.Ed
{
    /// <summary>
    /// 生成View的上下文
    /// </summary>
    public sealed class EdUIViewGenContext
    {
        public enum EMode
        {
            /// <summary>
            /// 如果该prefab 依赖另外一个子prefab
            /// 如果 子prefab的代码生成过，就不再生成了，即使该子prefab 生成过了，也不在生成
            /// </summary>
            AutoDependency,

            /// <summary>
            /// 不管子prefab 是否生成过，都生成
            /// </summary>
            AllDependency,
        }

        private EMode _Mode;
        public UIViewGeneratorConfig Config;
        private EdUIViewPathPool _PathPool;
        private EdUIViewDescDB _DBDesc;

        public List<EdUIView> ViewList;

        public EdUIViewGenContext(UIViewGeneratorConfig config, EdUIViewDescDB db_desc, EMode mode = EMode.AutoDependency)
        {
            this.Config = config;
            _Mode = mode;
            _DBDesc = db_desc;
            _PathPool = new EdUIViewPathPool();
        }

        public EdUIViewDescDB DBDesc => _DBDesc;

        public void AddInitPaths(IEnumerable<string> prefab_path)
        {
            foreach (string p in prefab_path)
            {
                AddInitPath(p);
            }
        }

        public void AddInitPath(string prefab_path)
        {
            EdUIViewDesc conf = _DBDesc.FindDescWithPrefabPath(prefab_path);
            if (null == conf)
            {
                conf = _CreateConfWithPath(prefab_path);
                _DBDesc.AddConf(conf);
            }

            _PathPool.Push(prefab_path);
        }

        public EdUIViewDesc AddDependPath(string prefab_path)
        {
            return _FindOrCreateDesc(prefab_path, _Mode);
        }

        public EdUIViewDesc AddDependPath_Variant(string prefab_path)
        {
            return _FindOrCreateDesc(prefab_path, EMode.AllDependency);
        }


        /// <summary>
        /// 获取下一个 prefab path，用来生成代码
        /// </summary>
        /// <returns></returns>
        public EdUIViewDesc GetNextPrefabConf()
        {
            string path = _PathPool.Pop();
            if (null == path)
                return null;

            return _DBDesc.FindDescWithPrefabPath(path);
        }

        private EdUIViewDesc _FindOrCreateDesc(string prefab_path, EMode mode)
        {
            EdUIViewDesc conf = _DBDesc.FindDescWithPrefabPath(prefab_path);
            switch (mode)
            {
                case EMode.AutoDependency:
                    {
                        if (conf == null)
                            _PathPool.Push(prefab_path);
                    }
                    break;

                case EMode.AllDependency:
                    _PathPool.Push(prefab_path);
                    break;

                default:
                    UnityEngine.Debug.LogError("Error : " + mode);
                    break;
            }

            if (null == conf)
            {
                conf = _CreateConfWithPath(prefab_path);
                _DBDesc.AddConf(conf);
            }
            return conf;
        }


        private EdUIViewDesc _CreateConfWithPath(string prefab_path)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefab_path);
            if (prefab == null)
                return null;

            string parent_prefab_path = null;
            UnityEngine.GameObject orig_prefab = EdUIViewGenPrefabUtil.GetOrigPrefabWithVariant(prefab);
            if (orig_prefab != null)
            {
                parent_prefab_path = AssetDatabase.GetAssetPath(orig_prefab);
            }

            EdUIViewDesc ret = new EdUIViewDesc(prefab_path, parent_prefab_path);
            return ret;
        }
    }
}