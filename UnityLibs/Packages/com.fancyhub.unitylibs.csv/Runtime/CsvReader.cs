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
        private CsvWordTokenizer _Tokenizer;

        public CsvReader(byte[] buff)
        {
            _Tokenizer = new CsvWordTokenizer(buff);
        }

        public CsvReader(string buf)
        {
            _Tokenizer = new CsvWordTokenizer(buf);
        }

        public bool IsEnd => _Tokenizer.IsEnd;


        public bool ReadRow(List<Str> out_list, bool clear = true)
        {
            if (clear)
                out_list.Clear();

            if (IsEnd)
                return false;

            bool hasWord = false;
            for (; ; )
            {
                var result = _Tokenizer.Next(out Str word);
                switch (result)
                {
                    case ECsvWordTokenizerResult.Word:
                        out_list.Add(word);
                        hasWord = true;
                        break;

                    case ECsvWordTokenizerResult.RowEnd:
                        out_list.Add(word);
                        return true;

                    case ECsvWordTokenizerResult.End:
                        return hasWord;

                    case ECsvWordTokenizerResult.Error:
                        return false;

                    default:
                        return false;
                }
            }
        }


        public bool ReadWord(out Str word)
        {
            var result = _Tokenizer.Next(out word);
            return result == ECsvWordTokenizerResult.Word ||
                result == ECsvWordTokenizerResult.RowEnd;
        }
    }
}
