/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;

namespace FH
{
    public partial class CsvReader
    {
        private CsvTokenizer _Tokenizer;

        public CsvReader(byte[] buff)
        {
            _Tokenizer = new CsvTokenizer(buff);
        }
        
        public CsvReader(string buf)
        {
            _Tokenizer = new CsvTokenizer(buf);
        }

        public bool IsEnd => _Tokenizer.IsEnd;

        public bool ReadRow(List<Str> out_list)
        {            
            if (IsEnd)
                return false;
            for (; ; )
            {
                var r = _Tokenizer.Next(out Str word);
                switch (r)
                {
                    case ECsvTokenizer.Word:
                        out_list.Add(word);
                        break;

                    case ECsvTokenizer.WordWithEnd:
                    case ECsvTokenizer.WordWithNewLine:
                        out_list.Add(word);
                        return true;
                    case ECsvTokenizer.Error:
                        return false;
                    case ECsvTokenizer.End:
                        return false;
                    default:
                        break;
                }
            }
        }

        public bool ReadWord(out Str word)
        {
            word = Str.Empty;
            if (IsEnd)
                return false;
            for (; ; )
            {
                var r = _Tokenizer.Next(out word);
                switch (r)
                {
                    case ECsvTokenizer.Word:
                    case ECsvTokenizer.WordWithEnd:
                    case ECsvTokenizer.WordWithNewLine:
                        return true;
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
