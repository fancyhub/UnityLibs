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
    public interface IFsmStateNodeWithContext<TConext, TMsg, TResult> : IDestroyable
    {
        void OnEnter(TConext context);

        /// <summary>        
        /// 因为 IFsmStateNode 对于fsm不是必须的,也是不可见的, 但是 IFsmStateVT 必须要有<para/>
        /// IFsmStateVT的一种实现,是用switch来做,不需要IFsmStateNode<para/>
        /// </summary>
        EFsmProcResult OnMsg(TConext context, TMsg msg, out TResult result);

        void OnExit(TConext context);
    }

    /// <summary>
    /// 状态的虚表结构的一个简单实现类, 是为了配合 IFsmStateNode 来实现的 
    /// IFsmStateNode  是传统的节点
    /// </summary>
    public class FsmStateVTWithContext<TContext, TState, TMsg, TResult> : IFsmStateVTWithContext<TContext, TState, TMsg, TResult>
    {
        public int ObjVersion { get; private set; }
        private Dictionary<TState, IFsmStateNodeWithContext<TContext, TMsg, TResult>> _Nodes;

        public FsmStateVTWithContext(int cap, IEqualityComparer<TState> comparer = null)
        {
            if (comparer == null)
                comparer = System.Collections.Generic.EqualityComparer<TState>.Default;

            _Nodes = new Dictionary<TState, IFsmStateNodeWithContext<TContext, TMsg, TResult>>(cap, comparer);
        }

        public virtual bool OnFsmStart(TContext context, TState state)
        {
            var node = GetNode(state);
            if (node == null)
                return false;
            node.OnEnter(context);
            return true;
        }

        public virtual bool OnFsmStop(TContext context, TState state)
        {
            var node = GetNode(state);
            if (node == null) return false;
            node.OnExit(context);
            return true;
        }

        public virtual EFsmProcResult OnFsmMsg(TContext context, TState state, TMsg msg, out TResult result)
        {
            var node = GetNode(state);
            if (node == null)
            {
                result = default;
                return EFsmProcResult.None;
            }

            return node.OnMsg(context, msg, out result);
        }

        public virtual void OnFsmChange(TContext context, TState state, TState old_state)
        {
            var node_old = GetNode(old_state);
            var node_new = GetNode(state);
            if (node_old != null)
                node_old.OnExit(context);

            if (node_new == null)
            {
                Log.Assert(false, "找不到状态对应的节点 {0}", state);
                return;
            }
            node_new.OnEnter(context);
        }

        public IFsmStateNodeWithContext<TContext, TMsg, TResult> GetNode(TState state)
        {
            if (_Nodes.TryGetValue(state, out var node))
                return node;
            Log.Assert(false, "FSM的state 索引超过了范围 {0}", state);
            return default;
        }

        //注意:这里可以替换, 可以动态修改成null
        public bool SetNode(TState state, IFsmStateNodeWithContext<TContext, TMsg, TResult> node)
        {
            _Nodes[state] = node;
            return true;
        }

        public IFsmStateNodeWithContext<TContext, TMsg, TResult> this[TState state]
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
