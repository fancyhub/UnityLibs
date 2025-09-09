/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/11/12
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
  

    public interface IScrollListItem : IScrollItem
    {
        UIBaseView View { get; }
        int ItemIndex { get; set; }
        Type ViewType { get; set; }
        void SetClickCB(Action<int, long> cb);
    }

    public interface IScrollListItem<TData> : IScrollListItem
    {
        bool Selected { get; set; }
        void SetData(TData data);
        TData GetData();

        //创建的时候出发
        void SetViewCreateCB(Action<TData, UIBaseView> cb);
        void Clear();
    }

    public class ScrollListItemConfig
    {
        //该类型是固定大小
        public bool IsFixedSize = true;
        public RefPrimitive<Vector2> FixedSize = null;
        public RefPrimitive<Vector2> FixedSelectedSize = null;

        //默认的 Dynamic Size, 如果为 null, 说明都要自己算一遍, 如果不为空, 只有等到item 可见了再计算
        // 可以看逻辑
        public RefPrimitive<Vector2> DefaultDynamicSize = new Vector2(1, 1);
    }

    public abstract class ScrollListItem : ScrollItemBase, IScrollListItem
    {
        public static Dictionary<Type, ScrollListItemConfig> _configs = new Dictionary<Type, ScrollListItemConfig>();

        protected bool _selected = false; //默认false
        public Action<int, long> _click_cb;

        public int ItemIndex { get; set; }
        public Type ViewType { get; set; }

        public virtual UIBaseView View { get; protected set; }

        public void SetClickCB(Action<int, long> click_cb)
        {
            _click_cb = click_cb;
        }

        public static ScrollListItemConfig GetConfig<T>()
            where T : IUIView
        {
            return GetConfig(typeof(T));
        }

        public static ScrollListItemConfig GetConfig(Type t)
        {
            if (t == null)
                return null;

            ScrollListItemConfig ret = null;
            _configs.TryGetValue(t, out ret);
            if (ret == null)
            {
                ret = new ScrollListItemConfig();
                _configs.Add(t, ret);
            }
            return ret;
        }

        public ScrollListItemConfig GetConfig()
        {
            return GetConfig(ViewType);
        }
    }

    public abstract class ScrollListItem<TData> : ScrollListItem, IScrollListItem<TData>
    {
        public const float C_SIZE_EPSILON = 0.1f;


        public TData _data;
        //view size
        public RefPrimitive<Vector2> _view_size;
        public bool _culling_visible = false;
        public IVoSetter<TData> _view_vo_setter;
        protected IUISelectable _view_selectable;

        //这个参数可以设置，当 UIView 被创建的时候触发
        public Action<TData, UIBaseView> _view_create_cb;

        public ScrollListItem(Type view_type)
        {
            ViewType = view_type;
        }

        public virtual void SetData(TData data)
        {
            _data = data;
            if (_view_vo_setter == null)
                return;
            _view_vo_setter.SetData(_data);
            if (!_update_size(View, Selected, ref _view_size))
                return;
            _parent.OnChildSizeChange();
        }

        public void Clear()
        {
            //清空之前的缓存状态，避免在scroll item复用的时候，出现问题
            Selected = false;
        }

        public TData GetData()
        {
            return _data;
        }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                if (_selected == value)
                    return;

                _selected = value;

                if (_view_selectable != null)
                {
                    _view_selectable.Selected = _selected;
                    _update_size(View, Selected, ref _view_size);
                    OnPosChange();
                }
                return;
            }
        }

        public override Vector2 Size
        {
            get
            {
                ScrollListItemConfig config = GetConfig();

                //1. 如果是固定大小的
                if (config.IsFixedSize)
                {
                    RefPrimitive<Vector2> fixed_size = Selected ? config.FixedSelectedSize : config.FixedSize;
                    if (fixed_size == null)
                    {
                        CullVisible = true;
                        fixed_size = Selected ? config.FixedSelectedSize : config.FixedSize;
                        Log.Assert(fixed_size != null, "选中模式下{0}, 强制修改 CullingVisible 也创建失败", Selected);
                    }

                    if (fixed_size == null)
                        return new Vector2(1, 1);
                    return fixed_size;
                }

                //2. 非固定大小的
                if (_view_size != null)  //如果该 view 已经计算过了, 就直接返回
                    return _view_size;

                //如果不为空, 在item未计算大小之前, 都返回该默认值
                if (config.DefaultDynamicSize != null)
                    return config.DefaultDynamicSize;

                //如果默认值为空, 说明必须要自己计算一次
                CullVisible = true;
                if (_view_size == null)
                    return new Vector2(1, 1);
                return _view_size;
            }
        }

        public override bool CullVisible
        {
            get { return _culling_visible; }
            set
            {
                if (_culling_visible == value)
                    return;

                _culling_visible = value;

                if (!_culling_visible)
                {
                    DestroyView();
                    return;
                }

                if (null != View)
                    return;

                //加完 之后，scroller 会触发重新排序
                Log.Assert(_parent != null, "parent is null");

                View = CreateView(_parent.ItemParent, _parent.Holder);
                _view_vo_setter = View as IVoSetter<TData>;
                _view_selectable = View as IUISelectable;

                if (null != _view_selectable)
                {
                    _view_selectable.SetBtnClickCb(_on_btn_click);
                    _view_selectable.Selected = _selected;
                }
                _view_vo_setter?.SetData(_data);

                _update_size(View, Selected, ref _view_size);
                OnPosChange();
                _view_create_cb?.Invoke(_data, View);
            }
        }


        public void SetViewCreateCB(Action<TData, UIBaseView> cb)
        {
            _view_create_cb = cb;
        }

        public override void Destroy()
        {
            _parent = null;
            DestroyView();
        }

        public override void SetParent(IScrollItemParent parent)
        {
            if (null == parent)
            {
                //my.info("parent is null");
            }

            _parent = parent;
        }

        protected override void OnPosChange()
        {
            if (null == View)
                return;

            Vector3 local_pos = CalcLocalPos(FinalPos, Size, View.GetPivot());
            View.SetLocalPos(local_pos);
        }

        public virtual void DestroyView()
        {
            _view_vo_setter = null;
            _view_selectable = null;

            var view = View;
            if (view == null)
                return;
            view.Destroy();
            View = null;
        }

        public abstract UIBaseView CreateView(Transform dummy, IResInstHolder holder);

        public void _on_btn_click(IUISelectable b, long user_data)
        {
            _click_cb?.Invoke(ItemIndex, user_data);
        }

        public static bool _update_size(UIBaseView view, bool selected, ref RefPrimitive<Vector2> view_size)
        {
            if (view == null)
                return false;

            //如果是固定大小的
            Type t = view.GetType();
            var config = GetConfig(t);

            if (config.IsFixedSize)
            {
                if (!selected)
                {
                    //已经设置过大小了，就不要再设置大小了，返回false，未改变
                    if (config.FixedSize != null)
                        return false;

                    config.FixedSize = view.GetViewSize();
                    return true;
                }
                else
                {
                    //已经设置过大小了，就不要再设置大小了，返回false，未改变
                    if (config.FixedSelectedSize != null)
                        return false;

                    config.FixedSelectedSize = view.GetViewSize();
                    return true;
                }
            }

            //非固定大小
            Vector2 size = view.GetViewSize();
            if (view_size == null)
            {
                view_size = size;
                return true;
            }

            Vector2 old_view_size = view_size;
            //判断是否有变化
            if (Mathf.Abs(size.x - old_view_size.x) < C_SIZE_EPSILON
                && Mathf.Abs(size.y - old_view_size.y) < C_SIZE_EPSILON)
                return false;

            view_size.Value = size;
            return true;
        }
    }


    public sealed class ScrollListItem<TView, TData> : ScrollListItem<TData> where TView : UIBaseView, IVoSetter<TData>, new()
    {
        public ScrollListItem() : base(typeof(TView))
        {
        }

        public override UIBaseView CreateView(Transform dummy, IResInstHolder holder)
        {
            return UIBaseView.CreateView<TView>(dummy, holder);
        }
    }
}
