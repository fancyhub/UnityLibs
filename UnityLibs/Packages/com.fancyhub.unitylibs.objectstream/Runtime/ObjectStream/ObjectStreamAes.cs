/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/10 15:30:10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.IO;
using System.Security.Cryptography;

namespace FH
{
    /// <summary>
    ///  Aes 加密
    /// </summary>
    public sealed class ObjectStreamAes : IObjectStream<NetPackage>
    {
        private IObjectStream<NetPackage> _Stream;
        public int _MaxPackageSize;

        public byte[] _ReadBodyBuff;
        public MemoryStream _MsReader;
        public AesManaged _ReadAes;

        public byte[] _WriteBodyBuff;
        public MemoryStream _MsWriter;
        public AesManaged _WriteAes;

        public ObjectStreamAes(IObjectStream<NetPackage> stream, int max_package_size)
        {
            _Stream = stream;
            _MaxPackageSize = max_package_size;

            _ReadBodyBuff = new byte[max_package_size];
            _MsReader = new MemoryStream(_ReadBodyBuff, true);
            _ReadAes = _CreateAes();

            _WriteBodyBuff = new byte[max_package_size];
            _MsWriter = new MemoryStream(_WriteBodyBuff, true);
            _WriteAes = _CreateAes();
        }

        private AesManaged _CreateAes()
        {
            var aes = new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 128;
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Padding = PaddingMode.PKCS7;


            // use separete assign key and iv, avoid key string or byte array find in app executable file.
            var key = new byte[16];
            key[0] = 0x18;
            key[1] = 0x60;
            key[2] = 0xa2;
            key[3] = 0x34;
            key[4] = 0xcf;
            key[5] = 0x6f;
            key[6] = 0x8b;
            key[7] = 0x85;
            key[8] = 0x3d;
            key[9] = 0xf6;
            key[10] = 0x90;
            key[11] = 0x34;
            key[12] = 0x4b;
            key[13] = 0xe7;
            key[14] = 0x91;
            key[15] = 0xdd;
            aes.Key = key;
            var iv = new byte[16];
            iv[0] = 0x02;
            iv[1] = 0xf5;
            iv[2] = 0x98;
            iv[3] = 0x9e;
            iv[4] = 0xc0;
            iv[5] = 0x26;
            iv[6] = 0xb2;
            iv[7] = 0xdf;
            iv[8] = 0xb2;
            iv[9] = 0x44;
            iv[10] = 0x21;
            iv[11] = 0xad;
            iv[12] = 0x1a;
            iv[13] = 0x9f;
            iv[14] = 0x90;
            iv[15] = 0x8d;
            aes.IV = iv;

            return aes;
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
            //1. 先读取package
            bool succ = _Stream.Read(out NetPackage pack);
            if (!succ)
            {
                data = default;
                return false;
            }

            //2. 如果没有内容,只有module cmd,直接返回
            if (pack.Header.MsgBodyLen == 0)
            {
                data = pack;
                return true;
            }

            //3. 解密
            _MsReader.Position = 0;
            ICryptoTransform dec = _ReadAes.CreateDecryptor();
            using (var cryptoStream = new CryptoStream(_MsReader, dec, CryptoStreamMode.Write))
            {
                cryptoStream.Write(pack.Body, 0, pack.Header.MsgBodyLen);
            }

            //4. 拼装 package
            data = new NetPackage();
            data.Header = pack.Header;
            data.Header.MsgBodyLen = (ushort)_MsReader.Length;
            data.Body = _ReadBodyBuff;
            return true;
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
            //1. 如果没有任何buff,直接写
            if (data.Header.MsgBodyLen == 0)
            {
                return _Stream.Write(data);
            }

            //2. 加密
            _MsWriter.Position = 0;
            ICryptoTransform enc = _WriteAes.CreateEncryptor();
            using (var csEncrypt = new CryptoStream(_MsWriter, enc, CryptoStreamMode.Write))
            {
                csEncrypt.Write(data.Body, 0, data.Header.MsgBodyLen);
            }

            //3. 写buff
            NetPackage pack = new NetPackage();
            pack.Header = data.Header;
            pack.Header.MsgBodyLen = (ushort)_MsWriter.Length;
            pack.Body = _WriteBodyBuff;

            return _Stream.Write(pack);
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
