
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
        public FUIScroll UIScroll;
        public override void OnCreate()
        {
            base.OnCreate();
            UIScroll = FUIScroll.Create(this._ScrollBase,  this.ResHolder);
        }

        public override void OnDestroy()
        {
            UIScroll?.Destroy();
            UIScroll = null;
            base.OnDestroy();
        }

        public Vector2 ViewSize
        {
            get => UIScroll.ViewSize;
            set => UIScroll.ViewSize = value;
        }

        public void SetLayout(IScrollLayout layout)
        {
            UIScroll.SetLayout(layout);
        }

        public void AddItem(IScrollItem item)
        {
            UIScroll.AddItem(item);
        }
        public void RemoveItem(IScrollItem item)
        {
            UIScroll.RemoveItem(item);
        }

        public void RemoveItem(IScrollItem item, bool destroy_item)
        {
            UIScroll.RemoveItem(item, destroy_item);
        }

        public virtual void ClearItems()
        {
            UIScroll.ClearItems();
        }

        public List<IScrollItem> GetItems()
        {
            return UIScroll.GetItemList();
        }
      

        public void MoveToEnd()
        {
            UIScroll.MoveToEnd();
        }

        public void MoveToHead()
        {
            UIScroll.MoveToHead();
        }

        public void MoveItemToViewport(IScrollItem item)
        {
            UIScroll.MoveItemToViewport(item);
        }

        public void MoveItemToViewport(int item_index)
        {
            UIScroll.MoveItemToViewport(item_index);
        }

        //view_percent 描述item在viewport区域的百分比位置，vertical情况下，0为最上方
        public void MoveToItem(int item_index, float view_percent)
        {
            UIScroll.MoveToItem(item_index, view_percent);
        }

        public void MoveToItem(IScrollItem item, float view_percent)
        {
            UIScroll.MoveToItem(item, view_percent);
        }
    }

}
