/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/16 12:50:30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace FH
{
    public class ObjectStreamFile : IObjectArraryInStream<byte>, IObjectArrayOutStream<byte>
    {
        public const int C_BUFF_LEN = 1024;

        private struct InnerBuff
        {
            public byte[] _bytes;
            public int _offset;

            public int Count
            {
                get
                {
                    return _bytes.Length - _offset;
                }
            }
        }

        private LinkedList<InnerBuff> _BuffQueue;
        private AutoResetEvent _AutoResetEvent;

        private Stream _FsOut;
        private byte[] _ReadBuff;

        public ObjectStreamFile(string file_path)
        {
            _FsOut = File.OpenWrite(file_path);
            _ReadBuff = new byte[C_BUFF_LEN];
            _BuffQueue = new LinkedList<InnerBuff>();
            _AutoResetEvent = new AutoResetEvent(false);
        }

        public void Close()
        {
            _FsOut?.Close();
            _FsOut = null;
        }

        public bool IsClosed()
        {
            return _FsOut == null;
        }

        public int Read(byte[] buff, int offset, int count)
        {
            int ret = count;
            try
            {
                for (; ; )
                {
                    LinkedListNode<InnerBuff> first = null;
                    lock (this)
                    {
                        first = _BuffQueue.First;
                    }

                    if (first == null)
                    {
                        _AutoResetEvent.WaitOne();
                        continue;
                    }

                    InnerBuff b = first.Value;
                    int count_to_copy = Math.Min(count, b.Count);
                    Buffer.BlockCopy(b._bytes, b._offset, buff, offset, count_to_copy);

                    b._offset += count_to_copy;
                    offset += count_to_copy;
                    count -= count_to_copy;
                    first.Value = b;

                    if (b.Count == 0)
                    {
                        lock (this)
                        {
                            _BuffQueue.RemoveFirst();
                        }
                    }

                    if (count == 0)
                        return ret;
                }
            }
            catch (Exception e)
            {
                Log.E(e);
            }
            return 0;
        }

        public int Write(byte[] buff, int offset, int count)
        {
            {
                byte[] temp = new byte[count];
                Buffer.BlockCopy(buff, offset, temp, 0, count);
                InnerBuff b = new InnerBuff();
                b._bytes = temp;
                b._offset = 0;

                lock (this)
                {
                    _BuffQueue.AddLast(b);
                }
                _AutoResetEvent.Set();
            }

            _FsOut.Write(buff, offset, count);
            return count;
        }
    }
}

