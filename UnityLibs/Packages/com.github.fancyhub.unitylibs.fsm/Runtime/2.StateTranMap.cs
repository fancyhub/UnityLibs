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
    /// <summary>
    /// 就是一个简单的状态迁移表, TState 可以是枚举
    /// </summary>
    public class FsmStateTranMapSimple<TState> : IFsmStateTranMap<TState, TState>
    {
        protected bool _AllowSameStateTran;
        protected TState _StartState;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">起始状态</param>
        /// <param name="allow_same_state_tran">是否允许相同状态之间的转换</param>
        public FsmStateTranMapSimple(TState start, bool allow_same_state_tran)
        {
            _StartState = start;
            _AllowSameStateTran = allow_same_state_tran;
        }

        public bool Contains(TState state, TState result)
        {
            return true;
        }

        //开始
        public virtual bool Start(out TState state)
        {
            state = _StartState;
            return true;
        }

        //重置
        public virtual void Reset()
        {
        }

        //只有值发生变化的时候，才会返回true
        public virtual bool Next(TState state, TState result, out TState next_state)
        {
            next_state = result;
            if (_AllowSameStateTran)
                return true;
            return !MyEqualityComparer<TState>.Default.Equals(state, next_state);
        }
    }

    /// <summary>
    /// 状态迁移表，无状态的
    /// </summary>
    public class StateTranMap<TState, TResult> : IFsmStateTranMap<TState, TResult>
    {
        protected struct InnerKey
        {
            public readonly TState State;
            public readonly TResult Result;

            public InnerKey(TState state, TResult result)
            {
                this.State = state;
                this.Result = result;
            }
        }

        protected sealed class InnerKeyComparer : IEqualityComparer<InnerKey>
        {
            public static InnerKeyComparer _ = new InnerKeyComparer();

            public bool Equals(InnerKey x, InnerKey y)
            {
                if (MyEqualityComparer<TState>.Default.Equals(x.State, y.State)
                    && MyEqualityComparer<TResult>.Default.Equals(x.Result, y.Result))
                    return true;
                return false;
            }

            public int GetHashCode(InnerKey obj)
            {
                return System.HashCode.Combine(
                    MyEqualityComparer<TState>.Default.GetHashCode(obj.State),
                    MyEqualityComparer<TResult>.Default.GetHashCode(obj.Result));
            }
        }

        protected Dictionary<InnerKey, TState> _Dict;
        protected TState _StartState;
        public struct TranAddHelper
        {
            private StateTranMap<TState, TResult> _Map;
            private TState _State;
            public TranAddHelper(StateTranMap<TState, TResult> map, TState state)
            {
                _Map = map;
                _State = state;
            }

            public TranAddHelper To(TResult result, TState dst_status)
            {
                _Map.AddTran(_State, result, dst_status);
                return this;
            }

            public TranAddHelper From(TState src_status)
            {
                return new TranAddHelper(_Map, src_status);
            }
        }

        public StateTranMap(TState start)
        {
            _Dict = new Dictionary<InnerKey, TState>(InnerKeyComparer._);
            _StartState = start;
        }

        public StateTranMap(TState start, int cap)
        {
            _StartState = start;
            _Dict = new Dictionary<InnerKey, TState>(cap);
        }

        public bool Contains(TState state, TResult result)
        {
            InnerKey key = new InnerKey(state, result);
            return _Dict.ContainsKey(key);
        }

        //开始
        public virtual bool Start(out TState state)
        {
            state = _StartState;
            return true;
        }

        //重置
        public virtual void Reset()
        {

        }

        /// <summary>
        /// 完全按照表格,  不管 next_state 和 state是否相同, 由构建的时候决定的
        /// </summary>        
        public virtual bool Next(TState state, TResult result, out TState next_state)
        {
            InnerKey key = new InnerKey(state, result);
            return _Dict.TryGetValue(key, out next_state);
        }

        public TranAddHelper From(TState src_status)
        {
            return new TranAddHelper(this, src_status);
        }

        public StateTranMap<TState, TResult> AddTran(TState state, TResult result, TState next_state)
        {
            InnerKey key = new InnerKey(state, result);
            _Dict.Add(key, next_state);
            return this;
        }
    }

    /// <summary>
    /// 状态迁移表， 有状态的
    /// </summary>
    public class StateTranMapHistory<TState, TResult> : StateTranMap<TState, TResult>
    {
        protected LinkedList<TState> _StateStack;

        //设置返回的 result 是什么
        public TResult _BackResult = default;
        public bool _HasBackResult = false;

        public StateTranMapHistory(TState start)
            : base(start)
        {
            _StateStack = new LinkedList<TState>();
        }

        public void SetBackResult(TResult result)
        {
            _BackResult = result;
            _HasBackResult = true;
        }

        public override bool Start(out TState state)
        {
            state = _StartState;
            Log.Assert(_StateStack.Count == 0);
            _StateStack.ExtAddLast(state);
            return true;
        }

        public override void Reset()
        {
            _StateStack.ExtClear();
        }

        //只有值发生变化的时候，才会返回true
        public override bool Next(TState state, TResult result, out TState next_state)
        {
            if (_HasBackResult && MyEqualityComparer<TResult>.Default.Equals(result, _BackResult))
            {
                return _StateStack.ExtPopLast(out next_state);
            }

            bool ret = base.Next(state, result, out next_state);
            if (ret)
            {
                _StateStack.ExtAddLast(next_state);
            }
            return ret;
        }
    }
}