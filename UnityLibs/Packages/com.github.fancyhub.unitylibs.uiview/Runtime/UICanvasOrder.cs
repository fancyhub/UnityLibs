/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/8/8
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
    public class UICanvasOrder : UIBehaviour
    {
        [System.Serializable]
        public struct InnerElement
        {
            public Canvas _canvas;
            public Renderer _render;
            public int _order;

            public InnerElement(Canvas c)
            {
                _canvas = c;
                _render = null;
                _order = 0;
                if (c != null && c.overrideSorting)
                    _order = c.sortingOrder;
            }

            public InnerElement(Renderer render)
            {
                _canvas = null;
                _render = render;
                _order = render.sortingOrder;
            }

            public void SetOrder(int order)
            {
                if (_canvas != null)
                {
                    _canvas.overrideSorting = true;
                    _canvas.sortingOrder = order + _order;
                }

                if (_render != null)
                {
                    _render.sortingOrder = order + _order;
                }
            }
        }

        public int _order = 0;
        public bool _relative = false;
        public InnerElement[] _elements = null;
        public Canvas _canvas_self = null;


        protected override void OnCanvasHierarchyChanged()
        {
            if (_relative)
                _RefreshOrder(_elements, _GetParentOrder(_order));
            else
                _RefreshOrder(_elements, _order);
        }

        public void SetOrder(int order, bool relative)
        {
            if (_order == order && _relative == relative)
                return;
            _order = order;
            _relative = relative;

            if (_relative)
                _RefreshOrder(_elements, _GetParentOrder(_order));
            else
            {
                //要保证创建
                if (_canvas_self == null)
                {
                    _canvas_self = this.ExtGetComp<Canvas>(true);
                    this.ExtGetComp<GraphicRaycaster>();
                    _elements[0] = new InnerElement(_canvas_self);
                }
                _RefreshOrder(_elements, _order);
            }
        }

        public static UICanvasOrder Get(GameObject obj)
        {
            //1. 检查
            if (obj == null)
                return null;

            //2. 先判断是否已经添加过了
            UICanvasOrder ret = obj.GetComponent<UICanvasOrder>();
            if (ret != null)
                return ret;

            //3. 添加一个新的
            ret = obj.AddComponent<UICanvasOrder>();

            //4. 检查根节点上是否有 Canvas对象,如果没有,就默认创建一个, 一旦创建, 在推出UnityEditor的时候, Unity 编辑器Crash
            Canvas canvas_self = obj.GetComponent<Canvas>();
            ret._canvas_self = canvas_self;

            //5. 获取所有的canvas 组件 & 并创建 InnerElem
            List<Canvas> canvas_list = obj.ExtGetCompsInChildren<Canvas>(true);
            List<Renderer> renderer_list = obj.ExtGetCompsInChildren<Renderer>(true);
            int ele_index = 0;
            if (canvas_self == null)
            {
                ret._elements = new InnerElement[canvas_list.Count + renderer_list.Count + 1];
                ret._elements[0] = new InnerElement()
                {
                    _canvas = null,
                    _order = 0,
                    _render = null
                };
                ele_index++;
            }
            else
            {
                ret._elements = new InnerElement[canvas_list.Count + renderer_list.Count];
                int index = canvas_list.IndexOf(canvas_self);
                if (index > 0)
                {
                    //交换
                    Canvas t = canvas_list[0];
                    canvas_list[0] = canvas_self;
                    canvas_list[index] = t;
                }
                else if (index < 0)
                {
                    Log.Assert(false, "严重错误, 不能 在 list 找到 自己根节点的canvas");
                }
            }


            for (int i = 0; i < canvas_list.Count; ++i, ele_index++)
                ret._elements[ele_index] = new InnerElement(canvas_list[i]);
            for (int i = 0; i < renderer_list.Count; i++, ele_index++)
                ret._elements[ele_index] = new InnerElement(renderer_list[i]);
            ret._order = 0;

            //6. 重新调整 order
            if (ret._elements.Length > 0)
            {
                //6.1 获取order的范围
                int min_order = int.MaxValue;
                int max_order = int.MinValue;
                for (int i = 0; i < ret._elements.Length; ++i)
                {
                    int order = ret._elements[i]._order;
                    min_order = Math.Min(order, min_order);
                    max_order = Math.Max(order, max_order);
                }

                //6.2 检查范围是否合法
                int dt_order = max_order - min_order;

                //6.3 调整order
                if (min_order != 0)
                {
                    for (int i = 0; i < ret._elements.Length; i++)
                    {
                        ret._elements[i]._order = ret._elements[i]._order - min_order;
                        ret._elements[i].SetOrder(0);
                    }
                }
            }

            return ret;
        }

        private int _GetParentOrder(int offset)
        {
            Canvas parent_canvas = GetComponentInParent<Canvas>();
            if (parent_canvas == null)
                return offset;
            return parent_canvas.sortingOrder + offset;
        }

        private void _RefreshOrder(InnerElement[] elements, int order)
        {
            if (elements == null)
                return;

            for (int i = 0; i < elements.Length; ++i)
            {
                elements[i].SetOrder(order);
            }
        }
    }
}
