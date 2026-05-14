/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/10 15:30:10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Net.Sockets;
using System.Threading;

namespace FH
{
    

    public sealed class ObjectStreamTcpSocket : IObjectStream<byte>
    {
        private const int CReadTimeOut = -1;//无限
        private Socket _Socket;
        private int _Closed;

        public ObjectStreamTcpSocket(Socket socket)
        {
            _Socket = socket;
            _Closed = socket == null ? 1 : 0;
        }

        private void _Close()
        {
            if (Interlocked.Exchange(ref _Closed, 1) != 0)
                return;

            var socket = _Socket;
            _Socket = null;
            socket?.Close();
        }

        #region Stream In
        public void CloseIn()
        {
            _Close();
        }

        public bool IsClosedIn()
        {
            return Volatile.Read(ref _Closed) != 0;
        }

        public bool Read(out byte b)
        {
            b = default;
            Span<byte> buff = stackalloc byte[1];
            if (Read(buff) != buff.Length)
                return false;

            b = buff[0];
            return true;
        }

        public int Read(byte[] buff, int offset, int count)
        {
            //1. 检查
            if (!buff.ExtCheckOffsetCount(offset, count))
                return 0;

            return Read(new Span<byte>(buff, offset, count));
        }

        public int Read(Span<byte> buff)
        {
            var socket = _Socket;
            if (!TcpBlockOp.Read(socket, buff))
            {
                _Close();
                return 0;
            }

            return buff.Length;
        }
        #endregion

        #region Stream Out
        public void CloseOut()
        {
            _Close();
        }

        public bool IsClosedOut()
        {
            return Volatile.Read(ref _Closed) != 0;
        }

        public bool Write(byte data)
        {
            Span<byte> buff = stackalloc byte[1];
            buff[0] = data;

            return Write(buff) == buff.Length;
        }

        public int Write(byte[] buff, int offset, int count)
        {
            //检查参数
            if (!buff.ExtCheckOffsetCount(offset, count))
                return 0;

            return Write(new ReadOnlySpan<byte>(buff, offset, count));
        }

        public int Write(ReadOnlySpan<byte> buff)
        {
            var socket = _Socket;
            if (!TcpBlockOp.Write(socket, buff))
            {
                _Close();
                return 0;
            }

            return buff.Length;
        }
        #endregion
    }
}
