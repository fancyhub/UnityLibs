/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/3/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;


namespace FH
{
    /// <summary>
    /// 为了方便创建FSM, 参数尽量使用非接口
    /// </summary>
    public static class FsmCreator
    {
        public static IFsm<TState, TMsg> CreateFsmSimple<TState, TMsg>(
            EFsmMode mode,
            FsmStateVTNormal<TState, TMsg, TState>  stateVirtualTable,
            FsmStateTranMapSimple<TState> stateTranMap)
        {
            var fsm = new FsmNormal<TState, TMsg, TState, IFsmStateTranMap<TState, TState>>(mode);
            fsm.StateVT = stateVirtualTable;
            fsm.StateTranMap = stateTranMap;
            return fsm;
        }
        
        public static IFsm<TState, TMsg> CreateFsm<TState, TMsg, TResult>(
            EFsmMode mode,
            FsmStateVTNormal<TState, TMsg, TResult> stateVirtualTable,
            FsmStateTranMap<TState, TResult> stateTranMap)
        {
            var fsm = new FsmNormal<TState, TMsg, TResult, IFsmStateTranMap<TState, TResult>>(mode);
            fsm.StateVT = stateVirtualTable;
            fsm.StateTranMap = stateTranMap;
            return fsm;
        }

        public static IFsm<TState, TMsg> CreateFsmWithContext<TContext, TState, TMsg, TResult>(
            EFsmMode mode,
            TContext context,
            FsmStateVTWithContext<TContext, TState, TMsg, TResult> stateVirtualTable,
            FsmStateTranMap<TState, TResult> stateTranMap)
        {
            var fsm = new FsmWithContext<TContext, TState, TMsg, TResult>(mode);
            fsm.Context = context;
            fsm.StateTranMap = stateTranMap;
            fsm.StateVT = stateVirtualTable;
            return fsm;
        }


        /// <summary>
        /// 该FSM, 没有状态节点, 状态的变化都是由 fsmListener来监听的
        /// </summary>        
        public static IFsm<TState, TMsg> CreateFsmWithListener<TState, TMsg>(
          EFsmMode mode,
          IFsmStateListener<TState, TMsg> stateListener,
          FsmStateTranMap<TState, TMsg> stateTranMap)
        {
            return new FsmWithStateListener<TState, TMsg, IFsmStateTranMap<TState, TMsg>>(mode, stateTranMap, stateListener);
        }
    }
}