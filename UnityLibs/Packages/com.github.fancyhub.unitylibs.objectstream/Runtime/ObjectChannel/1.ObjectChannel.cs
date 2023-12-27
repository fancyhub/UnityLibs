/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/10 15:53:11
 * Title   : 
 * Desc    :  这个是多线程的操作
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

namespace FH
{
    public sealed class ObjectChannel<T> : IObjectChannel<T>
    {
        private const int C_WAIT_TIME_OUT_MS = 2000;//等待的超时时间, 2秒
        private Queue<T> _Queue;
        private AutoResetEvent _SignalRead;

        public ObjectChannel(int init_cap = 0)
        {
            if (init_cap > 0)
                _Queue = new Queue<T>(init_cap);
            else
                _Queue = new Queue<T>();
            _SignalRead = new AutoResetEvent(false);
        }

        public void Close()
        {
            lock (this)
            {
                if (_Queue == null)
                    return;

                _Queue.Clear();
                _Queue = null;

                _SignalRead.Dispose();
            }
        }

        public bool IsClosed()
        {
            return _Queue == null;
        }

        public void Destroy()
        {
            Close();
        }

        public bool Read(out T v)
        {
            if (Select(out v))
                return true;

            for (; ; )
            {
                try
                {
                    if (!_SignalRead.WaitOne(C_WAIT_TIME_OUT_MS))
                        continue;
                }
                catch (Exception)
                {
                    v = default;
                    return false;
                }

                if (_Queue == null)
                {
                    v = default;
                    return false;
                }

                if (Select(out v))
                    return true;
            }
        }

        public bool Select(out T v)
        {
            lock (this)
            {
                if (_Queue == null || _Queue.Count == 0)
                {
                    v = default(T);
                    return false;
                }
                v = _Queue.Dequeue();
                return true;
            }
        }

        public int Select(ICollection<T> list, int count = 0)
        {
            if (count <= 0)
                count = int.MaxValue;

            lock (this)
            {
                if (_Queue == null || _Queue.Count == 0)
                    return 0;

                int count_read = Math.Min(_Queue.Count, count);
                for (int i = 0; i < count_read; ++i)
                {
                    T obj = _Queue.Dequeue();
                    list.Add(obj);
                }
                return count_read;
            }
        }

        public int Select(Queue<T> list, int count = 0)
        {
            if (count <= 0)
                count = int.MaxValue;

            lock (this)
            {
                if (_Queue == null || _Queue.Count == 0)
                    return 0;

                int count_read = Math.Min(_Queue.Count, count);
                for (int i = 0; i < count_read; ++i)
                {
                    T obj = _Queue.Dequeue();
                    list.Enqueue(obj);
                }
                return count_read;
            }
        }

        public bool Write(T v)
        {
            lock (this)
            {
                if (_Queue == null)
                    return false;

                _Queue.Enqueue(v);
                _SignalRead.Set();
                return true;
            }
        }
    }     
}
