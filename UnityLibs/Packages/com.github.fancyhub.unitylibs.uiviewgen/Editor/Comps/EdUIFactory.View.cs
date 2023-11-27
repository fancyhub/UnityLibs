/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FH.UI.View.Gen.ED
{
    public partial class EdUIFactory
    {
        public EdUIView CreateView(EdUIViewConf conf)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(conf.PrefabPath);

            var type = PrefabUtility.GetPrefabAssetType(prefab);
            switch (type)
            {
                case PrefabAssetType.Regular:
                    {
                        EdUIView ret = new EdUIView();
                        ret.Conf = conf;
                        ret.Prefab = prefab;
                        ret.IsVariant = false;
                        ret.ParentClass = _data.Config.BaseClassName;
                        return ret;
                    }

                case PrefabAssetType.Variant:
                    {
                        GameObject orig_prefab = EdUIViewGenPrefabUtil.GetOrigPrefabWithVariant(prefab);
                        string orig_prefab_path = AssetDatabase.GetAssetPath(orig_prefab);
                        if (_data.Config.IsPrefabPathValid(orig_prefab_path))
                        {
                            var parent_conf = _data.AddDependPath_Variant(orig_prefab_path);
                            EdUIView ret = new EdUIView();
                            ret.Conf = conf;
                            ret.Prefab = prefab;
                            ret.ParentClass = parent_conf.ClassName;
                            ret.IsVariant = true;
                            return ret;
                        }
                        else
                        {
                            UnityEngine.Debug.LogErrorFormat("Prefab 路径不合法\n Prefab : [{0}] \n Variant Prefab : [{1}]\n", orig_prefab_path, conf.PrefabPath);
                            EdUIView ret = new EdUIView();
                            ret.Conf = conf;
                            ret.Prefab = prefab;
                            ret.ParentClass = _data.Config.BaseClassName;
                            ret.IsVariant = false;
                            return ret;
                        }
                    }

                default:
                    Debug.LogErrorFormat(prefab, " 未知的类型 {0}", type);
                    return null;
            }
        }
    }
}
