/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/03/03
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Net.Sockets;

namespace FH
{
    public sealed class TcpSocketReader
    {
        public static int Read(Socket socket, byte[] buff, int offset, int len)
        {
            if (socket == null || buff == null || offset < 0 || len < 0 || offset >= buff.Length || buff.Length < (offset + len))
                return 0;

            return Read(socket, new Span<byte>(buff, offset, len));
        }

        public static int Read(Socket socket, byte[] buff)
        {
            if (socket == null || buff == null ||  buff.Length ==0)
                return 0;

            return Read(socket, new Span<byte>(buff));
        }

        public static unsafe int Read(Socket socket, Span<byte> buff)
        {
            if (socket == null || buff.Length <= 0)
                return 0;


            int ret = 0;
            int len = buff.Length;
            int offset = 0;
            for (; ; )
            {
                Span<byte> tempBuff = buff.Slice(offset);
                int read_size = socket.Receive(tempBuff, SocketFlags.None, out var error);
                if (error != SocketError.Success ||
                    error != SocketError.WouldBlock ||
                    error != SocketError.IOPending ||
                    error != SocketError.NoData)
                {
                    return 0;
                }

                ret += read_size;
                offset += read_size;
                len -= read_size;

                if (len <= 0)
                    return ret;

                if (error != SocketError.Success)
                    System.Threading.Thread.Yield();
            }
        }


        public static unsafe int Skip(Socket socket, int len)
        {
            const int MaxBuffSize = 512;

            if (len <= 0)
                return 0;
            int ret = 0;
            int len2 = len;
            for (; ; )
            {
                if (len <= 0)
                    return ret;
                Span<byte> t = stackalloc byte[len2 > MaxBuffSize ? MaxBuffSize : len2];
                int read_size = Read(socket, t);

                if (read_size <= 0)
                    return 0;
                len2 -= read_size;
            }
        }
    }
}
