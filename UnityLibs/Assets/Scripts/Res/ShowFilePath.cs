using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowFilePath : MonoBehaviour
{
    [System.Serializable]
    public struct Item
    {
        public string Name;
        public string FullName;
        public string FilePath;
        public FH.EFileLocation Location;
        public FH.EFileStatus Status;
    }

    public List<Item> StreamingAssetsFileList = new();
    public List<Item> LocalFileList = new();
    public List<Item> NeedDownloadFileList = new();

    [FH.Omi.Button]
    public void UpdateFileList()
    {
        StreamingAssetsFileList.Clear();
        LocalFileList.Clear();
        NeedDownloadFileList.Clear();

        var file_manifest = FH.FileMgr.GetCurrentManifest();
        if (file_manifest == null)
            return;
        foreach (var p in file_manifest.Files)
        {
            var item = new Item();
            item.Name = p.Name;
            item.FullName = p.FullName;

            item.Status = FH.FileMgr.FindFile(p.Name, out item.FilePath, out item.Location);

            switch (item.Location)
            {
                default:
                    Debug.LogError("未知类型 " + item.Location);
                    break;

                case FH.EFileLocation.Persistent:
                    LocalFileList.Add(item);
                    break;
                case FH.EFileLocation.Remote:
                    NeedDownloadFileList.Add(item);
                    break;
                case FH.EFileLocation.StreamingAssets:
                    StreamingAssetsFileList.Add(item);
                    break;
            }
        }
    }
}
