using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

namespace FH
{
    public sealed class DebugConnectionPopupContent : PopupWindowContent
    {
        private const string CHostKey = "FH.DebugConnection.Host";
        private const string CPortKey = "FH.DebugConnection.Port";
        private const string CAutoPortKey = "FH.DebugConnection.AutoPort";

        private readonly Action _OwnerRepaint;
        private string _Host;
        private int _Port;
        private bool _AutoPort;
        private string _Name;
        private int _HistoryIndex = -1;
        private List<DebugConnectionHistoryRecord> _History = new List<DebugConnectionHistoryRecord>();


        public static void DrawGUI(Action ownerRepaint = null)
        {
            Rect connectionRect = GUILayoutUtility.GetRect(
                new GUIContent(GetStateText()),
                EditorStyles.toolbarDropDown,
                GUILayout.Width(112));
            if (GUI.Button(connectionRect, GetStateText(), EditorStyles.toolbarDropDown))
                PopupWindow.Show(connectionRect, new DebugConnectionPopupContent(ownerRepaint));
        }

        public DebugConnectionPopupContent(Action ownerRepaint)
        {
            _OwnerRepaint = ownerRepaint;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(360, 300);
        }

        public override void OnOpen()
        {
            _Host = EditorPrefs.GetString(CHostKey, "127.0.0.1");
            _Port = EditorPrefs.GetInt(CPortKey, DebugConnectionServer.DefaultPort);
            _AutoPort = EditorPrefs.GetBool(CAutoPortKey, false);
            _Name = _Host;
            ReloadHistory();
            DebugConnectionEditorClient.Connected += RepaintAll;
            DebugConnectionEditorClient.Disconnected += RepaintAll;
            DebugConnectionEditorClient.Error += OnError;
            DebugConnectionEditorClient.TargetInfoChanged += OnTargetInfoChanged;
        }

        public override void OnClose()
        {
            DebugConnectionEditorClient.Connected -= RepaintAll;
            DebugConnectionEditorClient.Disconnected -= RepaintAll;
            DebugConnectionEditorClient.Error -= OnError;
            DebugConnectionEditorClient.TargetInfoChanged -= OnTargetInfoChanged;
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Space(8);
            using (new EditorGUILayout.VerticalScope())
            {
                DrawTarget();
                EditorGUILayout.Space(6);
                DrawConnectionControls();
                EditorGUILayout.Space(6);
                DrawHistoryControls();
            }
        }

        private void DrawTarget()
        {
            DebugConnectionTargetInfo target = DebugConnectionEditorClient.GetTargetInfo();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("State", GetStateText());
                if (target.IsConnected)
                {
                    EditorGUILayout.LabelField("Name", string.IsNullOrEmpty(target.DisplayName) ? target.RemoteEndPoint : target.DisplayName);
                    EditorGUILayout.LabelField("Remote", target.RemoteEndPoint);
                    EditorGUILayout.LabelField("Port", target.Port.ToString());
                }
                else if (!string.IsNullOrEmpty(DebugConnectionEditorClient.LastError))
                {
                    EditorGUILayout.HelpBox(DebugConnectionEditorClient.LastError, MessageType.Warning);
                }
            }
        }

        private void DrawConnectionControls()
        {
            using (new EditorGUI.DisabledScope(DebugConnectionEditorClient.IsRunning))
            {
                _Name = EditorGUILayout.TextField("Name", _Name);
                _Host = EditorGUILayout.TextField("Host", _Host);
                _AutoPort = EditorGUILayout.Toggle("Auto Port", _AutoPort);

                if (_AutoPort)
                {
                    int lastPort = DebugConnectionServer.DefaultPort + DebugConnectionServer.DefaultPortScanCount - 1;
                    EditorGUILayout.LabelField(
                        "Port Range",
                        DebugConnectionServer.DefaultPort + " - " + lastPort);
                }
                else
                {
                    _Port = EditorGUILayout.IntField("Port", _Port);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
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
                    {
                        DebugConnectionEditorClient.Disconnect();
                        RepaintAll();
                    }
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void DrawHistoryControls()
        {
            if (_History.Count == 0)
            {
                EditorGUILayout.HelpBox("No history records.", MessageType.Info);
                return;
            }

            string[] options = new string[_History.Count];
            for (int i = 0; i < _History.Count; i++)
                options[i] = _History[i].DisplayName;

            int newIndex = EditorGUILayout.Popup("History", Mathf.Clamp(_HistoryIndex, 0, _History.Count - 1), options);
            if (newIndex != _HistoryIndex)
            {
                _HistoryIndex = newIndex;
                ApplyHistory(_History[_HistoryIndex]);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use", GUILayout.Width(80)))
                    ApplySelectedHistory();

                using (new EditorGUI.DisabledScope(DebugConnectionEditorClient.IsRunning))
                {
                    if (GUILayout.Button("Connect", GUILayout.Width(100)))
                    {
                        ApplySelectedHistory();
                        Connect();
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(80)))
                        DeleteSelectedHistory();
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void Connect()
        {
            int port = GetPortForCurrentMode();
            EditorPrefs.SetString(CHostKey, _Host);
            EditorPrefs.SetInt(CPortKey, port);
            EditorPrefs.SetBool(CAutoPortKey, _AutoPort);
            SaveHistory();

            if (_AutoPort)
            {
                DebugConnectionEditorClient.ConnectAutoPort(
                    _Host,
                    DebugConnectionServer.DefaultPort,
                    DebugConnectionServer.DefaultPortScanCount);
            }
            else
            {
                DebugConnectionEditorClient.Connect(_Host, port);
            }

            RepaintAll();
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

            RepaintAll();
        }

        private void ApplySelectedHistory()
        {
            if (_HistoryIndex < 0 || _HistoryIndex >= _History.Count)
                return;

            ApplyHistory(_History[_HistoryIndex]);
        }

        private void DeleteSelectedHistory()
        {
            if (_HistoryIndex < 0 || _HistoryIndex >= _History.Count)
                return;

            DebugConnectionHistory.Remove(_History[_HistoryIndex].Host);
            ReloadHistory();
            RepaintAll();
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
                if (!string.Equals(_History[i].Host, host.Trim(), StringComparison.OrdinalIgnoreCase))
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
            RepaintAll();
        }

        private int GetPortForCurrentMode()
        {
            return _AutoPort ? DebugConnectionServer.DefaultPort : _Port;
        }

        private static string GetStateText()
        {
            if (DebugConnectionEditorClient.IsConnected)
                return "Connected";
            return DebugConnectionEditorClient.IsRunning ? "Connecting" : "Disconnected";
        }

        private void OnTargetInfoChanged(DebugConnectionTargetInfo info)
        {
            RepaintAll();
        }

        private void OnError(string message)
        {
            RepaintAll();
        }

        private void RepaintAll()
        {
            editorWindow?.Repaint();
            _OwnerRepaint?.Invoke();
        }
    }
}
