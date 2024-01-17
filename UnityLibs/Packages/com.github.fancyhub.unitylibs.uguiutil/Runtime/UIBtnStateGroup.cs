/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2022/8/10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    public class UIBtnStateGroup : MonoBehaviour
    {
        public enum EState
        {
            None,
            Normal,
            Selected,
            Pressed,
            Disable,
        }

        [SerializeField] private GameObject[] _Normal;
        [SerializeField] private GameObject[] _Select;
        [SerializeField] private GameObject[] _Pressed;
        [SerializeField] private GameObject[] _Disable;

        [NonSerialized] private EState _State = EState.None;

        [NonSerialized] public UIBtnStateGroupFlag Flag;

        public void Awake()
        {
            Flag = new UIBtnStateGroupFlag(this);
            SetState(EState.Normal, false);
        }

        public void Reset()
        {
            Flag = new UIBtnStateGroupFlag(this);
            SetState(EState.Normal, false);
        }

        public void SetState(EState state, bool force = false)
        {
            //1. 比较当前state
            if (_State == state && !force)
                return;
            _State = state;

            //2. 获取objs
            GameObject[] state_objs = _GetStateObjs(state);
            List<GameObject> all_objs = _GetAllObjs();

            //3.隐藏当前节点不需要显示的所有节点
            foreach (GameObject node in all_objs)
            {
                if (node == null)
                    continue;

                //过滤目标状态需要显示的节点，不同状态要显示的节点可能会有重叠
                if (!_Contains(state_objs, node))
                    node.SetActive(false);
            }

            //4.显示目标状态的节点
            foreach (GameObject node in state_objs)
            {
                if (null != node)
                    node.SetActive(true);
            }
        }

#if UNITY_EDITOR
        // [Sirenix.OdinInspector.ShowInInspector]
#endif
        public EState State
        {
            get { return _State; }
            set { SetState(value, false); }
        }

        private static bool _Contains(GameObject[] array, GameObject obj)
        {
            if (obj == null || array == null)
                return false;
            for (int i = 0; i < array.Length; i++)
                if (array[i] == obj)
                    return true;
            return false;
        }

        private static List<GameObject> _STempObjList = new List<GameObject>();
        private List<GameObject> _GetAllObjs()
        {
            _STempObjList.Clear();
            if (_Normal != null && _Normal.Length > 0)
                _STempObjList.AddRange(_Normal);
            if (_Select != null && _Select.Length > 0)
                _STempObjList.AddRange(_Select);
            if (_Pressed != null && _Pressed.Length > 0)
                _STempObjList.AddRange(_Pressed);
            if (_Disable != null && _Disable.Length > 0)
                _STempObjList.AddRange(_Disable);
            return _STempObjList;
        }

        private static GameObject[] _SEmptyArray = System.Array.Empty<GameObject>();
        private GameObject[] _GetStateObjs(EState state)
        {
            switch (state)
            {
                default:
                    UnityEngine.Debug.LogError($"未知类型 {state}");
                    return _SEmptyArray;

                case EState.None:
                    return _SEmptyArray;

                case EState.Normal:
                    return _Normal ?? _SEmptyArray;

                case EState.Selected:
                    return _Select ?? _SEmptyArray;

                case EState.Pressed:
                    return _Pressed ?? _SEmptyArray;

                case EState.Disable:
                    return _Disable ?? _SEmptyArray;
            }
        }
    }


    /// <summary>
    /// 为了支持 可以用 键盘和鼠标同时操作的功能
    /// 
    /// </summary>
    public struct UIBtnStateGroupFlag
    {
        [Flags]
        public enum EFlag 
        {
            None = 0,
            Normal = 1 << 0,
            Selected = 1 << 1,
            PressedKeybord = 1 << 2,
            PressedTouch = 1 << 3,
            Disable = 1 << 3,
        }

        [NonSerialized] private UIBtnStateGroup _BtnStateGroup;
        [NonSerialized] private EFlag _Flags;

        public UIBtnStateGroupFlag(UIBtnStateGroup btn_state_group)
        {
            _BtnStateGroup = btn_state_group;
            _Flags = EFlag.None;
        }

        public void SetFlag(EFlag flag, bool enable)
        {
            if (enable)
                _Flags |= flag;
            else
                _Flags &= ~flag;

            var state = _CalcStateWithFlag(_Flags);
            _BtnStateGroup.SetState(state, false);
        }

        public bool GetFlag(EFlag flag)
        {
            return (_Flags & flag) != 0;
        }
        public bool Selected { get { return GetFlag(EFlag.Selected); } set { SetFlag(EFlag.Selected, value); } }
        public bool PressedKey { get { return GetFlag(EFlag.PressedKeybord); } set { SetFlag(EFlag.PressedKeybord, value); } }
        public bool PressedTouch { get { return GetFlag(EFlag.PressedTouch); } set { SetFlag(EFlag.PressedTouch, value); } }
        public bool Disabled { get { return GetFlag(EFlag.Disable); } set { SetFlag(EFlag.Disable, value); } }

#if UNITY_EDITOR
        //[Sirenix.OdinInspector.ShowInInspector]
#endif
        public EFlag Flags { get { return _Flags; } }


        private static UIBtnStateGroup.EState _CalcStateWithFlag(EFlag flags)
        {
            if ((flags & EFlag.Disable) != 0)
                return UIBtnStateGroup.EState.Disable;

            if ((flags & EFlag.PressedKeybord) != 0)
                return UIBtnStateGroup.EState.Pressed;

            if ((flags & EFlag.PressedTouch) != 0)
                return UIBtnStateGroup.EState.Pressed;

            if ((flags & EFlag.Selected) != 0)
                return UIBtnStateGroup.EState.Selected;

            return UIBtnStateGroup.EState.Normal;
        }
    } 
}
