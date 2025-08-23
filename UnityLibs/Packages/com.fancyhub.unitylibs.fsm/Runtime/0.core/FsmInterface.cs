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
    public enum EFsmMode
    {
        Sync,
        Async
    }



    public interface IFsm<TMsg> : ICPtr
    {
        void SendMsg(TMsg msg);

        bool Start();
        bool IsRunning();
        int ProcAllMsgs();
        bool Stop();
    }

    public interface IFsm<TState, TMsg> : IFsm<TMsg>
    {
        bool TryGetState(out TState cur_state);
    }

    /// <summary>
    /// 状态迁移表   
    /// Fsm State Transition Map
    /// </summary>
    public interface IFsmStateTranMap<TState, TResult>
    {
        bool Start(out TState start_state);
        bool Next(TState state, TResult result, out TState next_state);
        void Reset();
    }

    /// <summary>
    /// 里面需要含有 TransMap了
    /// </summary>
    public interface IFsm<TState, TMsg, TResult, TStateTranMap> : IFsm<TState, TMsg> where TStateTranMap : IFsmStateTranMap<TState, TResult>
    {
        TStateTranMap StateTranMap { get; set; }
    }

    public enum EFsmProcResult
    {
        None,
        Channged,
    }

   

    

    
}
