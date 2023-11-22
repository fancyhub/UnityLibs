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
        public CsvReader(TextAsset ta)
        {            
            if (ta == null)
            {
                byte[] buff = null;
                Token = new CsvToken(buff);
            }
            else
                Token = new CsvToken(ta.bytes);
        }
    }
}
