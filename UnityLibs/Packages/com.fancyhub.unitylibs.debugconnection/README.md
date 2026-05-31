# Debug Connection

Debug Connection is a lightweight TCP channel for connecting Editor-side debug tools to normal Unity players.

The default port is `56020`. Runtime players start a server by calling `FH.DebugConnection.StartServer(port)`. Editor tools connect to that player through `FH.DebugConnectionEditorClient.Connect(host, port)` or the `Tools/Debug Connection` window.

The Editor window stores named connection history in `EditorPrefs`. Records are unique by host, so saving the same host updates its name and port instead of adding a duplicate.

Editor tools can also call `FH.DebugConnectionEditorClient.ConnectAutoPort(host, startPort)`. The Editor window's auto-port mode always scans 20 ports starting at `FH.DebugConnection.DefaultPort` and only accepts a connection after the debug protocol `Hello` handshake identifies the remote as a Player.
