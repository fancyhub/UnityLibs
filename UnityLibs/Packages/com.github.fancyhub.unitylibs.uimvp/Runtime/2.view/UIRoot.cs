/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace FH.UI
{
    public sealed class UIRoot
    {
        private const string CResPath = "UIRoot";
        private static UIRoot _;
        public GameObject _root;

        public RectTransform _root_2d;
        public Canvas _canvas_2d;

        public static UIRoot Inst
        {
            get
            {
                if (_ == null)
                    _ = new UIRoot();
                _._init();
                return _;
            }
        }
        
        public static RectTransform Root2D
        {
            get { return Inst._root_2d; }
        }
         
        public static Canvas Canvas2D
        {
            get { return Inst._canvas_2d; }
        }  

        private void _init()
        {
            if (_root != null)
                return;

            //1. 先查找当前的 Canvas 是否存在
            Canvas now_canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (now_canvas != null)
            {
                _canvas_2d = now_canvas.rootCanvas;
                _root_2d = _canvas_2d.transform as RectTransform;

                _root = _canvas_2d.gameObject;

                if (EventSystem.current == null)
                {
                    GameObject evt_obj = new GameObject("EventSystem");
                    evt_obj.AddComponent<EventSystem>();
                    evt_obj.AddComponent<FHStandaloneInputModule>();
                }
                return;
            }

            //2. 如果没有就创建一个
            {
                GameObject prefab = Resources.Load<GameObject>(CResPath);
                _root = GameObject.Instantiate<GameObject>(prefab);
                _root.name="UIRoot";
                if (!Application.isPlaying)
                    _root.hideFlags = HideFlags.DontSave;
                else
                    GameObject.DontDestroyOnLoad(_root);

                _canvas_2d = _root.GetComponent<Canvas>();
                _root_2d = _canvas_2d.transform as RectTransform;                
            }
        }
    }
}
