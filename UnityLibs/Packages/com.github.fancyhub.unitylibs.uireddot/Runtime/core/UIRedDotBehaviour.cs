/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/16 16:20:24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class UIRedDotBehaviour : MonoBehaviour, EventSet2<Str, int>.IHandler
    {
        public string _Path = "";
        public bool _AutoBind = false;

        [NonSerialized] protected bool _HasReg = false;
        [NonSerialized] protected string _BindPath;

        public void Awake()
        {
            _BindPath = _Path;
        }

        public void OnEnable()
        {
            if (!_AutoBind)
                return;

            UnBind();
            if (UIRedDotData.Reg(_BindPath, this))
                _HasReg = true;
            else
                OnEvent(_BindPath, 0);
        }

        public void OnDisable()
        {
            if (_AutoBind)
                UnBind();
        }

        /// <summary>
        /// 一旦调用,就是非自动的
        /// </summary>
        public void BindWithParam(string param1, string param2)
        {
            UnBind();
            _BindPath = string.Format(_Path, param1, param2);
            Bind();
        }

        /// <summary>
        /// 一旦调用,就是非自动的
        /// </summary>
        public void BindWithParam(string param)
        {
            UnBind();
            _BindPath = string.Format(_Path, param);
            Bind();
        }
        /// <summary>
        /// 一旦调用,就是非自动的
        /// </summary>
        public void BindPath(string path)
        {
            UnBind();
            _BindPath = path;
            Bind();
        }

        /// <summary>
        /// 一旦调用,就是非自动的
        /// </summary>
        public void Bind()
        {
            UnBind();
            if (UIRedDotData.Reg(_BindPath, this))
                _HasReg = true;
            else
                OnEvent(_BindPath, 0);
            _AutoBind = false;
        }

        public void UnBind()
        {
            if (!_HasReg)
                return;
            UIRedDotData.UnReg(_BindPath, this);
            _HasReg = false;

            OnEvent(_BindPath, 0);
        }

        public void OnDestroy()
        {
            if (!_HasReg)
                return;
            UIRedDotData.UnReg(_BindPath, this);
            _HasReg = false;
        }

        void EventSet2<Str, int>.IHandler.HandleEvent(Str key, int val)
        {
            OnEvent(key, val);
        }

        protected abstract void OnEvent(Str key, int val);
    }
}
