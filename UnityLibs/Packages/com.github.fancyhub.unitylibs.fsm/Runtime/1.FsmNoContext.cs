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
    public interface IFsmStateVT<TState, TMsg, TResult> : ICPtr
    {
        /// <summary>
        /// 状态机启动的时候调用
        /// </summary>
        bool OnFsmStart(TState state);

        EFsmProcResult OnFsmMsg(TState state, TMsg msg, out TResult result);

        void OnFsmChange(TState state, TState old_state);

        /// <summary>
        /// 状态机销毁的时候调用
        /// </summary>
        bool OnFsmStop(TState state);
    }

    public interface IFsmStateNode<TMsg, TResult> : IDestroyable
    {
        void OnEnter();
        /// <summary>
        /// 参见 IFsmStateVT.ProcMsg 的说明<para/>
        /// 因为 IFsmStateNode 对于fsm不是必须的,也是不可见的, 但是 IFsmStateVT 必须要有<para/>
        /// IFsmStateVT的一种实现,是用switch来做,不需要IFsmStateNode<para/>
        /// </summary>
        EFsmProcResult OnMsg(TMsg msg, out TResult result);

        void OnExit();
    }

    public class FsmNoContext<TState, TMsg, TResult, TStateTranMap> : IFsm<TState, TMsg, TResult, TStateTranMap>
        where TStateTranMap : IFsmStateTranMap<TState, TResult>
    {
        public const int C_MAX_MSG_PER_FRAME = 50;

        //下面都是外部要传入变量, FSM 不负责销毁
        private TStateTranMap _StateTranMap;
        private IFsmStateVT<TState, TMsg, TResult> _StateVt;
        private EFsmMode _Mode;

        //下面是内部的
        private LinkedList<TMsg> _MsgQueue;
        private bool _Running;
        private TState _State;
        private bool _InStack;
        public PtrList PtrList;

        public int PtrVer { get; private set; }

        /// <summary>
        /// 是否为异步模式
        /// </summary>        
        public FsmNoContext(EFsmMode mode)
        {
            _MsgQueue = new LinkedList<TMsg>();
            _Running = false;
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
        public IFsmStateVT<TState, TMsg, TResult> StateVT { get { return _StateVt; } set { _StateVt = value; } }


        public virtual void Destroy()
        {
            Stop();
            PtrList?.Destroy();
            PtrList = null;
            PtrVer++;
        }

        public bool IsRunning()
        {
            return _Running;
        }

        public bool Start()
        {
            if (_Running)
                return false;
            bool succ = _StateTranMap.Start(out var state);
            if (!succ)
                return false;

            bool ret = _StateVt.OnFsmStart(state);
            if (!ret)
                return false;

            _Running = true;
            _State = state;
            return true;
        }

        public bool Stop()
        {
            if (!_Running)
                return false;
            _Running = false;

            _StateVt.OnFsmStop(_State);
            _MsgQueue.ExtClear();
            return true;
        }

        public bool TryGetState(out TState state)
        {
            state = _Running ? _State : default;
            return _Running;
        }

        public void SendMsg(TMsg msg)
        {
            if (!_Running)
                return;

            _MsgQueue.ExtAddLast(msg);

            if (_Mode != EFsmMode.Async)
                _ProcAllMsgs();
        }

        public int ProcAllMsgs()
        {
            if (!_Running)
                return 0;

            //强制把 stack的标记位清除
            _InStack = false;

            if (_Mode == EFsmMode.Async)
                return _ProcAllMsgs();
            return 0;
        }

        private int _ProcAllMsgs()
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
                    Log.Assert(false, "FSM 一次处理的消息太多了,超过了 {0}", C_MAX_MSG_PER_FRAME);
                    return ret;
                }

                //2.1 获取消息队列里面的第一个消息,并弹出
                bool succ = _MsgQueue.ExtPopFirst(out TMsg msg);
                if (!succ)
                    break;

                //2.2 处理消息, 每个state node 需要返回是否产生Result
                ret++;
                EFsmProcResult change_result = _StateVt.OnFsmMsg(_State, msg, out TResult result);
                if (change_result != EFsmProcResult.Channged)
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
                _StateVt.OnFsmChange(_State, old_state);
            }
            _InStack = false;
            return ret;
        }
    }

    //状态的虚表结构的一个简单实现类, 是为了配合 IFsmStateNode 来实现的
    // IFsmStateNode  是传统的节点
    public class FsmStateVT<TState, TMsg, TResult> : IFsmStateVT<TState, TMsg, TResult>
    {
        public int PtrVer { get; private set; }
        private Dictionary<TState, IFsmStateNode<TMsg, TResult>> _Nodes;

        public FsmStateVT(int cap, IEqualityComparer<TState> comparer = null)
        {
            if (comparer == null)
                comparer = EqualityComparer<TState>.Default;

            _Nodes = new Dictionary<TState, IFsmStateNode<TMsg, TResult>>(cap, comparer);
        }

        public virtual bool OnFsmStart(TState state)
        {
            var node = GetNode(state);
            if (node == null)
                return false;
            node.OnEnter();
            return true;
        }

        public virtual bool OnFsmStop(TState state)
        {
            var node = GetNode(state);
            if (node == null)
                return false;
            node.OnExit();
            return true;
        }

        public virtual EFsmProcResult OnFsmMsg(TState state, TMsg msg, out TResult result)
        {
            var node = GetNode(state);
            if (node == null)
            {
                result = default;
                return EFsmProcResult.None;
            }

            return node.OnMsg(msg, out result);
        }

        public virtual void OnFsmChange(TState state, TState old_state)
        {
            var node_old = GetNode(old_state);
            var node_new = GetNode(state);
            if (node_old != null)
                node_old.OnExit();

            if (node_new == null)
            {
                Log.Assert(false, "找不到状态对应的节点 {0}", state);
                return;
            }
            node_new.OnEnter();
        }

        public IFsmStateNode<TMsg, TResult> GetNode(TState state)
        {
            if (_Nodes.TryGetValue(state, out var node))
                return node;
            Log.Assert(false, "FSM的state 索引超过了范围 {0}", state);
            return default;
        }

        //注意:这里可以替换, 可以动态修改成null
        public bool SetNode(TState state, IFsmStateNode<TMsg, TResult> node)
        {
            _Nodes[state] = node;
            return true;
        }

        public IFsmStateNode<TMsg, TResult> this[TState state]
        {
            get { return GetNode(state); }
            set { SetNode(state, value); }
        }

        public virtual void Destroy()
        {
            if (_Nodes == null)
                return;
            foreach (var p in _Nodes)
                p.Value.Destroy();
            _Nodes.Clear();
        }
    }
}
