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

namespace FH
{
    public sealed class UIRoot
    {
        public const string C_RES_PATH = "ui_root";
        public const float C_WIDTH = 1920;
        public const float C_HEIGHT = 1080;

        private static UIRoot _inst;

        public GameObject _root;
        public RectTransform _2d_root;
        public Transform _3d_root;
        public Canvas _2d_canvas;
        public Canvas _3d_canvas;

        public static void Init()
        {
            UIRoot root = UIRoot.GetInst();
        }

        public static RectTransform Root2D
        {
            get { return UIRoot.GetInst()._2d_root; }
        }

        public static Transform Root3D
        {
            get { return UIRoot.GetInst()._3d_root; }
        }

        public static Canvas Canvas2D
        {
            get { return UIRoot.GetInst()._2d_canvas; }
        }

        public static Canvas Canvas3D
        {
            get { return UIRoot.GetInst()._3d_canvas; }
        }

        public static Camera Camera2D
        {
            get { return UIRoot.GetInst()._2d_canvas.worldCamera; }
        }

        public static Camera Camera3D
        {
            get
            {
                var canvas = UIRoot.GetInst()._3d_canvas;
                if (canvas == null)
                    return null;
                return canvas.worldCamera;
            }
        }

        public static UIRoot GetInst()
        {
            if (_inst == null)
                _inst = new UIRoot();
            _inst._init();
            return _inst;
        }

        public void _init()
        {
            if (_root != null)
                return;

            //1. 先查找当前的 Canvas 是否存在
            Canvas now_canvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            if (now_canvas != null)
            {
                _2d_canvas = now_canvas.rootCanvas;
                _3d_canvas = _2d_canvas;
                _root = _2d_canvas.gameObject;
                _2d_root = _2d_canvas.transform as RectTransform;
                _3d_root = _2d_root;

                if (EventSystem.current == null)
                {
                    GameObject evt_obj = new GameObject("EventSystem");
                    evt_obj.AddComponent<EventSystem>();
                    evt_obj.AddComponent<StandaloneInputModule>();
                }
                return;
            }

            //2. 如果没有就创建一个
            {
                GameObject prefab = Resources.Load<GameObject>(C_RES_PATH);
                _root = GameObject.Instantiate<GameObject>(prefab);
                _root.name = "UiRoot";
                if (!Application.isPlaying)
                    _root.hideFlags = HideFlags.DontSave;
                else
                    GameObject.DontDestroyOnLoad(_root);
                Transform t = _root.transform.Find("2d");
                _2d_root = t as RectTransform;
                _2d_canvas = _2d_root.GetComponent<Canvas>();
                _3d_root = _root.transform.Find("3d");
                _3d_canvas = _3d_root.GetComponent<Canvas>();
            }
        }
    }
}
