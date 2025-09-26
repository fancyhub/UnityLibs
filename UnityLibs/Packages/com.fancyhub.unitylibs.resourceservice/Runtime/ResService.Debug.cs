using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Networking.PlayerConnection;

namespace FH
{
    [Serializable]
    public class SnapShot
    {
        public int FrameCount;
        public List<ResSnapShotItem> ResItems = new();
        public List<BundleSnapshotItem> BundleItems = new();
        public BundleManifest BundleManifest;

        public byte[] Serialize2Bytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(this));
        }

        public static SnapShot DeserializeFromBytes(byte[] data)
        {
            return JsonUtility.FromJson<SnapShot>(System.Text.Encoding.UTF8.GetString(data));
        }
    }

    public partial class ResService
    {
        public static readonly Guid DebugKeyPlayer2Editor = new Guid("D9AB1B8D-8640-4114-965F-5BDC4E313978");
        public static readonly Guid DebugKeyEditor2Player = new Guid("4AB41548-6455-46FF-A692-F794E5A7D986");

        public void InitDebug()
        {
            PlayerConnection.instance.Register(DebugKeyEditor2Player, _OnHandleEditorMessage);
        }

        private void _OnHandleEditorMessage(MessageEventArgs args)
        {
            SnapShot snapshot = new SnapShot();
            snapshot.FrameCount = Time.frameCount;
            ResMgr.Snapshot(ref snapshot.ResItems);
            BundleMgr.Snapshot(ref snapshot.BundleItems);
            snapshot.BundleManifest = BundleMgr.GetBundleManifest();


            byte[] data = snapshot.Serialize2Bytes();
            PlayerConnection.instance.Send(DebugKeyPlayer2Editor, data);
        }

        public void _RequestPlayerSnapshot()
        {

        }
    }

#if UNITY_EDITOR

    public class EdResServiceSnapshot
    {
        public EdResServiceSnapshot()
        {
            UnityEditor.Networking.PlayerConnection.EditorConnection.instance.Initialize();
            UnityEditor.Networking.PlayerConnection.EditorConnection.instance.RegisterConnection(OnHandleConnectionEvent);
            UnityEditor.Networking.PlayerConnection.EditorConnection.instance.RegisterDisconnection(OnHandleDisconnectionEvent);
            UnityEditor.Networking.PlayerConnection.EditorConnection.instance.Register(ResService.DebugKeyPlayer2Editor, _OnHandlePlayerMessage);
        }

        public void RequestSnapShot()
        {
            UnityEditor.Networking.PlayerConnection.EditorConnection.instance.Send(ResService.DebugKeyPlayer2Editor, null);
        }


        private void OnHandleConnectionEvent(int playerId)
        {
            Debug.Log($"Game player connection : {playerId}");
        }

        private void OnHandleDisconnectionEvent(int playerId)
        {
            Debug.Log($"Game player disconnection : {playerId}");
        }

        private void _OnHandlePlayerMessage(MessageEventArgs msg)
        {
            SnapShot snapshot = SnapShot.DeserializeFromBytes(msg.data);
        }
    }
#endif
}
