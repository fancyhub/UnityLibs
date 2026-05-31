using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FH
{
    public sealed class DebugConnectionWindow : EditorWindow
    {
        private const string CHostKey = "FH.DebugConnection.Host";
        private const string CPortKey = "FH.DebugConnection.Port";
        private const string CAutoPortKey = "FH.DebugConnection.AutoPort";

        private string _Host;
        private int _Port;
        private bool _AutoPort;
        private string _Name;
        private int _HistoryIndex = -1;
        private List<DebugConnectionHistoryRecord> _History = new List<DebugConnectionHistoryRecord>();

        [MenuItem("Tools/Debug Connection")]
        public static void Open()
        {
            DebugConnectionWindow window = GetWindow<DebugConnectionWindow>();
            window.titleContent = new GUIContent("Debug Connection");
            window.minSize = new Vector2(420, 260);
            window.Show();
        }

        private void OnEnable()
        {
            _Host = EditorPrefs.GetString(CHostKey, "127.0.0.1");
            _Port = EditorPrefs.GetInt(CPortKey, DebugConnectionServer.DefaultPort);
            _AutoPort = EditorPrefs.GetBool(CAutoPortKey, false);
            _Name = _Host;
            ReloadHistory();
            DebugConnectionEditorClient.Connected += OnConnectionChanged;
            DebugConnectionEditorClient.Disconnected += OnConnectionChanged;
            DebugConnectionEditorClient.Error += OnError;
            DebugConnectionEditorClient.TargetInfoChanged += OnTargetInfoChanged;
        }

        private void OnDisable()
        {
            DebugConnectionEditorClient.Connected -= OnConnectionChanged;
            DebugConnectionEditorClient.Disconnected -= OnConnectionChanged;
            DebugConnectionEditorClient.Error -= OnError;
            DebugConnectionEditorClient.TargetInfoChanged -= OnTargetInfoChanged;
        }

        private void OnGUI()
        {
            DrawConnectionControls();
            EditorGUILayout.Space(8);
            DrawHistoryControls();
            EditorGUILayout.Space(8);
            DrawTarget();
        }

        private void DrawConnectionControls()
        {
            EditorGUILayout.LabelField("Remote Player", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(DebugConnectionEditorClient.IsRunning))
            {
                _Name = EditorGUILayout.TextField("Name", _Name);
                _Host = EditorGUILayout.TextField("Host", _Host);
                _AutoPort = EditorGUILayout.Toggle("Auto Port", _AutoPort);

                if (_AutoPort)
                {
                    EditorGUILayout.LabelField(
                        "Port Range",
                        DebugConnectionServer.DefaultPort + " - " + (DebugConnectionServer.DefaultPort + DebugConnectionServer.DefaultPortScanCount - 1));
                }
                else
                {
                    _Port = EditorGUILayout.IntField("Port", _Port);
                }
            }

            EditorGUILayout.BeginHorizontal();
            if (!DebugConnectionEditorClient.IsRunning)
            {
                if (GUILayout.Button("Connect", GUILayout.Width(100)))
                    Connect();

                if (GUILayout.Button("Save", GUILayout.Width(80)))
                    SaveHistory();
            }
            else
            {
                if (GUILayout.Button("Disconnect", GUILayout.Width(100)))
                    DebugConnectionEditorClient.Disconnect();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(
                DebugConnectionEditorClient.IsConnected
                    ? "Connected"
                    : DebugConnectionEditorClient.IsRunning ? "Connecting" : "Stopped",
                GUILayout.Width(180));
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(DebugConnectionEditorClient.LastError))
                EditorGUILayout.HelpBox(DebugConnectionEditorClient.LastError, MessageType.Warning);
        }

        private void DrawHistoryControls()
        {
            EditorGUILayout.LabelField("History", EditorStyles.boldLabel);

            if (_History.Count == 0)
            {
                EditorGUILayout.HelpBox("No history records.", MessageType.Info);
                return;
            }

            string[] options = new string[_History.Count];
            for (int i = 0; i < _History.Count; i++)
                options[i] = _History[i].DisplayName;

            int newIndex = EditorGUILayout.Popup("Record", Mathf.Clamp(_HistoryIndex, 0, _History.Count - 1), options);
            if (newIndex != _HistoryIndex)
            {
                _HistoryIndex = newIndex;
                ApplyHistory(_History[_HistoryIndex]);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Use", GUILayout.Width(80)))
                ApplyHistory(_History[_HistoryIndex]);

            using (new EditorGUI.DisabledScope(DebugConnectionEditorClient.IsRunning))
            {
                if (GUILayout.Button("Connect", GUILayout.Width(100)))
                {
                    ApplyHistory(_History[_HistoryIndex]);
                    Connect();
                }

                if (GUILayout.Button("Delete", GUILayout.Width(80)))
                    DeleteSelectedHistory();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawTarget()
        {
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);

            DebugConnectionTargetInfo target = DebugConnectionEditorClient.GetTargetInfo();
            if (!target.IsConnected)
            {
                EditorGUILayout.HelpBox("No connected player.", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Name", target.DisplayName);
            EditorGUILayout.LabelField("Host", target.Host);
            EditorGUILayout.LabelField("Port", target.Port.ToString());
            EditorGUILayout.LabelField("Remote", target.RemoteEndPoint);
            EditorGUILayout.LabelField("Connected UTC", target.ConnectedUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            EditorGUILayout.EndVertical();
        }

        private void Connect()
        {
            int port = GetPortForCurrentMode();
            EditorPrefs.SetString(CHostKey, _Host);
            EditorPrefs.SetInt(CPortKey, port);
            EditorPrefs.SetBool(CAutoPortKey, _AutoPort);
            SaveHistory();
            if (_AutoPort)
                DebugConnectionEditorClient.ConnectAutoPort(_Host, DebugConnectionServer.DefaultPort, DebugConnectionServer.DefaultPortScanCount);
            else
                DebugConnectionEditorClient.Connect(_Host, port);
            Repaint();
        }

        private void SaveHistory()
        {
            DebugConnectionHistoryRecord record = DebugConnectionHistory.SaveOrUpdate(
                _Name,
                _Host,
                GetPortForCurrentMode(),
                _AutoPort,
                DebugConnectionServer.DefaultPortScanCount);
            ReloadHistory();

            if (record != null)
                SelectHistory(record.Host);

            Repaint();
        }

        private void DeleteSelectedHistory()
        {
            if (_HistoryIndex < 0 || _HistoryIndex >= _History.Count)
                return;

            DebugConnectionHistory.Remove(_History[_HistoryIndex].Host);
            ReloadHistory();
            Repaint();
        }

        private void ReloadHistory()
        {
            _History = DebugConnectionHistory.GetRecords();
            if (_History.Count == 0)
            {
                _HistoryIndex = -1;
                return;
            }

            SelectHistory(_Host);
            if (_HistoryIndex < 0)
                _HistoryIndex = 0;
        }

        private void SelectHistory(string host)
        {
            _HistoryIndex = -1;
            if (string.IsNullOrWhiteSpace(host))
                return;

            for (int i = 0; i < _History.Count; i++)
            {
                if (!string.Equals(_History[i].Host, host.Trim(), System.StringComparison.OrdinalIgnoreCase))
                    continue;

                _HistoryIndex = i;
                return;
            }
        }

        private void ApplyHistory(DebugConnectionHistoryRecord record)
        {
            if (record == null)
                return;

            _Name = record.Name;
            _Host = record.Host;
            _AutoPort = record.AutoPort;
            _Port = _AutoPort ? DebugConnectionServer.DefaultPort : record.Port;
            EditorPrefs.SetString(CHostKey, _Host);
            EditorPrefs.SetInt(CPortKey, _Port);
            EditorPrefs.SetBool(CAutoPortKey, _AutoPort);
        }

        private int GetPortForCurrentMode()
        {
            return _AutoPort ? DebugConnectionServer.DefaultPort : _Port;
        }

        private void OnConnectionChanged()
        {
            Repaint();
        }

        private void OnTargetInfoChanged(DebugConnectionTargetInfo info)
        {
            Repaint();
        }

        private void OnError(string message)
        {
            Repaint();
        }
    }
}
