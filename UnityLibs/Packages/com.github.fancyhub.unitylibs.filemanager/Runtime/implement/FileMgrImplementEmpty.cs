/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FH.FileManagement
{
    internal sealed class FileMgrImplementEmpty : IFileMgr
    {
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        private ExtractStreamingAssetsOperation _ExtractOp = new ExtractStreamingAssetsOperationEditor();

        public FileMgrImplementEmpty()
        {

        }

        public FileManifest GetCurrentManifest()
        {
            return null;
        }

        public ExtractStreamingAssetsOperation GetExtractOperation()
        {
            return _ExtractOp;
        }

        public void Destroy()
        {
            ___obj_ver++;
        }

        public VersionInfo GetVersionInfo()
        {
            return default;
        }

        public bool Upgrade(FileManifest new_manifest, List<FileManifest.FileItem> out_need_download_list = null)
        {
            return false;
        }

        public EFileStatus FindFile(string name, out string full_path, out EFileLocation file_location)
        {
            full_path = default;
            file_location = EFileLocation.None;
            return EFileStatus.None;
        }

        public EFileStatus FindFile(FileManifest.FileItem file, out string full_path, out EFileLocation file_location)
        {
            full_path = default;
            file_location = EFileLocation.None;
            return EFileStatus.None;
        }

        public bool IsAllTagsReady(FileManifest manifest, HashSet<string> tags = null, List<FileManifest.FileItem> out_need_download_list = null)
        {
            return true;
        }

        public bool IsAllFilesReady(FileManifest manifest, HashSet<string> file_names, List<FileManifest.FileItem> out_need_download_list = null)
        {
            return true;
        }

        public void OnFileDownload(FileManifest.FileItem item)
        {
        }

        public void RefreshFileList()
        {
        }

        public Stream OpenRead(string name)
        {
            return null;
        }

        public byte[] ReadAllBytes(string name)
        {
            return null;
        }

        public List<(string file_path, bool can_delete)> GetAllFiles(FileManifest new_manifest = null)
        {
            return new List<(string file_path, bool can_delete)>();
        }

        private sealed class ExtractStreamingAssetsOperationEditor : ExtractStreamingAssetsOperation
        {
            public override bool IsDone => true;

            public override float Progress => 1.0f;

            public override bool keepWaiting => false;
        }
    }
}