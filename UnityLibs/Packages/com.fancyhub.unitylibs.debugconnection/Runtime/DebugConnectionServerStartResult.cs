/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

namespace FH
{
    public enum DebugConnectionServerStartResult
    {
        Started,
        AlreadyRunning,
        InvalidPort,
        PortInUse,
        SocketError,
    }
}
