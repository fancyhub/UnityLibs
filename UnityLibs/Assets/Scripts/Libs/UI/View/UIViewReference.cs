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
    public class UIViewReference : MonoBehaviour
    {
        [System.Serializable]
        public struct Pair
        {
            public string name;
            public GameObject obj;
            public Pair(string name, GameObject obj)
            {
                this.name = name;
                this.obj = obj;
            }
        }
        //表示本脚本属于父级prefab还是属于自己
        public string _prefab_name;

        public Pair[] _objs = new Pair[0];
        //public string[] _name_list = new string[0];
        //public GameObject[] _obj_list = new GameObject[0];

        public GameObject GetObj(string key)
        {
            for (int i = 0; i < _objs.Length; i++)
            {
                if (_objs[i].name == key)
                    return _objs[i].obj;
            }
            return null;
        }

        public void Clear()
        {
            _objs = new Pair[0];
        }

        public bool Exist(string key)
        {
            for (int i = 0; i < _objs.Length; i++)
            {
                if (_objs[i].name == key)
                    return true;
            }
            return false;
        }

        public bool Exist(string key, GameObject obj)
        {
            for (int i = 0; i < _objs.Length; i++)
            {
                if (_objs[i].name == key)
                {
                    return _objs[i].obj == obj;
                }
            }
            return false;
        }


        public T GetComp<T>(string key) where T : Component
        {
            var obj = GetObj(key);
            if (null == obj)
                return null;

            T t = obj.GetComponent<T>();
            return t;
        }
#if UNITY_EDITOR

        public void EdAdd(string key, GameObject obj)
        {
            if (!Application.isEditor)
                return;

            List<Pair> t = new List<Pair>(_objs);
            t.Add(new Pair(key, obj));
            _objs = t.ToArray();
        }

        public void EdSet(string key, GameObject obj)
        {
            for (int i = 0; i < _objs.Length; i++)
            {
                if (_objs[i].name == key)
                {
                    _objs[i].obj = obj;
                    return;
                }
            }

            Log.Assert(false, "找不到 {0}", key);
        }
#endif
    }
}
