/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/10 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    public sealed class UIStateGroup : MonoBehaviour
    {
        [Serializable]
        public sealed class UIState
        {
            public string name;
            public GameObject[] objs;
            public bool Contains(GameObject obj)
            {
                if (objs == null)
                    return false;
                if (obj == null)
                    return false;
                for (int i = 0; i < objs.Length; i++)
                {
                    if (objs[i] == obj)
                        return true;
                }
                return false;
            }

            public void SetActive()
            {
                if (objs == null || objs.Length == 0)
                    return;
                for (int i = 0; i < objs.Length; i++)
                {
                    var obj = objs[i];
                    if (obj == null)
                        continue;
                    obj.SetActive(true);
                }
            }

            public void CopyTo(List<GameObject> out_list)
            {
                if (out_list == null || objs == null || objs.Length == 0)
                    return;

                for (int i = 0; i < objs.Length; i++)
                {
                    var obj = objs[i];
                    if (obj == null)
                        continue;
                    out_list.Add(obj);
                }
            }
        }

        [SerializeField] private UIState[] _states;

        [NonSerialized] private int _index = -1;

        public void Awake()
        {
            SetState(0, true);
        }

        public void Reset()
        {
            SetState(0, true);
        }
#if UNITY_EDITOR
        //[Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ValueDropdown("EdGetAllStates"), Sirenix.OdinInspector.PropertyOrder(-1)]
        public (int, string) EdCurrentState
        {
            get
            {
                return (_index, GetCurStateName());
            }
            set
            {
                SetState(value.Item1, true);
            }
        }

        public List<(int, string)> EdGetAllStates()
        {
            List<(int, string)> ret = new List<(int, string)>();
            if (_states == null || _states.Length == 0)
                return ret;
            for (int i = 0; i < _states.Length; i++)
            {
                ret.Add((i, _states[i].name));
            }
            return ret;
        }
#endif

        public void SetState(int state_index, bool force = false)
        {
            if (_index == state_index && !force)
                return;
            _index = state_index;

            if (_states == null || _states.Length == 0)
                return;

            List<GameObject> objs = _GetAllObjs(_states, _index);
            if (_index < 0 || _index >= _states.Length)
            {
                foreach (var p in objs)
                    p.SetActive(false);
                return;
            }

            UIState selected_state = _states[_index];
            foreach (var p in objs)
            {
                if (!selected_state.Contains(p))
                    p.SetActive(false);
            }
            selected_state.SetActive();
        }

        public void SetState(string state_name, bool force = false)
        {
            int index = IndexOf(state_name);
            SetState(index, force);
        }

        public int IndexOf(string state_name)
        {
            if (_states == null || _states.Length == 0)
                return -1;
            for (int i = 0; i < _states.Length; i++)
            {
                if (_states[i].name == state_name)
                {
                    return i;
                }
            }
            return -1;
        }

        public string GetCurStateName()
        {
            if (_states == null || _index < 0 || _index >= _states.Length)
                return null;
            return _states[_index].name;
        }

        public int GetCurStateIndex()
        {
            return _index;
        }

        public int StateCount { get { if (_states == null) return 0; return _states.Length; } }

        private static List<GameObject> _STempList = new List<GameObject>();
        private static List<GameObject> _GetAllObjs(UIState[] states, int ignore_index)
        {
            _STempList.Clear();
            if (states == null || states.Length == 0)
                return _STempList;
            for (int i = 0; i < states.Length; i++)
            {
                if (i == ignore_index)
                    continue;
                states[i].CopyTo(_STempList);
            }

            return _STempList;
        }
    }
}
