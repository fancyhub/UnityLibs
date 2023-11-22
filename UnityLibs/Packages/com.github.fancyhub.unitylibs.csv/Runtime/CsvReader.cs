/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace FH
{
    public partial class CsvReader
    {
        public CsvToken Token;

        public CsvReader(byte[] buff)
        {
            Token = new CsvToken(buff);
        }

        

        public CsvReader(string buf)
        {
            Token = new CsvToken(buf);
        }

        public bool IsEnd => Token.IsEnd;

        public bool ReadRow(List<Str> out_list)
        {
            out_list.Clear();
            if (IsEnd)
                return false;
            for (; ; )
            {
                var r = Token.Next(out Str word);
                switch (r)
                {
                    case ECsvToken.Word:
                        out_list.Add(word);
                        break;

                    case ECsvToken.WordWithEnd:
                    case ECsvToken.WordWithNewLine:
                        out_list.Add(word);
                        return true;
                    case ECsvToken.Error:
                        return false;
                    case ECsvToken.End:
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
                var r = Token.Next(out word);
                switch (r)
                {
                    case ECsvToken.Word:
                    case ECsvToken.WordWithEnd:
                    case ECsvToken.WordWithNewLine:
                        return true;
                    case ECsvToken.End:
                        return false;
                    case ECsvToken.Error:
                        return false;
                    default:
                        break;
                }
            }
        }
    }
}
