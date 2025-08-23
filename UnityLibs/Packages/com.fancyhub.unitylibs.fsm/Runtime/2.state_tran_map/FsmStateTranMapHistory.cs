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
    /// 状态迁移表， 有状态的
    /// </summary>
    public class FsmStateTranMapHistory<TState, TResult> : FsmStateTranMap<TState, TResult>
    {
        protected LinkedList<TState> _StateStack;

        //设置返回的 result 是什么
        public TResult _BackResult = default;
        public bool _HasBackResult = false;

        public FsmStateTranMapHistory(TState start)
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