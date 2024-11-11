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
    public abstract class UIRedDotBehaviour : MonoBehaviour, EventSet2<Str, UIRedDotValue>.IHandler
    {
        public string _Path = "";
        public bool _AutoBind = false;

        [NonSerialized] protected string _CurrentBindPath;
        [NonSerialized] protected EventSet2<Str, UIRedDotValue>.Handle _EventHandler;

        public void Awake()
        {
            _CurrentBindPath = _Path;
        }

        public void OnEnable()
        {
            if (!_AutoBind)
                return;

            UnBind();
            _EventHandler = UIRedDotMgr.Reg(_CurrentBindPath, this);
            if (_EventHandler.Valid)
            {
                OnEvent(_CurrentBindPath, UIRedDotMgr.Get(_CurrentBindPath));
            }
            else
            {
                OnEvent(_CurrentBindPath, default);
            }

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
            _CurrentBindPath = string.Format(_Path, param1, param2);
            Bind();
        }

        /// <summary>
        /// 一旦调用,就是非自动的
        /// </summary>
        public void BindWithParam(string param)
        {
            UnBind();
            _CurrentBindPath = string.Format(_Path, param);
            Bind();
        }
        /// <summary>
        /// 一旦调用,就是非自动的
        /// </summary>
        public void BindPath(string path)
        {
            UnBind();
            _CurrentBindPath = path;
            Bind();
        }

        /// <summary>
        /// 一旦调用,就是非自动的
        /// </summary>
        public void Bind()
        {
            UnBind();
            _EventHandler = UIRedDotMgr.Reg(_CurrentBindPath, this);
            if (_EventHandler.Valid)                
                OnEvent(_CurrentBindPath, default);
            _AutoBind = false;
        }

        public void UnBind()
        {
            if (!_EventHandler.Valid)
                return;
            _EventHandler.Destroy();
            _EventHandler = default;

            OnEvent(_CurrentBindPath, default);
        }

        public void OnDestroy()
        {
            _EventHandler.Destroy();
            _EventHandler = default;            
        }

        void EventSet2<Str, UIRedDotValue>.IHandler.HandleEvent(Str key, UIRedDotValue val)
        {
            OnEvent(key, val);
        }

        protected abstract void OnEvent(Str key, UIRedDotValue val);
    }
}
