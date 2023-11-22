/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace FH
{
    public enum ECsvToken
    {
        Word, //后面跟着的是 ,
        WordWithNewLine, //后面跟着的是 换行符
        WordWithEnd, //后面跟着的结束符
        End,
        Error,
    }

    public class CsvToken : IEnumerable<KeyValuePair<ECsvToken, Str>>
    {
        private const char C_NEW_LINE = '\n';
        private const char C_RETURN = '\r';
        private const char C_COMMAS = ',';
        private const char C_QUOTES = '"';

        private string _buf;
        private int _offset;

        public CsvToken(byte[] buff)
        {
            string text = String.Empty;
            if (buff != null)
            {
                if (buff.Length >= 3 && buff[0] == 0xef && buff[1] == 0xbb && buff[2] == 0xbf)
                {
                    text = System.Text.Encoding.UTF8.GetString(buff, 3, buff.Length - 3);
                }
                else
                    text = System.Text.Encoding.UTF8.GetString(buff);
            }
            _buf = text;
            _offset = 0;
        }

        public CsvToken(string buf)
        {
            _buf = buf;
            _offset = 0;
        }

        public bool IsEnd { get { return _offset >= _buf.Length; } }

        public ECsvToken Next(out Str word)
        {
            //1. 检查是否已经到了结尾
            word = Str.Empty;

            int buf_len = _buf.Length;
            if (_offset >= buf_len)
                return ECsvToken.End;

            //2. 读取第一个字符
            char first_char = _buf[_offset];
            switch (first_char)
            {
                case C_QUOTES: // " 碰到了这个
                    {
                        int end_index = _IndexOfNextQuotes(_buf, _offset + 1, out bool contain_double_quotes);
                        if (end_index == -1)
                        {
                            _offset = buf_len;
                            return ECsvToken.Error;
                        }

                        int start = _offset + 1;
                        int count = end_index - start;
                        word = new Str(_buf, start, count);

                        if (contain_double_quotes)
                            word = _TrimQuotes(word);

                        _offset = end_index + 1;

                        return _AdvanceSplitSymb();
                    }

                case C_NEW_LINE: // 换行符号
                case C_RETURN: //换行符号
                case C_COMMAS: //直接就是逗号
                    {
                        return _AdvanceSplitSymb();
                    }

                default: //普通的字符
                    {
                        int end_index = _IndexOfStrEnd(_buf, _offset);
                        if (end_index == -1)
                        {
                            _offset = buf_len;
                            return ECsvToken.Error;
                        }

                        int start = _offset;
                        int count = end_index - start;
                        _offset = end_index;
                        word = new Str(_buf, start, count);
                        return _AdvanceSplitSymb();
                    }
            }
        }


        private ECsvToken _AdvanceSplitSymb()
        {
            if (_offset >= _buf.Length)
                return ECsvToken.WordWithEnd;

            char c = _buf[_offset];
            switch (c)
            {
                case C_COMMAS: // ,
                    {
                        _offset++;
                        return ECsvToken.Word;
                    }
                case C_NEW_LINE:// \n
                    {
                        _offset++;
                        if (_offset >= _buf.Length)
                            return ECsvToken.WordWithNewLine;

                        if (_buf[_offset] == C_RETURN) // \n\r                
                            _offset++;

                        return ECsvToken.WordWithNewLine;
                    }
                case C_RETURN: // \r
                    {
                        _offset++;
                        if (_offset >= _buf.Length)
                            return ECsvToken.WordWithNewLine;
                        if (_buf[_offset] == C_NEW_LINE) // \r\n
                            _offset++;
                        return ECsvToken.WordWithNewLine;
                    }

                default:
                    return ECsvToken.Error;
            }
        }

        private Str _TrimQuotes(Str str)
        {
            return str.StrVal.Replace("\"\"", "\"");
        }

        private int _IndexOfStrEnd(string buf, int index)
        {
            for (int i = index; i < buf.Length; i++)
            {
                char c = buf[i];
                if (c == C_COMMAS || c == C_NEW_LINE || c == C_RETURN)
                    return i;
            }
            return -1;
        }

        private int _IndexOfNextQuotes(string buf, int index, out bool contain_double_quotes)
        {
            contain_double_quotes = false;
            for (int i = index; i < buf.Length - 1; i++)
            {
                char c = buf[i];
                if (c != C_QUOTES)
                    continue;

                if (buf[i + 1] != C_QUOTES)
                    return i;

                contain_double_quotes = true;
                i++;
            }
            return -1;
        }


        #region Enumerator
        public struct Enumerator : IEnumerator<KeyValuePair<ECsvToken, Str>>
        {
            public CsvToken _token;
            public KeyValuePair<ECsvToken, Str> _cur;
            public Enumerator(CsvToken reader)
            {
                _token = reader;
                _cur = default;
            }

            public KeyValuePair<ECsvToken, Str> Current => _cur;

            object IEnumerator.Current => _cur;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var r = _token.Next(out Str v);
                if (r == ECsvToken.End)
                    return false;
                _cur = new KeyValuePair<ECsvToken, Str>(r, v);
                return true;
            }

            public void Reset()
            {
                _token._offset = 0;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<KeyValuePair<ECsvToken, Str>> IEnumerable<KeyValuePair<ECsvToken, Str>>.GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }
        #endregion
    }
}
