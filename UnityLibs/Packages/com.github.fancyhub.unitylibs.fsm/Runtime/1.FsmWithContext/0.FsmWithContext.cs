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
    //状态的虚表结构
    public interface IFsmStateVTWithContext<TContext, TState, TMsg, TResult> : ICPtr
    {
        /// <summary>
        /// 状态机启动的时候调用
        /// </summary>
        bool OnFsmStart(TContext context, TState state);

        EFsmProcResult OnFsmMsg(TContext context, TState state, TMsg msg, out TResult result);

        void OnFsmChange(TContext context, TState state, TState old_state);

        /// <summary>
        /// 状态机销毁的时候调用
        /// </summary>
        bool OnFsmStop(TContext context, TState state);
    }

    public class FsmWithContext<TContext, TState, TMsg, TResult, TStateTranMap> : IFsm<TState, TMsg, TResult, TStateTranMap>
        where TStateTranMap : IFsmStateTranMap<TState, TResult>
    {
        private const int C_MAX_MSG_PER_FRAME = 50;

        //下面都是外部要传入变量, FSM 不负责销毁, 在创建的时候, 可以挂到 PtrList上, 等到Fsm销毁的时候一起销毁
        //如果多份Fsm共用, 就不要销毁了
        private TStateTranMap _StateTranMap;

        private IFsmStateVTWithContext<TContext, TState, TMsg, TResult> _StateVT;
        private TContext _Context;
        private EFsmMode _Mode;
        private TagLog _Logger = TagLog.Create("Fsm", ELogLvl.Info);


        //下面是内部的
        private LinkedList<TMsg> _MsgQueue;
        private bool _running;
        private TState _State;
        private bool _InStack;

        public PtrList PtrList;

        public int PtrVer { get; private set; }

        /// <summary>
        /// 是否为异步模式
        /// </summary>        
        public FsmWithContext(EFsmMode mode)
        {
            _MsgQueue = new LinkedList<TMsg>();
            _running = false;
            _InStack = false;
            _Mode = mode;
        }

        /// <summary>
        /// 迁移表
        /// </summary>
        public TStateTranMap StateTranMap { get { return _StateTranMap; } set { _StateTranMap = value; } }

        /// <summary>
        /// 状态虚表
        /// </summary>
        public IFsmStateVTWithContext<TContext, TState, TMsg, TResult> StateVT { get { return _StateVT; } set { _StateVT = value; } }

        public TagLog Logger { set { _Logger = value; } }

        /// <summary>
        /// 上下文
        /// </summary>
        public TContext Context { get { return _Context; } set { _Context = value; } }

        public virtual void Destroy()
        {
            Stop();
            PtrList?.Destroy();
            PtrList = null;
            PtrVer++;
        }

        public bool IsRunning()
        {
            return _running;
        }

        public bool Start()
        {
            if (_running)
                return false;
            if (!_StateTranMap.Start(out var state))
                return false;

            bool ret = _StateVT.OnFsmStart(_Context, state);
            if (!ret)
                return false;

            _running = true;
            _State = state;
            return true;
        }

        public bool Stop()
        {
            if (!_running)
                return false;
            _running = false;

            _StateVT.OnFsmStop(_Context, _State);
            _MsgQueue.ExtClear();
            return true;
        }

        public bool TryGetState(out TState state)
        {
            state = _running ? _State : default;
            return _running;
        }

        public void SendMsg(TMsg msg)
        {
            if (!_running)
                return;

            _MsgQueue.ExtAddLast(msg);

            if (_Mode != EFsmMode.Async)
                _ProcMsgs();
        }

        public int ProcAllMsgs()
        {
            if (!_running)
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
                    _Logger.Assert(false, "FSM 一次处理的消息太多了,超过了 {0}", C_MAX_MSG_PER_FRAME);
                    return ret;
                }

                //2.1 获取消息队列里面的第一个消息,并弹出
                bool succ = _MsgQueue.ExtPopFirst(out TMsg msg);
                if (!succ)
                    break;

                //2.2 处理消息, 每个state node 需要返回是否产生Result
                ret++;
                EFsmProcResult has_result = _StateVT.OnFsmMsg(_Context, _State, msg, out TResult result);
                if (has_result != EFsmProcResult.Channged)
                    continue;

                //2.3 到状态迁移表里面, 根据 当前状态 + 结果 -> 找到下一个状态
                bool changed = _StateTranMap.Next(_State, result, out TState next_state);
                if (!changed)
                    continue;

                //2.4 切换状态
                TState old_state = _State;
                _State = next_state;
                //备注: 如果想要连续切换状态, 可以在context 里面把 fsm加进去,在node的OnEnter里面给自己发消息
                // 不过这种做法有一定的风险, 因为是异步的,你发送的消息,是在消息队列的末尾
                _StateVT.OnFsmChange(_Context, _State, old_state);
            }
            _InStack = false;
            return ret;
        }
    }

    public class FsmWithContext<TContext, TState, TMsg, TResult>
     : FsmWithContext<TContext, TState, TMsg, TResult, IFsmStateTranMap<TState, TResult>>
    {
        public FsmWithContext(EFsmMode mode) : base(mode) { }
    }
}