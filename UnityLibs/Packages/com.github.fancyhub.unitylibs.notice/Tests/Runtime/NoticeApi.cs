/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.NoticeSample
{
    public static class NoticeApi
    {
        private static NoticeManager _Mgr;
        private static IResInstHolder _ResInstHolder;
        private static IClock _Clock;

        public static void Init()
        {
            if (_Mgr != null)
                return;

            if (_ResInstHolder == null)
                _ResInstHolder = ResMgr.CreateHolder(true, false);

            if (_Clock == null)
            {
                _Clock = new ClockUnityTime(ClockUnityTime.EType.Time);
                _Clock = new ClockDecorator(_Clock);
            }

            var res_ref = ResMgr.Load(NoticeConfigAsset.CPath);
            NoticeConfigAsset config = res_ref.Get<NoticeConfigAsset>();
            NoticeLog.SetMasks(config.LogLvl);

            _Mgr = new NoticeManager(_Clock);

            foreach (var p in config.Channels)
            {
                var channel = NoticeFactory.CreateChannel(p, _Clock, _ResInstHolder);
                if (channel == null)
                    continue;
                _Mgr.AddChannel(p.ChannelType, channel);
            }
            _Mgr.InitVisionFlag();

            MonoUpdater.CreateInst();
            MonoUpdater.ActionUpdate = _Mgr.Update;
        }

        public static void ShowNotice(NoticeData data)
        {
            _Mgr?.ShowNotice(data);
        }

        //跑马灯
        public static void ShowMarquee(string txt, float duration_sec = 5.0f)
        {
            if (_Mgr == null)
                return;

            NoticeData data = new NoticeData()
            {
                _channel = ENoticeChannel.Common,
                _duration_expire = -1,
                _duration_show = (int)(duration_sec * 1000),
                _item = new NoticeItemTextMarquee(txt)
            };
            _Mgr.ShowNotice(data);
        }

        public static void ShowCommon(string txt, float duration_sec = 2.0f)
        {
            if (_Mgr == null)
                return;

            NoticeData data = new NoticeData()
            {
                _channel = ENoticeChannel.Common,
                _duration_expire = -1,
                _duration_show = (int)(duration_sec * 1000),
                _item = new NoticeItemText(txt)
            };
            _Mgr.ShowNotice(data);
        }


        internal sealed class MonoUpdater : MonoBehaviour
        {
            public static Action ActionUpdate;

            private static MonoUpdater _Inst;

            public static void CreateInst()
            {
                if (_Inst == null)
                {
                    GameObject obj = new GameObject();
                    _Inst = obj.AddComponent<MonoUpdater>();
                    GameObject.DontDestroyOnLoad(obj);
                    obj.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            public void Awake()
            {
                _Inst = this;
            }

            public void Update()
            {
                if (ActionUpdate == null)
                    return;
                ActionUpdate();
            }

            public void OnDestroy()
            {
            }

            public void OnApplicationQuit()
            {
            }
        }
    }
}
