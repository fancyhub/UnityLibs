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
    public sealed partial class CsvReader
    {
        public CsvReader(TextAsset ta)
        {
            if (ta == null)
            {
                byte[] buff = null;
                _Tokenizer = new CsvWordTokenizer(buff);
            }
            else
                _Tokenizer = new CsvWordTokenizer(ta.bytes);
        }
    }
}
