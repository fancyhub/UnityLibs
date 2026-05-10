/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Threading;

namespace FH
{
    /// <summary>
    /// 基于 Linux kfifo 思想的高性能环形队列（带内存屏障，跨平台安全）
    /// 适用于单生产者-单消费者场景，无需加锁
    /// </summary>    
    public class KFifo<T>
    {
        private readonly T[] _buffer;
        private volatile uint _in;
        private volatile uint _out;
        private readonly uint _mask;

        public KFifo(uint capacity)
        {
            if ((capacity & (capacity - 1)) != 0 || capacity == 0)
                throw new ArgumentException("Capacity must be a power of 2 and > 0");

            _buffer = new T[capacity];
            _mask = capacity - 1;
            _in = 0;
            _out = 0;
        }

        public uint Count => _in - _out;
        public bool IsEmpty => _in == _out;
        public bool IsFull => Count == _buffer.Length;

        /// <summary>
        /// 入队（单生产者调用）
        /// 保证：数据先写入缓冲区，再更新 _in 指针
        /// </summary>
        public bool TryEnqueue(T item)
        {
            if (IsFull)
                return false;

            _buffer[_in & _mask] = item;

            // 写屏障：确保上面的数据写入完成，再让 _in 的更新对其他线程可见
            Thread.MemoryBarrier();

            _in++;
            return true;
        }

        /// <summary>
        /// 出队（单消费者调用）
        /// 保证：先读取最新的 _in 指针，再读取数据；数据读取完成后再更新 _out
        /// </summary>
        public bool TryDequeue(out T result)
        {
            // 读屏障：确保读到最新的 _in 值（生产者可能刚更新了它）
            Thread.MemoryBarrier();

            if (IsEmpty)
            {
                result = default;
                return false;
            }

            var index = _out & _mask;
            result = _buffer[index];
            _buffer[index] = default;

            // 写屏障：确保数据读取完成，再让 _out 的更新对其他线程可见
            Thread.MemoryBarrier();

            _out++;
            return true;
        }

        public bool TryPeek(out T result)
        {
            Thread.MemoryBarrier();

            if (IsEmpty)
            {
                result = default;
                return false;
            }

            result = _buffer[_out & _mask];
            return true;
        }

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _in = 0;
            _out = 0;
        }
    }
}
