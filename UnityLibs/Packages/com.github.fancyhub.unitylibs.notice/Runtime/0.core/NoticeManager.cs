/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;


namespace FH
{
    /// <summary>
    /// 整个notice系统的数据管理
    /// 
    /// 这边使用的是组合模式做的，这里没有任何创建数据的过程，
    /// 创建数据的部分全部扔到工厂类里面去做，这里只有移除和添加的操作
    /// 
    /// container这里是没有字典做映射的，只有一个list，container内部持有channel数据
    /// </summary>
    public class NoticeManager
    {
        public ENoticeVisibleFlag _vision_flag;
        public INoticeChannel[] _channels = new INoticeChannel[(int)ENoticeChannel.Max];
        public IClock _clock;

        public NoticeManager(IClock clock)
        {            
            _clock = clock;
            _vision_flag = 0;
        }

        public INoticeChannel GetChannel(ENoticeChannel channel_type)
        {
            int index = (int)channel_type;
            if (index < 0 || index >= _channels.Length)
            {
                NoticeLog.Assert(false, "channel not found :{0}", channel_type);
                return null;
            }
            var ret = _channels[index];
            NoticeLog.Assert(null != ret, "channel not found :{0}", channel_type);
            return ret;
        }

        public bool IsChannelVisible(ENoticeChannel channel_type)
        {
            INoticeChannel container = GetChannel(channel_type);
            if (null == container)
                return false;
            return container.IsVisible();
        }

        /// <summary>
        /// 显示 notice，数据是外部创建的
        /// </summary>
        public void ShowNotice(NoticeData data)
        {
            if (null == data._item)
            {
                NoticeLog.Assert(false, "data is null");
                return;
            }

            var container = GetChannel(data._channel);
            if (null == container)
            {
                NoticeLog.Assert(false, "channel 没有找到 {0}", data._channel);
                return;
            }
            container.Push(data);
        }

        public void AddChannel(ENoticeChannel channel_type, INoticeChannel channel)
        {
            _channels[(int)channel_type] = channel;
        }

        /// <summary>
        /// 这里面将当前时间也作为参数传到下一层
        /// </summary>
        public void Update()
        {
            for (int i = 0; i < _channels.Length; ++i)
            {
                var container = _channels[i];
                if (container != null)
                    container.Update();
            }
        }

        /// <summary>
        /// 初始化所有的显示标记为，
        /// 
        /// 因为构造函数被调用的时候还没有任何的container数据进来
        /// 而且如果调用标记的默认值是0，这种情况下如果调用了SetVisionFlag接口起不到任何作用
        /// 
        /// 只能将初始化和更新的接口拆开
        /// </summary>
        public void InitVisionFlag()
        {
            _vision_flag = ENoticeVisibleFlag.None;
            foreach (var p in _channels)
            {
                p?.SetVisibleFlag(_vision_flag);
            }
        }

        /// <summary>
        /// 设置可见
        /// </summary>        
        public void SetVisibleFlag(ENoticeVisibleFlag flag, bool v)
        {
            ENoticeVisibleFlag new_flag = _vision_flag;

            if (v) new_flag |= flag;
            else new_flag &= ~flag;

            if (_vision_flag == new_flag)
                return;

            foreach (var p in _channels)
            {
                p?.SetVisibleFlag(_vision_flag);
            }
        }

        /// <summary>
        /// 触发式的瞬时行为
        /// </summary>
        public void RaiseClearSignal(ENoticeClearSignal signal)
        {
            for (int i = 0; i < (int)ENoticeChannel.Max; ++i)
            {
                //var visible_ctrl = _visible_ctrls[i];
                //if (null == visible_ctrl) continue;

                //if (!visible_ctrl.NeedClear(signal))
                //    continue;

                //_data_queues[i].Clear();
                //_channels[i].Clear();
            }
        }

        public void Destroy()
        {
            for (int i = 0; i < (int)ENoticeChannel.Max; ++i)
            {
                _channels[i]?.Destroy();
                _channels[i] = null;
            }            
        }

        public void ClearChannel(ENoticeChannel channel)
        {
            if (channel != ENoticeChannel.Max)
            {
                _channels[(int)channel].Clear();
            }
        }
    }
}
