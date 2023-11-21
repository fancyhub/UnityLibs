/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/09/04
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Text;

namespace FH
{
    //String 2 Byte converter
    public struct StringWriter
    {
        private byte[] _buff;
        private int _position;
        private Encoding _encoding;

        public StringWriter(int buff_len, Encoding encoding)
        {
            _encoding = encoding;
            _buff = new byte[buff_len];
            _position = 0;
        }

        public StringWriter(byte[] buff, Encoding encoding)
        {
            _encoding = encoding;
            _buff = buff;
            _position = 0;
        }

        public int Position => _position;
        public byte[] Buffer => _buff;

        public void Clear()
        {
            _position = 0;
        }

        public int Write(string str, ref int index)
        {
            int ret = 0;
            if ((_buff.Length - _position) <= 0)
                return 0;

            for (; index < str.Length; index++)
            {
                int byte_count_need = _encoding.GetByteCount(str, index, 1);
                if (byte_count_need > (_buff.Length - _position))
                    return ret;

                _position += _encoding.GetBytes(str, index, 1, _buff, _position);
                ret++;
            }
            return ret;
        }
    }
}
