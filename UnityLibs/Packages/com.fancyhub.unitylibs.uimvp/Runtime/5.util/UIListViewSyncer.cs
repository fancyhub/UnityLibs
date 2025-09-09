/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/9/5
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    public interface IDataSetter<T>
    {
        public void SetData(T data);
    }

    public interface IListDataSetter<T> : IDataSetter<IList<T>>
    {
        public void Clear();
    }

    public interface IListViewSyncerViewer<TData>: IDataSetter<TData>, IDestroyable
    {
    }

    public class UIListViewSyncer<TData> : IListDataSetter<TData> 
    {
        private Func<IListViewSyncerViewer<TData>> _ViewCreator;
        private List<IListViewSyncerViewer<TData>> _ViewList = new List<IListViewSyncerViewer<TData>>();

        public UIListViewSyncer(Func<IListViewSyncerViewer<TData>> view_creator)
        {
            _ViewCreator = view_creator;
        }

        public void SetData(IList<TData> dataList)
        {

            //1. 删除多余的
            int dataCount = 0;
            if (dataList != null)
                dataCount = dataList.Count;

            for (int i = _ViewList.Count - 1; i >= dataCount; i--)
            {
                _ViewList[i].Destroy();
                _ViewList.RemoveAt(i);
            }

            //2. 创建新的
            for (int i = _ViewList.Count; i < dataCount; i++)
            {
                _ViewList.Add(_ViewCreator());
            }

            //3. 数据同步
            for (int i = 0; i < dataCount; i++)
            {
                _ViewList[i].SetData(dataList[i]);
            }

        }

        public void Clear()
        {
            SetData(null);
        }
    }

}
