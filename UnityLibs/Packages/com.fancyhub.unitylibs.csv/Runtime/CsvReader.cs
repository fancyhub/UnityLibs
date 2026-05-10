/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;

namespace FH
{
    public sealed partial class CsvReader
    {
        private CsvTokenizer _Tokenizer;
        private bool _ReadWordNeedWord = true;
        private bool _ReadWordPendingEmpty;

        public CsvReader(byte[] buff)
        {
            _Tokenizer = new CsvTokenizer(buff);
        }
        
        public CsvReader(string buf)
        {
            _Tokenizer = new CsvTokenizer(buf);
        }

        public bool IsEnd => _Tokenizer.IsEnd;

        /// <summary>
        /// 不会清除 out_list
        /// </summary>
        public bool ReadRow(List<Str> out_list)
        {
            if (IsEnd)
                return false;

            bool hasToken = false;
            bool needWord = true;
            for (; ; )
            {
                var r = _Tokenizer.Next(out Str word);
                switch (r)
                {
                    case ECsvTokenizer.Word:
                        out_list.Add(word);
                        hasToken = true;
                        needWord = false;
                        break;

                    case ECsvTokenizer.CharDelimiter:
                        if (needWord)
                            out_list.Add(Str.Empty);
                        hasToken = true;
                        needWord = true;
                        break;

                    case ECsvTokenizer.NewLine:
                        if (needWord)
                            out_list.Add(Str.Empty);
                        return true;

                    case ECsvTokenizer.End:
                        if (!hasToken)
                            return false;
                        if (needWord)
                            out_list.Add(Str.Empty);
                        return true;

                    case ECsvTokenizer.Error:
                        return false;

                    default:
                        break;
                }
            }
        }

        public bool ReadWord(out Str word)
        {
            word = Str.Empty;
            if (_ReadWordPendingEmpty)
            {
                _ReadWordPendingEmpty = false;
                _ReadWordNeedWord = false;
                return true;
            }

            if (IsEnd)
                return false;
            for (; ; )
            {
                var r = _Tokenizer.Next(out word);
                switch (r)
                {
                    case ECsvTokenizer.Word:
                        _ReadWordNeedWord = false;
                        return true;

                    case ECsvTokenizer.CharDelimiter:
                        if (_ReadWordNeedWord)
                        {
                            _ReadWordPendingEmpty = IsEnd;
                            word = Str.Empty;
                            return true;
                        }
                        _ReadWordNeedWord = true;
                        if (IsEnd)
                        {
                            _ReadWordNeedWord = false;
                            word = Str.Empty;
                            return true;
                        }
                        break;

                    case ECsvTokenizer.NewLine:
                        if (_ReadWordNeedWord)
                        {
                            word = Str.Empty;
                            return true;
                        }
                        _ReadWordNeedWord = true;
                        break;

                    case ECsvTokenizer.End:
                        return false;

                    case ECsvTokenizer.Error:
                        return false;

                    default:
                        break;
                }
            }
        }
    }
}
