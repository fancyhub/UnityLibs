/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/18 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    public interface IFsmStateListener<TState, TMsg>
    {
        void OnFsmStart(TState state);
        void OnFsmChange(TState cur, TMsg msg, TState old);
        void OnFsmStop(TState state);
    }

    /// <summary>
    /// 没有 StateVirtualTable的 Fsm
    /// </summary>
    public class FsmNoVT<TState, TMsg, TStateTranMap> : IFsm<TState, TMsg, TMsg, TStateTranMap>
        where TStateTranMap : IFsmStateTranMap<TState, TMsg>
    {
        private const int C_MAX_MSG_PER_FRAME = 50;

        private TStateTranMap _StateTranMap;
        private IFsmStateListener<TState, TMsg> _Listener;
        private TState _State;
        private bool _InStack = false;
        private bool _Running = false;
        private EFsmMode _Mode;
        private LinkedList<TMsg> _MsgQueue;

        public int PtrVer { get; set; }

        public FsmNoVT(
            EFsmMode mode = EFsmMode.Sync,
            TStateTranMap state_tran_map = default,
            IFsmStateListener<TState, TMsg> listener = default)
        {
            _Mode = mode;
            _MsgQueue = new LinkedList<TMsg>();
            _StateTranMap = state_tran_map;
            _Listener = listener;
        }

        public TStateTranMap StateTranMap
        {
            get { return _StateTranMap; }
            set { _StateTranMap = value; }
        }

        public IFsmStateListener<TState, TMsg> Listener
        {
            get { return _Listener; }
            set { _Listener = value; }
        }

        public bool Start()
        {
            if (_Running)
                return false;
            if (!_StateTranMap.Start(out var state))
                return false;

            _Running = true;
            _State = state;
            _Listener?.OnFsmStart(_State);
            return true;
        }

        public bool Stop()
        {
            if (!_Running)
                return false;
            _Running = false;
            _MsgQueue.ExtClear();
            _Listener?.OnFsmStop(_State);
            return true;
        }

        public void SendMsg(TMsg msg)
        {
            if (!_Running)
                return;
            _MsgQueue.ExtAddLast(msg);
            if (_Mode != EFsmMode.Async)
                _ProcMsgs();
        }

        public void Destroy()
        {
            Stop();
            PtrVer++;
        }

        public bool IsRunning()
        {
            return _Running;
        }

        public bool TryGetState(out TState state)
        {
            state = _Running ? _State : default;
            return _Running;
        }

        public int ProcAllMsgs()
        {
            if (!_Running)
                return 0;

            //强制把 stack的标记位清除
            _InStack = false;

            if (_Mode == EFsmMode.Async)
                return _ProcMsgs();
            return 0;
        }

        private int _ProcMsgs()
        {
            //1. 先设置标记位
            if (_InStack)
                return 0;
            _InStack = true;

            //2. 开始循环处理
            int ret = 0;
            for (; ; )
            {
                if (ret > C_MAX_MSG_PER_FRAME)
                {
                    _InStack = false;
                    Log.Assert(false, "StateTran 一次处理的消息太多了,超过了 {0}", C_MAX_MSG_PER_FRAME);
                    return ret;
                }

                bool succ = _MsgQueue.ExtPopFirst(out TMsg result);
                if (!succ)
                    break;
                ret++;
                bool channged = _StateTranMap.Next(_State, result, out TState next);
                if (!channged)
                    continue;

                TState old_state = _State;
                _State = next;
                _Listener?.OnFsmChange(_State, result, old_state);
            }

            _InStack = false;
            return ret;
        }
    }

    /// <summary>
    /// 没有 StateVirtualTable的 Fsm
    /// </summary>
    public class FsmNoVT<TState, TMsg> : FsmNoVT<TState, TMsg, IFsmStateTranMap<TState, TMsg>>
    {
        public FsmNoVT(
            EFsmMode mode = EFsmMode.Sync,
            IFsmStateTranMap<TState, TMsg> state_tran_map = default,
            IFsmStateListener<TState, TMsg> listener = default)
            : base(mode, state_tran_map, listener)
        {
        }
    }
}