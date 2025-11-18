/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2020/5/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;

namespace FH
{
    public interface IDynamicComponent
    {
        public void OnDynamicRelease();
    }


    //GameObject pool 相关的操作
    public static class GameObjectPoolUtil
    {
        public static Transform _dummy_inactive;
        public static Transform _dummy_active;
        public static Transform _dummy_dynamic_root;
        public static Transform GetDynamicRoot()
        {
            if (_dummy_dynamic_root == null)
            {
                GameObject obj = new GameObject("DynamicRoot");
                GameObject.DontDestroyOnLoad(obj);
                _dummy_dynamic_root = obj.transform;
            }

            return _dummy_dynamic_root;
        }

        //获取 dummy点，非激活模式的
        public static Transform GetDummyInactive()
        {
            if (null == _dummy_inactive)
            {
                GameObject obj = new GameObject("__PoolInactive");
                GameObject.DontDestroyOnLoad(obj);
                _dummy_inactive = obj.transform;

                obj.SetActive(false);

                //移远一些
                //float pos = 1000000;
                //_dummy_inactive.transform.localPosition = new Vector3(pos, pos, pos);
            }

            return _dummy_inactive;
        }

        //获取dummy点，激活模式的
        public static Transform GetDummyActive()
        {
            if (null == _dummy_active)
            {
                GameObject obj = new GameObject("__PoolActive");
                GameObject.DontDestroyOnLoad(obj);
                _dummy_active = obj.transform;
                float pos = 10000;
                _dummy_active.transform.localPosition = new Vector3(pos, pos, pos);
            }
            return _dummy_active;
        }

        internal static GameObject InstNew(string path, GameObject prefab)
        {
            //1. check
            if (null == prefab || string.IsNullOrEmpty(path))
            {
                Log.Assert(null != prefab, "prefab 为空");
                Log.Assert(!string.IsNullOrEmpty(path), "路径为空");
                return null;
            }

            Transform dummy = GetDummyInactive();
            GameObject obj_inst = GameObject.Instantiate(prefab, dummy, false);


            return obj_inst;
        }

        internal static AsyncInstantiateOperation<GameObject> InstNewAsync(string path, GameObject prefab)
        {
            //1. check
            if (null == prefab || string.IsNullOrEmpty(path))
            {
                Log.Assert(null != prefab, "prefab 为空");
                Log.Assert(!string.IsNullOrEmpty(path), "路径为空");
                return null;
            }

            Transform dummy = GetDummyInactive();
            return GameObject.InstantiateAsync(prefab, dummy);

        }

        internal static void InstRename(GameObject obj_inst, GameObject prefab)
        {
#if DEBUG
            if (obj_inst == null || prefab == null)
                return;

            obj_inst.name = prefab.name + "_" + obj_inst.GetInstanceID();
#endif
        }

        internal static void Push2Pool(GameObject obj)
        {
            //1. check
            if (null == obj)
                return;

            //2. 清除动态增加的组件
            var comps = obj.ExtGetCompsInChildren<MonoBehaviour>(true);
            for (int i = 0; i < comps.Count; i++)
            {
                if (comps[i] is IDynamicComponent dynamic_comp)
                {
                    dynamic_comp.OnDynamicRelease();
                }
            }
            comps.Clear();

            //3. 获取组件, 组件disable/enable模式
            obj.transform.SetParent(GetDummyInactive(), false);
        }
    }
}
