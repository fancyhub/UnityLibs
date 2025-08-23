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
}