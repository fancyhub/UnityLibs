
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FH.UI;

namespace Game
{

    public partial class UIScrollBaseView // : FH.UI.UIBaseView 
    {
        public EUIScroll EUIScroll;
        public override void OnCreate()
        {
            base.OnCreate();
            EUIScroll = EUIScroll.Create(this._ScrollBase,  this.ResHolder);
        }

        public override void OnDestroy()
        {
            EUIScroll?.Destroy();
            EUIScroll = null;
            base.OnDestroy();
        }

        public Vector2 ViewSize
        {
            get => EUIScroll.ViewSize;
            set => EUIScroll.ViewSize = value;
        }

        public void SetLayout(IScrollerLayout layout)
        {
            EUIScroll.SetLayout(layout);
        }

        public void AddItem(IScrollerItem item)
        {
            EUIScroll.AddItem(item);
        }
        public void RemoveItem(IScrollerItem item)
        {
            EUIScroll.RemoveItem(item);
        }

        public void RemoveItem(IScrollerItem item, bool destroy_item)
        {
            EUIScroll.RemoveItem(item, destroy_item);
        }

        public virtual void ClearItems()
        {
            EUIScroll.ClearItems();
        }

        public List<IScrollerItem> GetItems()
        {
            return EUIScroll.GetItemList();
        }
      

        public void MoveToEnd()
        {
            EUIScroll.MoveToEnd();
        }

        public void MoveToHead()
        {
            EUIScroll.MoveToHead();
        }

        public void MoveItemToViewport(IScrollerItem item)
        {
            EUIScroll.MoveItemToViewport(item);
        }

        public void MoveItemToViewport(int item_index)
        {
            EUIScroll.MoveItemToViewport(item_index);
        }

        //view_percent 描述item在viewport区域的百分比位置，vertical情况下，0为最上方
        public void MoveToItem(int item_index, float view_percent)
        {
            EUIScroll.MoveToItem(item_index, view_percent);
        }

        public void MoveToItem(IScrollerItem item, float view_percent)
        {
            EUIScroll.MoveToItem(item, view_percent);
        }
    }

}
