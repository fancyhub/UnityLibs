/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;

namespace FH
{
    internal enum ECsvTokenizer
    {
        Word, //后面跟着的是 ,
        WordWithNewLine, //后面跟着的是 换行符
        WordWithEnd, //后面跟着的结束符
        End,
        Error,
    }

    internal sealed class CsvTokenizer : IEnumerable<KeyValuePair<ECsvTokenizer, Str>>
    {
        internal const char CNewLine = '\n';
        internal const char CReturn = '\r';

        internal const char CCharDelimiter = ',';
        internal const char CCharQuote = '"';
        internal const string CStrDoubleQuote = "\"\"";
        internal const string CStrQuote = "\"";

        private string _Buff;
        private int _Offset;

        public CsvTokenizer(byte[] buff)
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
            _Buff = text;
            _Offset = 0;
        }

        public CsvTokenizer(string buf)
        {
            _Buff = buf;
            _Offset = 0;
        }

        public bool IsEnd { get { return _Offset >= _Buff.Length; } }

        public ECsvTokenizer Next(out Str word)
        {
            //1. 检查是否已经到了结尾
            word = Str.Empty;

            int buf_len = _Buff.Length;
            if (_Offset >= buf_len)
                return ECsvTokenizer.End;

            //2. 读取第一个字符
            char first_char = _Buff[_Offset];
            switch (first_char)
            {
                case CCharQuote: // " 碰到了这个
                    {
                        int end_index = _IndexOfNextQuote(_Buff, _Offset + 1, out bool contain_double_quotes);
                        if (end_index == -1)
                        {
                            _Offset = buf_len;
                            return ECsvTokenizer.Error;
                        }

                        int start = _Offset + 1;
                        int count = end_index - start;
                        word = new Str(_Buff, start, count);

                        if (contain_double_quotes)
                            word = word.StrVal.Replace(CStrDoubleQuote, CStrQuote);

                        _Offset = end_index + 1;

                        return _AdvanceSplitSymb();
                    }

                case CNewLine: // 换行符号
                case CReturn: //换行符号
                case CCharDelimiter: //直接就是逗号
                    {
                        return _AdvanceSplitSymb();
                    }

                default: //普通的字符
                    {
                        int end_index = _IndexOfStrEnd(_Buff, _Offset);
                        if (end_index == -1)
                        {
                            _Offset = buf_len;
                            return ECsvTokenizer.Error;
                        }

                        int start = _Offset;
                        int count = end_index - start;
                        _Offset = end_index;
                        word = new Str(_Buff, start, count);
                        return _AdvanceSplitSymb();
                    }
            }
        }


        private ECsvTokenizer _AdvanceSplitSymb()
        {
            if (_Offset >= _Buff.Length)
                return ECsvTokenizer.WordWithEnd;

            char c = _Buff[_Offset];
            switch (c)
            {
                case CCharDelimiter: // ,
                    {
                        _Offset++;
                        return ECsvTokenizer.Word;
                    }
                case CNewLine:// \n
                    {
                        _Offset++;
                        if (_Offset >= _Buff.Length)
                            return ECsvTokenizer.WordWithNewLine;

                        if (_Buff[_Offset] == CReturn) // \n\r                
                            _Offset++;

                        return ECsvTokenizer.WordWithNewLine;
                    }
                case CReturn: // \r
                    {
                        _Offset++;
                        if (_Offset >= _Buff.Length)
                            return ECsvTokenizer.WordWithNewLine;
                        if (_Buff[_Offset] == CNewLine) // \r\n
                            _Offset++;
                        return ECsvTokenizer.WordWithNewLine;
                    }

                default:
                    return ECsvTokenizer.Error;
            }
        }        

        private int _IndexOfStrEnd(string buf, int index)
        {
            for (int i = index; i < buf.Length; i++)
            {
                char c = buf[i];
                if (c == CCharDelimiter || c == CNewLine || c == CReturn)
                    return i;
            }
            return -1;
        }

        private int _IndexOfNextQuote(string buf, int index, out bool contain_double_quotes)
        {
            contain_double_quotes = false;
            for (int i = index; i < buf.Length - 1; i++)
            {
                char c = buf[i];
                if (c != CCharQuote)
                    continue;

                if (buf[i + 1] != CCharQuote)
                    return i;

                contain_double_quotes = true;
                i++;
            }
            return -1;
        }


        #region Enumerator
        public struct Enumerator : IEnumerator<KeyValuePair<ECsvTokenizer, Str>>
        {
            public CsvTokenizer _token;
            public KeyValuePair<ECsvTokenizer, Str> _cur;
            public Enumerator(CsvTokenizer reader)
            {
                _token = reader;
                _cur = default;
            }

            public KeyValuePair<ECsvTokenizer, Str> Current => _cur;

            object IEnumerator.Current => _cur;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                var r = _token.Next(out Str v);
                if (r == ECsvTokenizer.End)
                    return false;
                _cur = new KeyValuePair<ECsvTokenizer, Str>(r, v);
                return true;
            }

            public void Reset()
            {
                _token._Offset = 0;
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<KeyValuePair<ECsvTokenizer, Str>> IEnumerable<KeyValuePair<ECsvTokenizer, Str>>.GetEnumerator()
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
