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
    public static partial class NoticeApi
    {
        private static NoticeManager _Mgr;
        private static IResHolder _ResHolder;
        private static IClock _Clock;


        public static void Init()
        {
            if (_Mgr != null)
                return;

            if (_ResHolder == null)
                _ResHolder = ResMgr.CreateHolder(true, false);

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
                var channel = NoticeFactory.CreateChannel(p, _Clock, _ResHolder);
                if (channel == null)
                    continue;
                _Mgr.AddChannel(p.ChannelType, channel);
            }
            _Mgr.InitVisionFlag();

            MonoUpdater.CreateInst();
            MonoUpdater.ActionUpdate = _Mgr.Update;
        }

        public static IResHolder ResHolder => _ResHolder;   

        internal sealed class MonoUpdater : MonoBehaviour
        {
            public static Action<float> ActionUpdate;

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
                ActionUpdate(Time.deltaTime);
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
