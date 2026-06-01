# Debug Connection

Debug Connection is a lightweight TCP channel for connecting Editor-side debug tools to normal Unity players.

The default port is `56020`. Runtime players start a server by calling `FH.DebugConnectionServer.StartServer(port)`. The Editor connects through the `Tools/Debug Connection` window, then exposes the active connection through `FH.DebugConnectionClient`.

The Editor window stores named connection history in `EditorPrefs`. Records are unique by host, so saving the same host updates its name and port instead of adding a duplicate.

The Editor window's auto-port mode scans 20 ports starting at `FH.DebugConnectionServer.DefaultPort` and only accepts a connection after the debug protocol `Hello` handshake identifies the remote as a Player.

For Android devices connected by ADB, the Editor window can forward the Android player port `56020` to a local port from `56040` through `56060`, then connect to `127.0.0.1:<localPort>`.
