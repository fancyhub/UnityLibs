/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/10 15:30:10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Net.Sockets;

namespace FH
{
    public sealed class ObjectStreamTcpSocket : IObjectStream<byte>
    {
        private const int CReadTimeOut = -1;//无限
        private Socket _Socket;

        public ObjectStreamTcpSocket(Socket socket)
        {
            _Socket = socket;
        }

        #region Stream In
        public void CloseIn()
        {
            _Socket?.Close();
        }

        public bool IsClosedIn()
        {
            if (_Socket == null || !_Socket.Connected)
                return true;
            return false;
        }

        public bool Read(out byte b)
        {
            Span<byte> buff = stackalloc byte[1];

            int ret = Read(buff);
            b = buff[0];

            return ret == 1;
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
            int ret_count = 0;
            int offset = 0;
            int count = buff.Length;

            for (; ; )
            {
                if (!_Socket.Connected)
                    return 0;

                if (!_Socket.Poll(1, SelectMode.SelectRead))
                    continue;

                int recv_count = _Socket.Receive(buff.Slice(offset, count), SocketFlags.None, out SocketError socket_err);
                if (socket_err != SocketError.Success)
                {
                    Log.E("Recv Err: {0}", socket_err);
                    return 0;
                }

                Log.Assert(recv_count <= count, "有问题,需要看一下, need:{0}, recv:{1}", count, recv_count);

                //这个比较特殊,说明网络断了
                if (recv_count == 0)
                {
                    continue;
                }

                count -= recv_count;
                offset += recv_count;
                ret_count += recv_count;
                if (count <= 0)
                    break;
            }
            return ret_count;
        }
        #endregion

        #region Stream Out
        public void CloseOut()
        {
            _Socket?.Close();
        }

        public bool IsClosedOut()
        {
            if (_Socket == null || !_Socket.Connected)
                return true;
            return false;
        }

        public bool Write(byte data)
        {
            Span<byte> buff = stackalloc byte[1];
            buff[0] = data;
            return Write(buff) == 1;
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
            if (_Socket == null)
                return 0;
            if (buff.Length == 0)
                return 0;

            int ret_count = 0;
            for (; ; )
            {
                if (!_Socket.Connected)
                    return 0;
                if (!_Socket.Poll(1, SelectMode.SelectWrite))
                    continue;

                int send_count = _Socket.Send(buff, SocketFlags.None, out SocketError socket_err);
                if (socket_err != SocketError.Success)
                {
                    Log.E("Send Err: {0}", socket_err);
                    return 0;
                }
                if (send_count == 0)
                {
                    Log.E("Send Err count: {0},need {1}", send_count, buff.Length);
                    return 0;
                }

                buff = buff.Slice(send_count);

                ret_count += send_count;
                if (buff.Length <= 0)
                    break;
            }
            return ret_count;
        }
        #endregion
    }
}
