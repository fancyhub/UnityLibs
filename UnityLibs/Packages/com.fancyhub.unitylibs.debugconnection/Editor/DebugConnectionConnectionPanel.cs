/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FH
{
    internal sealed class DebugConnectionConnectionPanel : IDisposable
    {
        private const string CHostKey = "FH.DebugConnection.Host";
        private const string CPortKey = "FH.DebugConnection.Port";
        private const string CAutoPortKey = "FH.DebugConnection.AutoPort";

        private readonly Action _Repaint;
        private string _Host;
        private int _Port;
        private bool _AutoPort;
        private string _Name;
        private int _HistoryIndex = -1;
        private List<DebugConnectionHistoryRecord> _History = new List<DebugConnectionHistoryRecord>();

        public DebugConnectionConnectionPanel(Action repaint)
        {
            _Repaint = repaint;
            LoadPrefs();
            ReloadHistory();

            DebugConnectionEditorClient.Connected += OnConnectionChanged;
            DebugConnectionEditorClient.Disconnected += OnConnectionChanged;
            DebugConnectionEditorClient.Error += OnError;
            DebugConnectionEditorClient.TargetInfoChanged += OnTargetInfoChanged;
        }

        public void Dispose()
        {
            DebugConnectionEditorClient.Connected -= OnConnectionChanged;
            DebugConnectionEditorClient.Disconnected -= OnConnectionChanged;
            DebugConnectionEditorClient.Error -= OnError;
            DebugConnectionEditorClient.TargetInfoChanged -= OnTargetInfoChanged;
        }

        public void DrawWindowGUI()
        {
            DrawConnectionControls(true);
            EditorGUILayout.Space(8);
            DrawHistoryControls(true);
            EditorGUILayout.Space(8);
            DrawTargetDetails();
        }

        public void DrawPopupGUI()
        {
            GUILayout.Space(8);
            using (new EditorGUILayout.VerticalScope())
            {
                DrawStateBox();
                EditorGUILayout.Space(6);
                DrawConnectionControls(false);
                EditorGUILayout.Space(6);
                DrawHistoryControls(false);
            }
        }

        public static string GetStateText()
        {
            if (DebugConnectionEditorClient.IsConnected)
                return "Connected";

            return DebugConnectionEditorClient.IsRunning ? "Connecting" : "Disconnected";
        }

        private void DrawConnectionControls(bool showTitle)
        {
            if (showTitle)
                EditorGUILayout.LabelField("Remote Player", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(DebugConnectionEditorClient.IsRunning))
            {
                _Name = EditorGUILayout.TextField("Name", _Name);
                _Host = EditorGUILayout.TextField("Host", _Host);
                _AutoPort = EditorGUILayout.Toggle("Auto Port", _AutoPort);

                if (_AutoPort)
                {
                    int lastPort = DebugConnectionServer.DefaultPort + DebugConnectionServer.DefaultPortScanCount - 1;
                    EditorGUILayout.LabelField("Port Range", DebugConnectionServer.DefaultPort + " - " + lastPort);
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
                        Repaint();
                    }
                }

                GUILayout.FlexibleSpace();

                if (showTitle)
                    EditorGUILayout.LabelField(GetStateText(), GUILayout.Width(180));
            }

            if (showTitle && !string.IsNullOrEmpty(DebugConnectionEditorClient.LastError))
                EditorGUILayout.HelpBox(DebugConnectionEditorClient.LastError, MessageType.Warning);
        }

        private void DrawHistoryControls(bool showTitle)
        {
            if (showTitle)
                EditorGUILayout.LabelField("History", EditorStyles.boldLabel);

            if (_History.Count == 0)
            {
                EditorGUILayout.HelpBox("No history records.", MessageType.Info);
                return;
            }

            string[] options = new string[_History.Count];
            for (int i = 0; i < _History.Count; i++)
                options[i] = _History[i].DisplayName;

            string label = showTitle ? "Record" : "History";
            int newIndex = EditorGUILayout.Popup(label, Mathf.Clamp(_HistoryIndex, 0, _History.Count - 1), options);
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

        private void DrawStateBox()
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

        private void DrawTargetDetails()
        {
            EditorGUILayout.LabelField("Target", EditorStyles.boldLabel);

            DebugConnectionTargetInfo target = DebugConnectionEditorClient.GetTargetInfo();
            if (!target.IsConnected)
            {
                EditorGUILayout.HelpBox("No connected player.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Name", target.DisplayName);
                EditorGUILayout.LabelField("Host", target.Host);
                EditorGUILayout.LabelField("Port", target.Port.ToString());
                EditorGUILayout.LabelField("Remote", target.RemoteEndPoint);
                EditorGUILayout.LabelField("Connected UTC", target.ConnectedUtc.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        private void Connect()
        {
            int port = GetPortForCurrentMode();
            SavePrefs(port);
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
            SavePrefs(_Port);
            Repaint();
        }

        private void LoadPrefs()
        {
            _Host = EditorPrefs.GetString(CHostKey, "127.0.0.1");
            _Port = EditorPrefs.GetInt(CPortKey, DebugConnectionServer.DefaultPort);
            _AutoPort = EditorPrefs.GetBool(CAutoPortKey, false);
            _Name = _Host;
        }

        private void SavePrefs(int port)
        {
            EditorPrefs.SetString(CHostKey, _Host);
            EditorPrefs.SetInt(CPortKey, port);
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

        private void Repaint()
        {
            _Repaint?.Invoke();
        }
    }
}
