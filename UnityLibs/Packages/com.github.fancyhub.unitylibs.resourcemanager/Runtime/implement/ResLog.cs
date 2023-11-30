/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/


namespace FH.Res
{
    internal static class ResLog
    {
        public static TagLogger _ = TagLogger.Create("Res", ELogLvl.Debug);

        public static void ErrCode(EResError code, string msg)
        {
            if (code == EResError.OK)
                return;
            _.E(string.Format("ErrCode: {0} {1}", code, msg));
        }

        public static void ErrCode(EResError code)
        {
            if (code == EResError.OK)
                return;
            _.E(string.Format("ErrCode: {0} ", code));
        }
    }
}
