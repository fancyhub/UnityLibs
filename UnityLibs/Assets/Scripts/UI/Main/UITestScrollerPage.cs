using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;
using Unity.Collections;

namespace Game
{
    public class UITestScrollerPage : UIPageBase<UITestScrollerView>
    {
        private List<string> _data = new List<string>();
        private List<PageElemScrollDemoBase> _SubPages = new List<PageElemScrollDemoBase>();
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = this.UIClose;
            _data.Clear();
            for (int i = 1; i < 50; i++)
            {
                _data.Add(i.ToString());
            }

            _SubPages.Clear();
            _SubPages.Add(new ScrollDemoRow2(BaseView._scroll_row_2));
            _SubPages.Add(new ScrollDemoCenter(BaseView._scroll_center));
            _SubPages.Add(new ScrollDemoLoop(BaseView._scroll_loop));

            foreach (var p in _SubPages)
            {
                p.SetData(_data);
                p.SetItemClickCB(OnItemClick);
            }
        }

        private void OnItemClick(int index, long user_data)
        {
            foreach (var p in _SubPages)
                p.Select(index);
        }
    }

    public class PageElemScrollDemoBase
    {
        public UIScrollListView _view;
        public IUIScrollBinder _binder;
        public ScrollItemClickCB _ItemClickCallBack;

        public PageElemScrollDemoBase(UIScrollListView view)
        {
            _view = view;
            _binder = _view.CreateBinder<UITestScrollItemView, string>();
            _binder.SetItemClickCB(_on_item_click);
        }

        public virtual void SetData(List<string> data)
        {
            _view.SetData(data);
        }

        public virtual void Select(int index)
        {
            _binder.Select(index);
        }

        public void SetItemClickCB(ScrollItemClickCB cb)
        {
            _ItemClickCallBack = cb;
        }

        public void _on_item_click(int item_index, long user_data)
        {
            Debug.Log("OnClick");
            _ItemClickCallBack?.Invoke(item_index, user_data);
        }
    }

    public class ScrollDemoRow2 : PageElemScrollDemoBase
    {
        public FUIScrollAnim _scroll_anim;
        public ScrollDemoRow2(UIScrollListView view) : base(view)
        {
            var layout = new ScrollScrollLayoutVRow(2);
            layout.Spacing = 4;
            view.SetLayout(layout);
            _scroll_anim = new FUIScrollAnim(view.UIScroll);
        }

        public override void SetData(List<string> data)
        {
            base.SetData(data);
            _scroll_anim.Play(FUIScrollAnim.IN_DIR_LEFT_2_RIGHT, 0.5f, 0.1f, null, _anim_item_map);
        }

        private int _anim_item_map(int item_count, int index)
        {
            return FUIScrollAnim.RowMultiIndexMap(2, item_count, index);
        }
    }


    /// <summary>
    /// 一个剧中的scroll
    /// </summary>
    public class ScrollDemoCenter : PageElemScrollDemoBase
    {
        public FUIScrollCenterChild _center_child;
        public FUIScrollAnim _scroll_anim;

        public ScrollDemoCenter(UIScrollListView view) : base(view)
        {
            view.SetLayout(new ScrollLayoutCenterV(5));
            _scroll_anim = new FUIScrollAnim(view.UIScroll);

             _center_child = new FUIScrollCenterChild(view.UIScroll, 100, 0.5f, _on_select); ;
            
        }

        public override void SetData(List<string> data)
        {
            base.SetData(data);
            _scroll_anim.Play(FUIScrollAnim.IN_DIR_RIGHT_2_LEFT, 0.5f, 0.1f, null);
        }

        public override void Select(int index)
        {
            _view.Select(index);
            _center_child.MoveTo(index);
        }

        public void _on_select(int index)
        {
            _ItemClickCallBack?.Invoke(index, 0);
        }
    }

    /// <summary>
    /// 循环的scroll
    /// </summary>
    public class ScrollDemoLoop : PageElemScrollDemoBase
    {
        public ScrollDemoLoop(UIScrollListView view) : base(view)
        {
            view.SetLayout(new ScrollLayoutVLoop(20, true));
        }
    }
}
