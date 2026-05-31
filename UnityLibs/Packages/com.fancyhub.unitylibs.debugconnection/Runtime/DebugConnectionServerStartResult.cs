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
