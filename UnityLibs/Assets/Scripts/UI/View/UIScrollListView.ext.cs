
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FH;
using FH.UI;

namespace Game
{

    public partial class UIScrollListView // : UIScrollBaseView 
    {
        public IUIScrollBinder _binder;

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public IUIScrollBinder<TData> CreateBinder<TView, TData>() 
            where TView : UIBaseView, IVoSetter<TData>, new()
        {
            if (_binder != null)
            {
                Log.Assert(false, "不支持创建多次");
                return null;
            }
            IUIScrollBinder<TData> binder = new FUIScrollBinder<TData>(UIScroll, new ScrollListItemSingleFactory<TView, TData>());
            _binder = binder;
            return binder;
        }

        public IUIScrollBinder<TData> CreateBinder<TData>(IUIScrollListItemFactory<TData> factory)
        {
            if (_binder != null)
            {
                Log.Assert(false, "不支持创建多次");
                return null;
            }

            IUIScrollBinder<TData> binder = new FUIScrollBinder<TData>(UIScroll, factory);
            _binder = binder;
            return binder;
        }

        public void SetItemClickCB(ScrollItemClickCB click_cb)
        {
            if (_binder == null)
            {
                Log.Assert(false, "还没有创建binder");
                return;
            }
            _binder.SetItemClickCB(click_cb);
        }

        public void SetData(IList data)
        {
            if (_binder == null)
            {
                Log.Assert(false, "还没有创建binder");
                return;
            }

            _binder.SetData(data);
        }

        public void Select(int item_index)
        {
            if (_binder == null)
            {
                Log.Assert(false, "还没有创建binder");
                return;
            }
            _binder.Select(item_index);
        }

        public override void OnDestroy()
        {
            ClearItems();
            _binder?.Destroy();
            base.OnDestroy();
        }

        public override void ClearItems()
        {
            _binder?.SetData(null);
            UIScroll.ClearItems();
        }
    }

}
