/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/03/03
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Net.Sockets;
using System.Threading;

namespace FH
{
    /// <summary>
    /// 这个类里面的所有操作都是要基于一个block, 要么全部写完,要么全部读取完, 才能返回true,要不然就是false
    /// </summary>
    public static class TcpBlockOp
    {
        public static bool Read(Socket socket, byte[] buff, int offset, int size)
        {
            if (socket == null)
                return false;

            if (buff == null || size < 0 || offset < 0 || offset > buff.Length || size > buff.Length - offset)
                return false;

            return Read(socket, new Span<byte>(buff, offset, size));
        }

        public static bool Read(Socket socket, Span<byte> buff)
        {
            if (socket == null)
                return false;

            try
            {
                int offset = 0;
                while (offset < buff.Length)
                {
                    Span<byte> temp = buff.Slice(offset);

                    if (!socket.Blocking)
                    {
                        if (!socket.Poll(-1, SelectMode.SelectRead))
                            return false;
                    }

                    int readSize = socket.Receive(temp, SocketFlags.None, out SocketError error);

                    if (error == SocketError.WouldBlock)
                    {
                        Thread.Yield();
                        continue;
                    }

                    if (error != SocketError.Success)
                    {
                        Log.E("Recv Err: {0}", error);
                        return false;
                    }

                    if (readSize <= 0)
                        return false;

                    if (readSize > temp.Length)
                    {
                        Log.Assert(false, "Receive size overflow, need:{0}, recv:{1}", temp.Length, readSize);
                        return false;
                    }

                    offset += readSize;
                }

                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (SocketException e)
            {
                Log.E("Recv Err: {0}", e.SocketErrorCode);
                return false;
            }
        }

        public static bool Skip(Socket socket, int size)
        {
            if (size < 0)
                return false;

            Span<byte> buff = stackalloc byte[256];

            for (; ; )
            {
                int t = Math.Min(size, buff.Length);
                if (t <= 0)
                    return true;

                var temp = buff.Slice(0, t);
                if (!Read(socket, temp))
                    return false;
                size -= temp.Length;
            }
        }

        public static bool Write(Socket socket, byte[] buff, int offset, int size)
        {
            if (socket == null)
                return false;

            if (buff == null || size < 0 || offset < 0 || offset > buff.Length || size > buff.Length - offset)
                return false;

            return Write(socket, new ReadOnlySpan<byte>(buff, offset, size));
        }

        public static bool Write(Socket socket, ReadOnlySpan<byte> buff)
        {
            if (socket == null)
                return false;

            try
            {
                int offset = 0;
                while (offset < buff.Length)
                {
                    ReadOnlySpan<byte> temp = buff.Slice(offset);

                    if (!socket.Blocking)
                    {
                        if (!socket.Poll(-1, SelectMode.SelectWrite))
                            return false;
                    }

                    int writeSize = socket.Send(temp, SocketFlags.None, out SocketError error);

                    if (error == SocketError.WouldBlock)
                    {
                        Thread.Yield();
                        continue;
                    }

                    if (error != SocketError.Success)
                    {
                        Log.E("Send Err: {0}", error);
                        return false;
                    }

                    if (writeSize <= 0)
                        return false;

                    if (writeSize > temp.Length)
                    {
                        Log.Assert(false, "Send size overflow, need:{0}, send:{1}", temp.Length, writeSize);
                        return false;
                    }

                    offset += writeSize;
                }

                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (SocketException e)
            {
                Log.E("Send Err: {0}", e.SocketErrorCode);
                return false;
            }
        }
    }
}
