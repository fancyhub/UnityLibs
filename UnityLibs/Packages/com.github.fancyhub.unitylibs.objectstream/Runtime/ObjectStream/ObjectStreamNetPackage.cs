/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/10 15:30:10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public struct NetPackageHeader
    {
        public const int CHeaderSize = 6;

        public int MsgId;
        public ushort MsgBodyLen;

        public static NetPackageHeader Parse(ReadOnlySpan<byte> data)
        {
            NetPackageHeader ret = new NetPackageHeader();
            ret.MsgId = BitConverter.ToInt32(data.Slice(0, 4));
            ret.MsgBodyLen = BitConverter.ToUInt16(data.Slice(4, 2));
            return ret;
        }

        public void ToBuff(Span<byte> data)
        {
            BitConverter.TryWriteBytes(data.Slice(0, 4), MsgId);
            BitConverter.TryWriteBytes(data.Slice(4, 2), MsgBodyLen);
        }
    }

    public struct NetPackage
    {
        public NetPackageHeader Header;
        public byte[] Body;
    }

    /// <summary>
    ///  处理Tcp的粘包问题
    /// </summary>
    public sealed class ObjectStreamNetPackage : IObjectStream<NetPackage>
    {
        private IObjectStream<byte> _Stream;
        public int _MaxPackageSize;

        public byte[] _ReadBodyBuff;

        public ObjectStreamNetPackage(ObjectStreamTcpSocket stream, int max_package_size)
        {
            _Stream = stream;
            _MaxPackageSize = max_package_size;
            _ReadBodyBuff = new byte[max_package_size];
        }

        #region Stream In
        public void CloseIn()
        {
            _Stream.CloseIn();
        }

        public bool IsClosedIn()
        {
            return _Stream.IsClosedIn();
        }

        public bool Read(out NetPackage data)
        {
            for (; ; )
            {
                //1. 读取消息头
                if (!_ReadHeader(out var header))
                {
                    data = default;
                    return false;
                }

                //2. 检查长度是否超过了
                if (header.MsgBodyLen > _MaxPackageSize)
                {
                    Log.Assert(false,
                            "要接收的Package 的size 超过了最大值 now:{0} > def:{1}, MsgId: {2}",
                            header.MsgBodyLen,
                            _MaxPackageSize,
                            header.MsgId);

                    if (!_ReadSkip(header.MsgBodyLen))
                    {
                        data = default;
                        return false;
                    }
                    continue;
                }

                //3. 空消息
                if (header.MsgBodyLen == 0)
                {
                    data = new NetPackage();
                    data.Header = header;
                    data.Body = System.Array.Empty<byte>();
                    return true;
                }

                //4. 读取消息体
                int read_count = _Stream.Read(_ReadBodyBuff, 0, header.MsgBodyLen);
                if (read_count != header.MsgBodyLen)
                {
                    Log.E("need:{0} != recv:{1}", header.MsgBodyLen, read_count);
                    data = default;
                    return false;
                }

                //5. 返回
                data = new NetPackage();
                data.Header = header;
                data.Body = _ReadBodyBuff;
                return true;
            }
        }


        private bool _ReadHeader(out NetPackageHeader header)
        {
            Span<byte> header_buff = stackalloc byte[NetPackageHeader.CHeaderSize];

            //1. 读取消息头
            int recv_count = _Stream.Read(header_buff);
            if (recv_count != header_buff.Length)
            {
                Log.Assert(recv_count != header_buff.Length, "need:{0} != recv:{1}", header_buff.Length, recv_count);
                header = default;
                return false;
            }
            header = NetPackageHeader.Parse(header_buff);
            return true;
        }

        private bool _ReadSkip(int count)
        {
            int max_stack_size = 512;
            Span<byte> buff = stackalloc byte[max_stack_size];

            for (; ; )
            {
                int need_count = Math.Min(count, buff.Length);
                if (need_count == 0)
                    return true;
                int read_count = _Stream.Read(buff.Slice(0, need_count));
                if (read_count != need_count)
                    return false;
                count -= read_count;
            }
        }

        public int Read(NetPackage[] buff, int offset, int count)
        {
            //1. 检查
            if (!buff.ExtCheckOffsetCount(offset, count))
                return 0;


            if (!Read(out var data))
                return 0;
            buff[offset] = data;
            return 1;
        }

        public int Read(Span<NetPackage> buff)
        {
            if (buff.Length == 0)
                return 0;

            if (!Read(out var data))
                return 0;
            buff[0] = data;
            return 1;
        }

        #endregion

        #region Stream Out
        public void CloseOut()
        {
            _Stream?.CloseOut();
        }

        public bool IsClosedOut()
        {
            return _Stream.IsClosedOut();
        }

        public bool Write(NetPackage data)
        {
            Span<byte> buff = stackalloc byte[NetPackageHeader.CHeaderSize];
            data.Header.ToBuff(buff);
            int write_count = _Stream.Write(buff);
            if (write_count != buff.Length)
            {
                Log.Assert(false, "write 的数量不一致 need:{0}, result:{1}", NetPackageHeader.CHeaderSize, write_count);
                return false;
            }


            if (data.Header.MsgBodyLen == 0)
                return true;

            write_count = _Stream.Write(data.Body, 0, data.Header.MsgBodyLen);
            Log.Assert(write_count == data.Header.MsgBodyLen, "write 的数量不一致 need:{0}, result: {1}", data.Header.MsgBodyLen, write_count);

            return write_count == data.Header.MsgBodyLen;
        }

        public int Write(NetPackage[] buff, int offset, int count)
        {
            //检查参数
            if (!buff.ExtCheckOffsetCount(offset, count))
                return 0;

            for (int i = 0; i < count; i++)
            {
                if (!Write(buff[offset + i]))
                    return i;
            }
            return count;
        }

        public int Write(ReadOnlySpan<NetPackage> buff)
        {
            for (int i = 0; i < buff.Length; i++)
            {
                if (!Write(buff[i]))
                    return i;
            }
            return buff.Length;
        }
        #endregion
    }
}
