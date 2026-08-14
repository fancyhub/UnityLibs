using System;
using System.Collections.Generic;
using UnityEngine;

/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/25
 * Title   : 
 * Desc    : 
*************************************************************************************/

namespace FH.UI
{
    public sealed class UIViewCompReference : MonoBehaviour
    {
        private static List<UIViewCompReference> _Temp = new List<UIViewCompReference>();

        [System.Serializable]
        public struct Pair
        {
            public string name;
            public UnityEngine.Object obj;
            public Pair(string name, UnityEngine.Object obj)
            {
                this.name = name;
                this.obj = obj;
            }
        }
        //表示本脚本属于父级prefab还是属于自己
        public string _prefab_name;

        public Pair[] _objs = new Pair[0];

        public UnityEngine.Object Get(string key)
        {
            UnityEngine.Object ret = null;
            for (int i = 0; i < _objs.Length; i++)
            {
                if (_objs[i].name == key)
                {
                    ret = _objs[i].obj;
                    break;
                }
            }
            if (!ret)
                return null;
            return ret;
        }

        public T Get<T>(string key) where T : UnityEngine.Object
        {
            UnityEngine.Object ret = Get(key);            
            return ret as T;
        }


        public UnityEngine.GameObject GetGameObject(string key)
        {
            UnityEngine.Object ret = Get(key);
            if (!ret)
                return null;

            if (ret is GameObject obj)
                return obj;
            else if (ret is Component comp)
                return comp.gameObject;
            return null;
        }
       

        public static UIViewCompReference Find(GameObject obj, string prefab_name)
        {
            UIViewCompReference ret = null;
            _Temp.Clear();
            obj.GetComponents<UIViewCompReference>(_Temp);
            for (int i = 0; i < _Temp.Count; i++)
            {
                if (_Temp[i]._prefab_name == prefab_name)
                {
                    ret = _Temp[i];
                    break;
                }
            }
            _Temp.Clear();
            return ret;
        }

#if UNITY_EDITOR

        public void EdClear()
        {
            _objs = new Pair[0];
        }

        public bool EdExist(string key)
        {
            for (int i = 0; i < _objs.Length; i++)
            {
                if (_objs[i].name == key)
                    return true;
            }
            return false;
        }

        public void EdAdd(string key, UnityEngine.Object obj)
        {
            if (!Application.isEditor)
                return;

            List<Pair> t = new List<Pair>(_objs);
            t.Add(new Pair(key, obj));
            _objs = t.ToArray();
        }

        public void EdSet(string key, UnityEngine.Object obj)
        {
            for (int i = 0; i < _objs.Length; i++)
            {
                if (_objs[i].name == key)
                {
                    _objs[i].obj = obj;
                    return;
                }
            }

            Debug.AssertFormat(false, "找不到 {0}", key);
        }
#endif
    }
}
