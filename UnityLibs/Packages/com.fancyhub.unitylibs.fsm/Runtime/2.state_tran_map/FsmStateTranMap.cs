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
    /// 状态迁移表，无状态的
    /// </summary>
    public class FsmStateTranMap<TState, TResult> : IFsmStateTranMap<TState, TResult>
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
            private FsmStateTranMap<TState, TResult> _Map;
            private TState _State;
            public TranAddHelper(FsmStateTranMap<TState, TResult> map, TState state)
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

        public FsmStateTranMap(TState start)
        {
            _Dict = new Dictionary<InnerKey, TState>(InnerKeyComparer._);
            _StartState = start;
        }

        public FsmStateTranMap(TState start, int cap)
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

        public FsmStateTranMap<TState, TResult> AddTran(TState state, TResult result, TState next_state)
        {
            InnerKey key = new InnerKey(state, result);
            _Dict.Add(key, next_state);
            return this;
        }
    }
         
}