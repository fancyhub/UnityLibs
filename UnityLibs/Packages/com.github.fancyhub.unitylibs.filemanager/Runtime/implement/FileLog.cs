/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.FileManagement
{
    internal static class FileLog
    {
        internal static TagLogger _ = TagLogger.Create("File", ELogLvl.Info);
    }
}
