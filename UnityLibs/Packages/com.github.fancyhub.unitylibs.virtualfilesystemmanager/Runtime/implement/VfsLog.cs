using System;
using System.Collections.Generic;


namespace FH.VFSManagement
{
    internal static class VfsLog
    {
        public static TagLogger _ = TagLogger.Create("VFS", ELogLvl.Debug);
    }
}
