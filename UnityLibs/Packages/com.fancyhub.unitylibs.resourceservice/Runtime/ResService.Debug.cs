using UnityEngine;
using System.Collections.Generic;
using System;

namespace FH
{
    [Serializable]
    public class SnapShot
    {
        public int FrameCount;
        public string Error;
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

    [Serializable]
    public class SnapShotRequest
    {
        public bool IncludeBundleManifest;

        public byte[] Serialize2Bytes()
        {
            return System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(this));
        }

        public static SnapShotRequest DeserializeFromBytes(byte[] data)
        {
            if (data == null || data.Length == 0)
                return new SnapShotRequest();

            return JsonUtility.FromJson<SnapShotRequest>(System.Text.Encoding.UTF8.GetString(data)) ?? new SnapShotRequest();
        }
    }

    public partial class ResService
    {
        public static readonly Guid DebugKeyPlayer2Editor = new Guid("D9AB1B8D-8640-4114-965F-5BDC4E313978");
        public static readonly Guid DebugKeyEditor2Player = new Guid("4AB41548-6455-46FF-A692-F794E5A7D986");

        public void InitDebug()
        {
            DebugConnection.Register(DebugKeyEditor2Player, _OnHandleEditorMessage);
        }

        private void _OnHandleEditorMessage(DebugConnectionMessageEventArgs args)
        {
            try
            {
                SnapShotRequest request = SnapShotRequest.DeserializeFromBytes(args.Data);
                SnapShot snapshot = new SnapShot();
                snapshot.FrameCount = Time.frameCount;
                ResMgr.Snapshot(ref snapshot.ResItems);
                BundleMgr.Snapshot(ref snapshot.BundleItems);
                if (request.IncludeBundleManifest)
                    snapshot.BundleManifest = BundleMgr.GetBundleManifest();

                PrepareForSerialization(snapshot);
                DebugConnection.Send(DebugKeyPlayer2Editor, SerializeForSend(snapshot));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                SendErrorSnapshot(e.Message);
            }
        }

        public void _RequestPlayerSnapshot()
        {

        }

        private static void PrepareForSerialization(SnapShot snapshot)
        {
            if (snapshot == null)
                return;

            if (snapshot.ResItems != null)
            {
                for (int i = 0; i < snapshot.ResItems.Count; i++)
                {
                    ResSnapShotItem item = snapshot.ResItems[i];
                    item.Users = null;
                    snapshot.ResItems[i] = item;
                }
            }      

        }

        private static byte[] SerializeForSend(SnapShot snapshot)
        {
            byte[] data = snapshot.Serialize2Bytes();
            if (data.Length <= DebugConnection.MaxPayloadSize)
                return data;

            if (snapshot.BundleManifest != null)
            {
                snapshot.BundleManifest = null;
                snapshot.Error = "Snapshot payload was too large. BundleManifest was omitted.";
                data = snapshot.Serialize2Bytes();
                if (data.Length <= DebugConnection.MaxPayloadSize)
                    return data;
            }

            snapshot.ResItems.Clear();
            snapshot.BundleItems.Clear();
            snapshot.BundleManifest = null;
            snapshot.Error = "Snapshot payload was too large: " + data.Length + " bytes.";
            return snapshot.Serialize2Bytes();
        }

        private static void SendErrorSnapshot(string error)
        {
            SnapShot snapshot = new SnapShot
            {
                FrameCount = Time.frameCount,
                Error = error,
            };

            try
            {
                DebugConnection.Send(DebugKeyPlayer2Editor, snapshot.Serialize2Bytes());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
