/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/23
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FH
{
    public interface IEffectHost : ICPtr
    {
        public Transform GetDummy();
    }

    public enum EEffectDummy
    {
        Local,
        World,
    }

    public struct MonoEffectInitParam
    {
        public IEffectHost Host;

        public EEffectDummy DummyType;
        public Transform LocalDummy;
        public Vector3 Offset;
        public Quaternion Rot;


        public float TimeElapsed;

        /// <summary>
        /// <=0 无结束时间
        /// </summary>
        public float TimeLength;
    }


    [DisallowMultipleComponent]
    public abstract class MonoEffect : MonoBehaviour, IDynamicComponent
    {
        private static GameObject _WorldDummy;
        protected CPtr<IEffectHost> _Host;
        protected EEffectDummy _DummyType;
        protected EState _State = EState.None;
        protected ClockUnityTimeScaleable _Clock;
        protected long _TimeLength; //MS
        protected long _TimeStop;

        public enum EState
        {
            None,
            Runing,
            Stopping,
            Stopped,
        }

        public static Transform WorldDummy
        {
            get
            {
                if (_WorldDummy == null)
                {
                    _WorldDummy = new GameObject("WorldEffectDummy");
                }
                return _WorldDummy.transform;
            }
        }

        public virtual void Play(MonoEffectInitParam init_param)
        {
            if (_State != EState.None)
            {
                Log.E(this, "Play Effect Error, State: {0}", _State);
                //Error
                return;
            }
            OnPrepare();
            _Clock = new ClockUnityTimeScaleable(ClockUnityTime.EType.Time, true);

            _Host = new CPtr<IEffectHost>(init_param.Host);
            _DummyType = init_param.DummyType;
            if (_DummyType == EEffectDummy.World)
            {
                transform.SetParent(WorldDummy, false);
                transform.SetLocalPositionAndRotation(init_param.Offset, init_param.Rot);
            }
            else
            {
                transform.SetParent(init_param.Host.GetDummy(), false);
                transform.SetLocalPositionAndRotation(init_param.Offset, init_param.Rot);
            }
            _State = EState.Runing;
            _TimeLength = (long)(init_param.TimeLength * 1000);
            _TimeStop = _TimeLength;
            OnPlay(init_param.TimeElapsed);
        }

        public virtual void ChangeToWorld()
        {
            if (_DummyType == EEffectDummy.World)
                return;
            if (this == null)
                return;
            _DummyType = EEffectDummy.World;
            transform.SetParent(WorldDummy, true);
        }

        //暂停和Resume 也用这个
        public virtual void SetScale(float scale)
        {
            _Clock.ScaleFloat = scale;
            OnScale(scale);
        }

        public virtual void Stop(float delay_time = 0, bool change_2_world = false)
        {
            if (change_2_world)
                ChangeToWorld();

            if (_State != EState.Runing)
                return;

            _State = EState.Stopping;
            if (delay_time <= float.Epsilon)
            {
                // 立刻结束
                _State = EState.Stopped;
            }
        }

        public abstract void LinkTo(Transform tar);

#if UNITY_EDITOR
        /// <summary>
        /// 编辑的时候, 收集组件用的
        /// </summary>
        [FH.Omi.Button]
        public abstract void EdCollect();
#endif

        protected abstract void OnPrepare();
        protected abstract void OnPlay(float time_elapsed);
        protected abstract void OnScale(float scale);

        void IDynamicComponent.OnDynamicRelease()
        {
            _State = EState.None;
        }
    }
}
