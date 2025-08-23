/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23 10:08:09
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;


namespace FH
{
    public interface INoticeChannelRoot
    {
        public GameObject CreateItemDummy();
        public void ReleaseItemDummy(GameObject obj);

        void Destroy();
    }

    public struct NoticeContainerContext
    {
        public INoticeChannelRoot Root;
        public NoticeDataQueue DataQueue;
        public IClock Clock;
        public bool Visible;
    }

    public interface INoticeContainer
    {
        public void OnUpdate(NoticeContainerContext context);
        public void OnVisibleChange(bool visible);
        public void OnClear();

        public void OnDestroy();
    }

    /// <summary>
    /// 具体的container的基类
    /// </summary>
    public sealed class NoticeChannel : INoticeChannel
    {
        public NoticeContainerContext _context;
        public NoticeChannelConfig _Config;

        public INoticeContainer _container;
        public NoticeVisibleCtrl _VisibleCtrl;

        public NoticeChannel(
            NoticeChannelConfig config,
            IClock clock,
            INoticeChannelRoot root,
            INoticeContainer container)
        {
            _Config = config;
            _VisibleCtrl = new NoticeVisibleCtrl(config.Visible);
            _context = new NoticeContainerContext()
            {
                DataQueue = new NoticeDataQueue(),
                Clock = new ClockDecorator(clock),
                Root = root,
                Visible = true,
            };
            _container = container;
        }

        public void Push(NoticeData data, INoticeItem item)
        {
            _context.DataQueue.Push(new NoticeData(data, item, _context.Clock.Time));
        }

        public void Update()
        {
            _container.OnUpdate(_context);
            _context.Clock.ScaleFloat = _Config.TimeScale.CalcScale(_context.DataQueue.Count);
        }

        public void Destroy()
        {
            _container.OnDestroy();
            _container = null;
            _context.Root.Destroy();
            _context.Root = null;
        }

        //当前是否可以显示
        public bool IsVisible()
        {
            return _context.Visible;
        }

        /// <summary>
        /// 隐藏所有的数据        
        /// 清空当前的显示队列，但是不会清空当前的等待队列
        /// 等待队列更新暂停
        /// </summary>
        public void SetVisibleFlag(ENoticeVisible flag)
        {
            bool visible = _VisibleCtrl.IsVisible(flag);
            if (_context.Visible == visible)
                return;
            _context.Visible = visible;
            _container.OnVisibleChange(visible);
        }

        public void RaiseClearSignal(ENoticeClearSignal signal)
        {
            if (!_VisibleCtrl.NeedClear(signal))
                return;

            Clear();
        }

        /// <summary>
        /// 清空所有的数据，包括显示数据和等待数据
        /// </summary>
        public void Clear()
        {
            _context.DataQueue.Clear();
            _container.OnClear();
        }
    }
}
