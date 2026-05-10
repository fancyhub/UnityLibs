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
    public enum ECsvTokenizer
    {
        Word,
        CharDelimiter,
        NewLine,
        End,
        Error,
    }

    internal sealed class CsvTokenizer
    {
        internal const char CNewLine = '\n';
        internal const char CReturn = '\r';

        internal const char CCharDelimiter = ',';
        internal const char CCharQuote = '"';
        internal const string CStrDoubleQuote = "\"\"";
        internal const string CStrQuote = "\"";

        private string _Buff;
        private int _Offset;
        private bool _AfterQuotedWord;

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
            _Buff = buf ?? String.Empty;
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

            if (_AfterQuotedWord)
            {
                _AfterQuotedWord = false;
                char c = _Buff[_Offset];
                if (c != CCharDelimiter && c != CNewLine && c != CReturn)
                {
                    _Offset = buf_len;
                    return ECsvTokenizer.Error;
                }
            }

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
                        _AfterQuotedWord = _Offset < buf_len;
                        return ECsvTokenizer.Word;
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
                        return ECsvTokenizer.Word;
                    }
            }
        }

        private ECsvTokenizer _AdvanceSplitSymb()
        {
            if (_Offset >= _Buff.Length)
                return ECsvTokenizer.End;

            char c = _Buff[_Offset];
            switch (c)
            {
                case CCharDelimiter: // ,
                    {
                        _Offset++;
                        return ECsvTokenizer.CharDelimiter;
                    }
                case CNewLine:// \n
                    {
                        _Offset++;
                        if (_Offset >= _Buff.Length)
                            return ECsvTokenizer.NewLine;

                        if (_Buff[_Offset] == CReturn) // \n\r                
                            _Offset++;

                        return ECsvTokenizer.NewLine;
                    }
                case CReturn: // \r
                    {
                        _Offset++;
                        if (_Offset >= _Buff.Length)
                            return ECsvTokenizer.NewLine;
                        if (_Buff[_Offset] == CNewLine) // \r\n
                            _Offset++;
                        return ECsvTokenizer.NewLine;
                    }

                default:
                    _Offset = _Buff.Length;
                    _AfterQuotedWord = false;
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
            return buf.Length;
        }

        private int _IndexOfNextQuote(string buf, int index, out bool contain_double_quotes)
        {
            contain_double_quotes = false;
            for (int i = index; i < buf.Length; i++)
            {
                char c = buf[i];
                if (c != CCharQuote)
                    continue;

                if (i + 1 >= buf.Length || buf[i + 1] != CCharQuote)
                    return i;

                contain_double_quotes = true;
                i++;
            }
            return -1;
        }
    }
     
}
