/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

namespace FH
{
    public sealed class DebugConnectionListeningAddress
    {
        public string Host;
        public int Port;

        public string EndPoint
        {
            get { return string.IsNullOrEmpty(Host) ? ":" + Port : Host + ":" + Port; }
        }

        public override string ToString()
        {
            return EndPoint;
        }
    }
}
