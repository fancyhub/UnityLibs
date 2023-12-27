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
    /// <summary>
    /// 有界的ObjChannel
    /// </summary>
    public sealed class BoundedObjectChannel<T> : IObjectChannel<T>
    {
        private const int CWaitTimeOutMs = 2000;//等待的超时时间, 2秒
        private const int CMinCount = 1;

        private int _BoundCount = CMinCount;
        private Queue<T> _Queue;
        private AutoResetEvent _SignalRead;
        private AutoResetEvent _SignalWrite;

        public BoundedObjectChannel(int count)
        {
            _BoundCount = Math.Max(count, CMinCount);
            _Queue = new Queue<T>(_BoundCount);
            _SignalRead = new AutoResetEvent(false);
            _SignalWrite = new AutoResetEvent(true);
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
                _SignalWrite.Dispose();
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
                    if (!_SignalRead.WaitOne(CWaitTimeOutMs))
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

                if (_Queue.Count < _BoundCount)
                    _SignalWrite.Set();
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

                if (_Queue.Count < _BoundCount)
                    _SignalWrite.Set();
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
                if (_Queue.Count < _BoundCount)
                    _SignalWrite.Set();
                return count_read;
            }
        }


        public bool Write(T v)
        {
            //先锁住, 如果可以写, 直接写入
            lock (this)
            {
                if (_Queue == null)
                    return false;

                if (_Queue.Count < _BoundCount)
                {
                    _Queue.Enqueue(v);
                    _SignalRead.Set();
                    return true;
                }
            }

            //等到写锁
            for (; ; )
            {
                try
                {
                    if (!_SignalWrite.WaitOne(CWaitTimeOutMs))
                        continue;
                }
                catch (Exception)
                {
                    return false;
                }

                lock (this)
                {
                    if (_Queue == null)
                        return false;

                    if (_Queue.Count < _BoundCount)
                    {
                        _Queue.Enqueue(v);
                        _SignalRead.Set();
                        return true;
                    }
                }
            }
        }
    }
}
