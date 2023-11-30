/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2019/8/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    //处理消息的接口
    public interface IMsgProc<T>
    {
        void OnMsgProc(ref T msg);
    }

    //所有的MsgQueue 公用一个Id 生成器
    public static class MsgQueueIdGen
    {
        public const int C_INVALID_HANDLE_ID = 0;
        public static int _id_gen = 0;
        public static int GenId()
        {
            _id_gen++;
            return _id_gen;
        }
    }

    public class MsgQueue<T>
    {
        private const int C_INVALID_HANDLE_ID = MsgQueueIdGen.C_INVALID_HANDLE_ID;

        //所有的 handler 以及对应的 target
        // Value : IMsgQueueProc<T>
        private MyDict<int, object> _Procs;
        private LinkedList<KeyValuePair<int, T>> _MsgQueues;

        public MsgQueue()
        {
            _Procs = new MyDict<int, object>();
            _MsgQueues = new LinkedList<KeyValuePair<int, T>>();
        }

        public IMsgProc<T> Find(int handle)
        {
            _Procs.TryGetValue(handle, out object v);
            return v as IMsgProc<T>;
        } 

        public bool Reg(int handle, IMsgProc<T> proc)
        {
            //1. 检查
            if (null == proc)
            {
                //Error:
                Log.Assert(false, "proc is null");
                return false;
            }

            if (handle == C_INVALID_HANDLE_ID)
            {
                Log.Assert(false, "Handle == C_INVALID_HANDLE_ID");
                return false;
            }

            if (_Procs.ContainsKey(handle))
            {
                Log.Assert(false, "Handle: {0} 已经注册过了", handle);
                return false;
            }

            //2. 分配id，并且 加到  _targets 里面
            _Procs.Add(handle, proc);
            return true;
        }

        public bool UnReg(int handle)
        {
            return _Procs.Remove(handle, out object v);
        } 

        public void ClearMsgQueue()
        {
            _MsgQueues.ExtClear();
        }

        public void UnRegAll()
        {
            _Procs.Clear();
        }

        public int MsgCount { get { return _MsgQueues.Count; } }
      
        /// <summary>
        /// 同步发消息
        /// </summary>
        public void SendTo(int handle_id, ref T msg, bool sync = false)
        {
            //一般都是异步的
            if (!sync)
            {
                _MsgQueues.ExtAddLast(new KeyValuePair<int, T>(handle_id, msg));
                return;
            }

            IMsgProc<T> target = Find(handle_id);
            if (null == target)
            {
                Log.Assert(false, "找不到 target {0}", handle_id);
                return;
            }
            target.OnMsgProc(ref msg);
        }

        /// <summary>
        /// 异步发消息
        /// </summary>        
        public void SyncSendTo(int handle_id, ref T msg)
        {
            SendTo(handle_id, ref msg, true);
        }

        /// <summary>
        /// 异步发消息
        /// </summary>        
        public void AsyncSendTo(int handle_id, ref T msg)
        {
            SendTo(handle_id, ref msg, false);
        }

        public int ProcessMsgs(int msg_count)
        {
            int msg_count_process = 0;
            for (; ; )
            {
                //1. 找到消息
                bool succ = _MsgQueues.ExtPopFirst(out KeyValuePair<int, T> msg_pair);
                if (!succ)
                    break;

                //2. 计数器+1
                msg_count_process++;

                //3. 找到对应的 target
                int target_id = msg_pair.Key;
                IMsgProc<T> target = Find(target_id);

                //4. 处理 消息
                if (null != target)
                {
                    T msg = msg_pair.Value;
                    target.OnMsgProc(ref msg);
                }
                else
                {
                    //Log.Assert(false, "找不到: {0}", target_id);
                }

                //5. 检查是否处理消息超过了上限
                if (msg_count_process >= msg_count)
                {
                    Log.Assert(false, "当前帧的msg 消息超过了上限{0}, 剩下{1}", msg_count_process, _MsgQueues.Count);
                    //Warning: 超过了上限，最好检查一下错误，可能出现了循环消息
                    break;
                }
            }
            return msg_count_process;
        }
    }
}
