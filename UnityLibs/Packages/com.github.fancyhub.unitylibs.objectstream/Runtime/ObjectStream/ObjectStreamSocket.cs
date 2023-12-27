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
    public class ObjsInStreamSocket : IObjectArraryInStream<byte>
    {
        private const int C_BUFF_LEN = 1024;
        private const int C_TIMEOUT_MS = -1;//无限
        private Socket _Socket;
        private byte[] _Buff;

        public ObjsInStreamSocket(Socket socket)
        {
            _Socket = socket;
            _Buff = new byte[C_BUFF_LEN];
        }

        public void Close()
        {
            //_socket.Close();
        }
         

        public bool IsClosed()
        {
            if (_Socket == null || !_Socket.Connected)
                return true;
            return false;
        }

        public int Read(byte[] buff, int offset, int count)
        {
            //1. 检查
            if (!buff.ExtCheckOffsetCount(offset, count))
                return 0;

            int ret_count = 0;
            for (; ; )
            {
                if (!_Socket.Connected)
                    return 0;

                if (!_Socket.Poll(1, SelectMode.SelectRead))
                    continue;

                int count_need = Math.Min(C_BUFF_LEN, count);
                int recv_count = _Socket.Receive(_Buff, 0, count_need, SocketFlags.None, out SocketError socket_err);
                if (socket_err != SocketError.Success)
                {
                    Log.E("Recv Err: {0}", socket_err);
                    return 0;
                }

                Log.Assert(recv_count <= count_need, "有问题,需要看一下, need:{0}, recv:{1}", count_need, recv_count);

                //这个比较特殊,说明网络断了
                if (recv_count == 0)
                    continue;

                Array.Copy(_Buff, 0, buff, offset, recv_count);
                count -= recv_count;
                offset += recv_count;
                ret_count += recv_count;
                if (count <= 0)
                    break;
            }

            return ret_count;
        }
    }

    public class ObjsOutStreamSocket : IObjectArrayOutStream<byte>
    {
        private Socket _Socket;
        public ObjsOutStreamSocket(Socket socket)
        {
            _Socket = socket;
        }

        public void Close()
        {
            //_socket.Close();
        }

        public bool IsClosed()
        {
            if (_Socket == null || !_Socket.Connected)
                return true;
            return false;
        }

        public int Write(byte[] buff, int offset, int count)
        {
            //检查参数
            if (!buff.ExtCheckOffsetCount(offset, count))
                return 0;

            if (_Socket == null)
                return 0;
            if (count == 0)
                return 0;

            int ret_count = 0;
            for (; ; )
            {
                if (!_Socket.Connected)
                    return 0;
                if (!_Socket.Poll(1, SelectMode.SelectWrite))
                    continue;
                
                int send_count = _Socket.Send(buff, offset, count, SocketFlags.None, out SocketError socket_err);
                if (socket_err != SocketError.Success)
                {
                    Log.E("Send Err: {0}", socket_err);
                    return 0;
                }
                if (send_count == 0)
                {
                    Log.E("Send Err count: {0},need {1}", send_count, count);
                    return 0;
                }

                offset += send_count;
                count -= send_count;
                ret_count += send_count;
                if (count <= 0)
                    break;
            }
            return ret_count;
        }
    }
}
