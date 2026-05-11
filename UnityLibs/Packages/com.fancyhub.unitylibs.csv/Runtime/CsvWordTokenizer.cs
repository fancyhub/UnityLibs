/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/05/11
 * Title   :
 * Desc    :
*************************************************************************************/

namespace FH
{
    internal enum ECsvWordTokenizerResult
    {
        Word,
        RowEnd,
        End,
        Error,
    }

    internal struct CsvWordTokenizer
    {
        private enum EState
        {
            ReadWord,
            ReadWordEnd,
        }

        private CsvTokenizer _Tokenizer;

        public CsvWordTokenizer(byte[] buff)
        {
            _Tokenizer = new CsvTokenizer(buff);
        }

        public CsvWordTokenizer(string buf)
        {
            _Tokenizer = new CsvTokenizer(buf);
        }

        public bool IsEnd => _Tokenizer.IsEnd && _Tokenizer.LastResult != ECsvTokenizerResult.CharDelimiter;

        public ECsvWordTokenizerResult Next(out Str word)
        {
            word = Str.Empty;
            Str pending_word = Str.Empty;
            EState state = EState.ReadWord;

            for (; ; )
            {
                var token_result = _Tokenizer.Next(out pending_word, out ECsvTokenizerResult last_token_result);

                switch (state)
                {
                    case EState.ReadWord:
                        {
                            switch (token_result)
                            {
                                case ECsvTokenizerResult.Word:
                                    word = pending_word;
                                    state = EState.ReadWordEnd;
                                    break;

                                case ECsvTokenizerResult.CharDelimiter:
                                    word = Str.Empty;
                                    return ECsvWordTokenizerResult.Word;

                                case ECsvTokenizerResult.NewLine:
                                    word = Str.Empty;
                                    return ECsvWordTokenizerResult.RowEnd;

                                case ECsvTokenizerResult.End:
                                    if (last_token_result != ECsvTokenizerResult.CharDelimiter)
                                        return ECsvWordTokenizerResult.End;

                                    word = Str.Empty;
                                    return ECsvWordTokenizerResult.Word;

                                case ECsvTokenizerResult.Error:
                                default:
                                    return ECsvWordTokenizerResult.Error;
                            }
                            break;
                        }

                    case EState.ReadWordEnd:
                        {
                            switch (token_result)
                            {
                                case ECsvTokenizerResult.CharDelimiter:
                                    return ECsvWordTokenizerResult.Word;

                                case ECsvTokenizerResult.NewLine:
                                    return ECsvWordTokenizerResult.RowEnd;

                                case ECsvTokenizerResult.End:
                                    return ECsvWordTokenizerResult.Word;

                                case ECsvTokenizerResult.Error:
                                default:
                                    return ECsvWordTokenizerResult.Error;
                            }
                        }

                    default:
                        return ECsvWordTokenizerResult.Error;
                }
            }
        }
    }
}
