/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/31 17:09:02
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;


namespace FH.UI
{
    internal sealed class UIViewWrapper : CPoolItemBase
    {
        public static UIViewWrapper Create()
        {
            return GPool.New<UIViewWrapper>();
        }

        public bool _bg_mask;
        public IUILayerView _view;
        public EUIBgClickMode _click_mode;
        public IUILayerViewBGHandler _click_handler;
        public int _order;

        public void SetOrder(int order)
        {
            _order = order;
            _view?.SetOrder(order);
        }

        protected override void OnPoolRelease()
        {
            _view = null;
            _click_handler = null;
        }
    }

    internal struct UIViewNode
    {
        public int _id;
        public int _node_idx;
        public int _layer_idx;
        public UIViewWrapper _extra_data;
    }

    internal class UIViewLayer
    {
        public string _name;
        public int _start_idx;
        public int _cap;// 节点数量
        public LinkedList<UIViewNode> _nodes;

        public int GetNewIndex()
        {
            if (_nodes.Count == 0)
                return _start_idx;

            return _nodes.Last.Value._node_idx + 1;
        }

        /// <summary>
        /// 添加之前, 检查是否可以添加
        /// 0: ok
        /// 1: 需要扩容
        /// 2: 需要重新排序
        /// </summary>
        public int CheckBeforeAdd()
        {
            //容量达到了上限, 需要扩容
            if (_nodes.Count >= _cap)
                return 1;

            if (_nodes.Count == 0)
                return 0;

            int new_index = _nodes.Last.Value._node_idx + 1;
            if ((new_index - _start_idx) >= _cap)
                return 2;

            return 0;
        }
    }    

}
