/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/09/05
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

namespace FH
{
    public interface ILogRecorder : IDisposable
    {
        public void Start();
        public void Record(List<string> messages);
    }

    public sealed class LogRecorderMgr
    {
        private const int C_COUNT_PER_PROCESS = 100;
        private static LogRecorderMgr _;
        private Thread _thread;
        private List<ILogRecorder> _recorders = new List<ILogRecorder>();
        private Queue<string> _msg_queue;
        private AutoResetEvent _auto_reset_evt;

        public static void Init(ILogRecorder[] log_recorders)
        {
            if (_ != null)
                return;

            LogRecorderMgr mgr = new LogRecorderMgr();

            foreach(var p in log_recorders)
            {
                mgr._recorders.Add(p);
            }

            mgr._auto_reset_evt = new AutoResetEvent(false);
            mgr._msg_queue = new Queue<string>(1000);
            mgr._CreateThread();
            foreach (var p in log_recorders)
            {
                p.Start();
            }

            _ = mgr;
            Record($"================ Start {DateTime.Now:yy_MM_dd HH:mm:ss:fff} ============\n\tIsPlaying:{UnityEngine.Application.isPlaying} \n\tVersion:{UnityEngine.Application.version} \n\tUnityVersion:{UnityEngine.Application.unityVersion}");
        }

        public static void Record(string msg1, string msg2)
        {
            if (_ == null)
                return;

            lock (_)
            {
                _._msg_queue.Enqueue(msg1);
                _._msg_queue.Enqueue(msg2);

                _._CreateThread();
            }

            _._auto_reset_evt.Set();
        }

        public static void Record(string msg)
        {
            if (_ == null)
                return;

            lock (_)
            {
                _._msg_queue.Enqueue(msg);

                _._CreateThread();
            }

            _._auto_reset_evt.Set();
        }

        public static void Record(List<string> msg)
        {
            if (_ == null)
                return;
            lock (_)
            {
                foreach (var p in msg)
                {
                    _._msg_queue.Enqueue(p);
                }

                _._CreateThread();
            }
        }

        private void _CreateThread()
        {
            if (_thread != null && _thread.IsAlive)
                return;

            _thread = new Thread(_Worker);
            _thread.IsBackground = true;
            _thread.Priority = ThreadPriority.Lowest;
            _thread.Start();
        }


        private void _Worker()
        {
            List<string> temp_list = new List<string>(C_COUNT_PER_PROCESS);
            for (; ; )
            {
                //1. 清除
                temp_list.Clear();

                //2. get msg
                lock (this)
                {
                    int count = System.Math.Min(C_COUNT_PER_PROCESS, _msg_queue.Count);
                    for (int i = 0; i < count; i++)
                    {
                        var temp = _msg_queue.Dequeue();
                        temp_list.Add(temp);
                    }
                }

                //3. check msg count
                if (temp_list.Count == 0)
                {
                    _auto_reset_evt.WaitOne();
                    continue;
                }

                //4. process msg
                try
                {
                    foreach (var r in _recorders)
                        r.Record(temp_list);
                }
                catch (Exception) { }

            }
        }
    }

}
