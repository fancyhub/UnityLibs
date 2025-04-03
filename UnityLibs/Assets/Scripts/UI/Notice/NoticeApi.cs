/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using FH;
namespace Game
{
    public sealed partial class NoticeApi
    {
        private static NoticeApi _;
        private NoticeManager _Mgr;
        private CPtr<IResInstHolder> _ResInstHolder;
        private IClock _Clock;

        private NoticeApi()
        {
            _ResInstHolder = ResMgr.CreateHolder(false, false).CPtr();
            _Clock = new ClockDecorator(new ClockUnityTime(ClockUnityTime.EType.Time));

            var res_ref = ResMgr.Load(NoticeConfigAsset.CPath);
            NoticeConfigAsset config = res_ref.Get<NoticeConfigAsset>();
            NoticeLog.SetMasks(config.LogLvl);
            _Mgr = new NoticeManager(_Clock);

            foreach (var p in config.Channels)
            {
                var channel = NoticeFactory.CreateChannel(p, _Clock, _ResInstHolder.Val);
                if (channel == null)
                    continue;
                _Mgr.AddChannel(p.ChannelType, channel);
            }
            _Mgr.InitVisionFlag();
        }
        public static NoticeApi Inst => _;

        public void ShowNotice(NoticeData data, INoticeItem item)
        {
            if (item == null)
                return;
            _Mgr.ShowNotice(data, item);
        }

        public IResInstHolder ResInstHolder => _ResInstHolder.Val;


        public static void Init()
        {
            if (_ != null)
                return;

            _ = new NoticeApi();
            FH.UI.UISceneMgr.AddUpdateForever(_._Mgr.Update);
        }
    }
}
