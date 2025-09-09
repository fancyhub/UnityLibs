/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/30
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
    /// <summary>
    /// Scroll 的容器类
    /// 1. 添加/删除 Item
    /// 2. 获取/设置 ViewPort的size
    /// 3. 获取/设置 Content Size
    /// 4. 获取/设置 Content Pos
    /// </summary>
    public class UIScroller : IScroller, IScrollerItemParent
    {
        public int ObjVersion { get; private set; }


        public struct ScrollerDummy
        {
            public RectTransform _Tran;

            public RectTransform Get(ScrollRect scroll_rect)
            {
                if (_Tran == null)
                {
                    GameObject obj = new GameObject("");
                    obj.layer = scroll_rect.gameObject.layer;
                    obj.transform.SetParent(scroll_rect.content, false);
                    _Tran = obj.AddComponent<RectTransform>();
                    _Tran.pivot = Vector2.one * 0.5f;
                    _Tran.anchorMin = new Vector2(0, 1);
                    _Tran.anchorMax = new Vector2(0, 1);
                    _Tran.localPosition = Vector3.zero;
                    _Tran.sizeDelta = Vector2.zero;
                }
                return _Tran;
            }

            public void Destroy()
            {
                if (_Tran == null)
                    return;
                GameObject.Destroy(_Tran);
                _Tran = null;
            }
        }

        private IResInstHolder _holder;
        private ScrollRect _unity_scroll;
        private RectTransform _view_port_tran;
        private RectTransform _content_tran;
        private ScrollerDummy _dummy;

        private Vector2 _all_item_size = Vector2.zero;
        private IScrollerEvent _evt;
        private List<IScrollerItem> _item_list = new List<IScrollerItem>();
        private int _batch_mode = 0;

        public UIScroller(ScrollRect scroll_rect, IResInstHolder holder)
        {
            _holder = holder;

            _unity_scroll = scroll_rect;
            _view_port_tran = _unity_scroll.viewport;
            _content_tran = _unity_scroll.content;

            ContentPos = Vector2.zero;
            ContentSize = Vector2.zero;

            _unity_scroll.onValueChanged.AddListener(_on_scroll_move);
            UIScrollEventBehaviour scroll_evt = _unity_scroll.ExtGetComp<UIScrollEventBehaviour>(true);
            scroll_evt.EvtOnDragStart = _on_drag_start;
            scroll_evt.EvtOnDragEnd = _on_drag_end;
            scroll_evt.EvtOnScrollMoveEnd = _on_move_end;
            scroll_evt.EvtUpdate = _on_update;
        }


        public IScrollerEvent ScrollEvent { get { return _evt; } set { _evt = value; } }

        public ScrollRect UnityScroll { get { return _unity_scroll; } }

        #region 
        public Vector2 ViewSize
        {
            get
            {
                Vector2 size = _view_port_tran.rect.size;
                return size;
            }

            set
            {
                //1. 获取size, 不能变成负数
                Vector2 new_size = value;
                new_size.x = Mathf.Max(0, new_size.x);
                new_size.y = Mathf.Max(0, new_size.y);

                //2. 比较,是否发生变化
                Vector2 now_size = ViewSize;
                if (now_size.Equals(new_size))
                    return;

                //3. 设置view size
                _view_port_tran.sizeDelta = new_size;

                //4. 调整 container size
                _update_content_size();

                //5. 发出事件, view size 发生变化了
                _evt?.OnScrollViewSizeChange(now_size - new_size);
            }
        }


        /// <summary>
        /// ContentSize = Max(ViewSize,AllItemSize)
        /// </summary>
        public Vector2 ContentSize
        {
            set
            {
                Vector2 new_all_item_size = Vector2.Max(value, Vector2.zero);
                if (_all_item_size.Equals(new_all_item_size))
                    return;
                _all_item_size = new_all_item_size;
                _update_content_size();
            }
            get
            {
                return _content_tran.rect.size;
            }
        }

        public Vector2 ContentPos
        {
            get
            {
                Vector2 ret = _content_tran.localPosition;
                ret.x = -ret.x;
                return ret;
            }
            set
            {
                Vector3 pos = value;
                pos.z = 0;
                pos.x = -pos.x;
                _content_tran.localPosition = pos;
            }
        }

        public Vector2 ContentPosMax
        {
            get
            {
                //根据 容器的大小来计算,而不是根据 内容的大小
                Vector2 cont_size = ContentSize;
                Vector2 view_size = ViewSize;

                Vector2 ret = cont_size - view_size;
                ret.x = Mathf.Max(ret.x, 0.0001f);
                ret.y = Mathf.Max(ret.y, 0.0001f);

                return ret;
            }
        }

        private void _update_content_size()
        {
            //1. 获取view 的size
            Vector2 new_content_size = Vector2.Max(ViewSize, _all_item_size);
            Vector2 old_content_size = _content_tran.rect.size;

            //4. 比较content size 是否发生了变化
            if (new_content_size.Equals(old_content_size))
                return;

            //5. 调整content size
            _content_tran.sizeDelta = new_content_size;
        }
        #endregion


        #region Movement

        public void StopMovement()
        {
            _unity_scroll.StopMovement();
        }

        private void _on_scroll_move(Vector2 pos)
        {
            _evt?.OnScrollMoving();
        }

        private void _on_drag_start()
        {
            _evt?.OnScrollDragStart();
        }

        //这里是drag end
        private void _on_drag_end()
        {
            _evt?.OnScrollDragEnd();
        }

        private void _on_move_end()
        {
            _evt?.OnScrollMoveEnd();
        }

        private void _on_update()
        {
            _evt?.OnScrollUpdate();
        }

        #endregion


        #region Items
        public int BeginBatch()
        {
            //Debug.Log("Batch Begin");
            _batch_mode++;
            return _batch_mode;
        }

        public void AddItem(IScrollerItem item)
        {
            Log.Assert(!_item_list.Contains(item), "item list duplicate item !!!");
            _item_list.Add(item);
            item.SetParent(this);

            _item_changed();
        }

        /// <summary>
        /// 只有找到了之后才会destroy item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="destroy_item"></param>
        public void RemoveItem(IScrollerItem item, bool destroy_item)
        {
            if (!_item_list.Remove(item))
                return;

            item.SetParent(null);
            if (destroy_item)
                item.Destroy();

            _item_changed();
        }

        public List<IScrollerItem> GetItemList()
        {
            return _item_list;
        }

        public void ClearItems()
        {
            _clear_items();

            _item_changed();
        }

        public int EndBatch()
        {
            //Debug.Log("Batch End");
            if (_batch_mode <= 0)
                return _batch_mode;
            _batch_mode--;
            if (_batch_mode > 0)
                return _batch_mode;
            _batch_mode = 0;

            _item_changed();
            return _batch_mode;
        }

        private void _clear_items()
        {
            foreach (var a in _item_list)
            {
                a.SetParent(null);
                a.Destroy();
            }

            _item_list.Clear();
        }

        private void _item_changed()
        {
            if (_batch_mode > 0)
                return;

            _evt?.OnScrollItemChange();
        }
#endregion


        public void Destroy()
        {
            _clear_items();
            if (_unity_scroll != null)
                _unity_scroll.onValueChanged.RemoveAllListeners();


            _dummy.Destroy();
            ObjVersion++;

        }

        #region IScrollItemParent
        RectTransform IScrollerItemParent.ItemParent
        {
            get
            {
                //return _dummy.Get(_unity_scroll);
                return _content_tran;
            }
        }

        IResInstHolder IScrollerItemParent.Holder
        {
            get { return _holder; }
        }

        void IScrollerItemParent.OnChildSizeChange()
        {
            _item_changed();
        }
        #endregion

    }


    [RequireComponent(typeof(ScrollRect))]
    public class UIScrollEventBehaviour : MonoBehaviour, IBeginDragHandler, IEndDragHandler
    {
        public const float C_THRESHOLD = 1.0f;
        public Action EvtOnDragStart;
        public Action EvtOnDragEnd;
        public Action EvtOnScrollMoveEnd;
        public Action EvtUpdate;

        public float _threshold = C_THRESHOLD;

        public enum EStatus
        {
            Stop,
            Scrolling,
            Moving,
        }

        public EStatus _status = EStatus.Stop;
        private ScrollRect _scroll_rect;
        private ScrollRect ScrollRect
        {
            get
            {
                if (_scroll_rect == null)
                    _scroll_rect = GetComponent<ScrollRect>();
                return _scroll_rect;
            }
        }

        public static UIScrollEventBehaviour Get(ScrollRect rect)
        {
            return rect.ExtGetComp<UIScrollEventBehaviour>(true);
        }

        protected void Awake()
        {
            _status = EStatus.Stop;
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!ScrollRect.IsActive())
                return;
            _status = EStatus.Scrolling;
            EvtOnDragStart?.Invoke();
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _status = EStatus.Moving;
            EvtOnDragEnd?.Invoke();
        }

        protected void OnDisable()
        {
            _status = EStatus.Stop;
        }

        protected void Update()
        {
            _UpdateMovement();
            EvtUpdate?.Invoke();
        }

        private void _UpdateMovement()
        {
            if (_status != EStatus.Moving)
                return;

            ScrollRect scroll = ScrollRect;
            if (!_is_velocity_zero(scroll.velocity, _threshold))
                return;
            scroll.StopMovement();
            _status = EStatus.Stop;
            EvtOnScrollMoveEnd?.Invoke();
        }

        private static bool _is_velocity_zero(Vector2 velocity, float threshold)
        {
            if (Math.Abs(velocity.x) < threshold && Mathf.Abs(velocity.y) < threshold)
                return true;
            return false;
        }
    }
}
