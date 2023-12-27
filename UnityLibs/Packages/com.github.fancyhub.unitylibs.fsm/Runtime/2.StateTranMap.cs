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
    public class FsmStateTranMapSimple<TState> : IFsmStateTranMap<TState, TState> where TState : struct
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
            return state.GetHashCode() != result.GetHashCode();
        }
    }

    /// <summary>
    /// 状态迁移表，无状态的
    /// </summary>
    public class StateTranMap<TState, TResult> : IFsmStateTranMap<TState, TResult>
        where TState : struct, IConvertible
        where TResult : struct, IConvertible
    {
        protected Dictionary<ulong, TState> _Dict;
        protected TState _StartState;
        public struct TranAddHelper
        {
            public StateTranMap<TState, TResult> _map;
            public TState _start;
            public TranAddHelper(StateTranMap<TState, TResult> map, TState start)
            {
                _map = map;
                _start = start;
            }

            public TranAddHelper Add(TResult result, TState next)
            {
                _map.AddTran(_start, result, next);
                return this;
            }

            public TranAddHelper Next(TState start)
            {
                return new TranAddHelper(_map, start);
            }
        }

        public StateTranMap(TState start)
        {
            _Dict = new Dictionary<ulong, TState>();
            _StartState = start;
        }

        public StateTranMap(TState start, int cap)
        {
            _StartState = start;
            _Dict = new Dictionary<ulong, TState>(cap);
        }

        public bool Contains(TState state, TResult result)
        {
            uint key1 = BitUtil.Struct2Uint(state);
            uint key2 = BitUtil.Struct2Uint(result);
            ulong key = BitUtil.MakePair(key1, key2);

            return _Dict.TryGetValue(key, out var _);
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
        public virtual bool Next(TState state, TResult result, out TState next_state)
        {
            uint key1 = BitUtil.Struct2Uint(state);
            uint key2 = BitUtil.Struct2Uint(result);
            ulong key = BitUtil.MakePair(key1, key2);

            bool ret = _Dict.TryGetValue(key, out next_state);
            if (!ret)
                return false;
            ulong key3 = BitUtil.Struct2Uint(next_state);
            if (key1 == key3)
            {
                next_state = default;
                return false;
            }
            return true;
        }

        public TranAddHelper Begin(TState start_state)
        {
            return new TranAddHelper(this, start_state);
        }

        public StateTranMap<TState, TResult> AddTran(TState state, TResult result, TState next_state)
        {
            uint key1 = BitUtil.Struct2Uint(state);
            uint key2 = BitUtil.Struct2Uint(result);
            ulong key = BitUtil.MakePair(key1, key2);

            _Dict.Add(key, next_state);
            return this;
        }
    }

    /// <summary>
    /// 状态迁移表， 有状态的
    /// </summary>
    public class StateTranMap2<TState, TResult> : StateTranMap<TState, TResult>
        where TState : struct, IConvertible
        where TResult : struct, IConvertible
    {
        protected LinkedList<TState> _StateStack;

        //设置返回的 result 是什么
        public int _BackResult = int.MinValue;

        public StateTranMap2(TState start)
            : base(start)
        {
            _StateStack = new LinkedList<TState>();
        }

        public void SetBackResult(TResult result)
        {
            _BackResult = BitUtil.Struct2Int(result);
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
            int int_result = BitUtil.Struct2Int(result);
            if (int_result == _BackResult)
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