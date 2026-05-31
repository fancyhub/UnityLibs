using System;
using UnityEngine;

namespace FH
{
    public sealed class EdResServiceSnapshot : IDisposable
    {
        public event Action<SnapShot> SnapshotReceived;

        public SnapShot LatestSnapshot { get; private set; }
        public DateTime LatestSnapshotUtc { get; private set; }
        public string LastError { get; private set; }

        public EdResServiceSnapshot()
        {
            DebugConnectionEditorClient.Connected += OnHandleConnectionEvent;
            DebugConnectionEditorClient.Disconnected += OnHandleDisconnectionEvent;
            DebugConnectionEditorClient.Register(ResService.DebugKeyPlayer2Editor, _OnHandlePlayerMessage);
        }

        public void Dispose()
        {
            DebugConnectionEditorClient.Connected -= OnHandleConnectionEvent;
            DebugConnectionEditorClient.Disconnected -= OnHandleDisconnectionEvent;
            DebugConnectionEditorClient.Unregister(ResService.DebugKeyPlayer2Editor, _OnHandlePlayerMessage);
        }

        public bool RequestSnapShot(bool includeBundleManifest)
        {
            LastError = string.Empty;
            SnapShotRequest request = new SnapShotRequest
            {
                IncludeBundleManifest = includeBundleManifest,
            };

            if (!DebugConnectionEditorClient.Send(ResService.DebugKeyEditor2Player, request.Serialize2Bytes()))
            {
                LastError = "No connected player.";
                return false;
            }

            return true;
        }

        private void OnHandleConnectionEvent()
        {
            LastError = string.Empty;
            Debug.Log("Game player connection");
        }

        private void OnHandleDisconnectionEvent()
        {
            Debug.Log("Game player disconnection");
        }

        private void _OnHandlePlayerMessage(DebugConnectionMessageEventArgs msg)
        {
            try
            {
                SnapShot snapshot = SnapShot.DeserializeFromBytes(msg.Data);
                LatestSnapshot = snapshot;
                LatestSnapshotUtc = DateTime.UtcNow;
                LastError = string.Empty;
                SnapshotReceived?.Invoke(snapshot);
            }
            catch (Exception e)
            {
                LastError = e.Message;
                Debug.LogException(e);
            }
        }
    }
}
