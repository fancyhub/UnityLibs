/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FH;

namespace Game
{
    /// <summary>
    /// 不同的框架,修改这个内容
    /// </summary>
    public sealed class NoticeChannelRoot : INoticeChannelRoot
    {
        private static SimpleGameObjectInstPool _ItemDummyPool = new SimpleGameObjectInstPool();
        private static RectTransform _UIRootCanvas;
        private Transform _Layer;

        private string _DummyName;
        private NoticeDummyConfig _Config;
        private RectTransform _ChannelRoot;

        private IResHolder _ResHolder;

        public NoticeChannelRoot(NoticeDummyConfig config, string dummy_name, IResHolder holder)
        {
            _Config = config;
            _DummyName = dummy_name;
            _ResHolder = holder;
        }

        public GameObject CreateItemDummy()
        {
            GameObject obj = _ItemDummyPool.PopFree();
            if (obj == null)
            {
                obj = new GameObject("NoticeItemDummy");
            }

            var root = _GetOrCreateChanRoot();
            obj.transform.SetParent(root, false);
            return obj;
        }

        public void ReleaseItemDummy(GameObject obj)
        {
            _ItemDummyPool.Recycle(obj);
        }

        public void Destroy()
        {

        }


        private Transform _GetOrCreateChanRoot()
        {
            if (_ChannelRoot == null)
            {
                GameObject obj = new GameObject(_DummyName);
                _ChannelRoot = obj.AddComponent<RectTransform>();
                _ChannelRoot.SetParent(_FindLayer(), false);
                _ChannelRoot.anchorMin = _ChannelRoot.anchorMax = _Config.Pos;
            }

            if (_ChannelRoot.parent != _FindLayer())
            {
                _ChannelRoot.SetParent(_FindLayer(), false);
            }

            return _ChannelRoot;
        }

        private Transform _FindLayer()
        {
            if (_Layer != null)
                return _Layer;

            if (_UIRootCanvas == null)
            {
                //找到UIRoot
                CanvasScaler canvas = GameObject.FindFirstObjectByType<CanvasScaler>();
                if (canvas == null)
                {
                    UnityEngine.Debug.Log("找不到UIRoot");
                }
                else
                {
                    _UIRootCanvas = canvas.GetComponent<RectTransform>();
                }
            }

            if (_UIRootCanvas == null)
                return null;

            if (string.IsNullOrEmpty(_Config.LayerName))
                return _UIRootCanvas;

            for (int i = 0; i < _UIRootCanvas.childCount; i++)
            {
                var child = _UIRootCanvas.GetChild(i);
                if (child.name == _Config.LayerName)
                {
                    _Layer = child;
                    return _Layer;
                }
            }

            UnityEngine.Debug.LogError("找不到 Layer " + _Config.LayerName);
            return null;
        }   
    }
}
