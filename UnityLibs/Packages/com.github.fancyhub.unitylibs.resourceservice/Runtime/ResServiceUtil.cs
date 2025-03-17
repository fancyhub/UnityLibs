using UnityEngine;
using System.Collections.Generic;
using System;

namespace FH
{
    public static class ResServiceUtil
    {
        private static List<FileManifest.FileItem> _SharedFileItemList = new();
        private static List<IBundle> _SharedBundleList = new();
        private static List<string> _SharedStringList = new();

        public static List<FileManifest.FileItem> GetNeedDownloadFilesByAssetNames(List<string> asset_list)
        {
            _SharedFileItemList.Clear();
            _SharedBundleList.Clear();

            //1. 获取要下载的BundleList
            BundleMgr.GetAllNeedDownload(asset_list, _SharedBundleList);
            if (_SharedFileItemList.Count == 0)
                return _SharedFileItemList;

            //2. 获取要下载的文件名列表
            _SharedStringList.Clear();
            foreach (var p in _SharedBundleList)
            {
                _SharedStringList.Add(p.Name);
            }

            //3. 获取要下载的 FileItem
            _SharedFileItemList.Clear();
            FileMgr.IsAllFilesReady(_SharedStringList, _SharedFileItemList);

            return _SharedFileItemList;
        }

        public static List<FileManifest.FileItem> GetNeedDownloadFilesByAssetTags(List<string> tags)
        {
            _SharedFileItemList.Clear();
            
            foreach (var p in _SharedBundleList)
            {
                _SharedStringList.Add(p.Name);
            }

            //3. 获取要下载的 FileItem
            _SharedFileItemList.Clear();
            FileMgr.IsAllFilesReady(_SharedStringList, _SharedFileItemList);

            return _SharedFileItemList;
        }

        public static void DownloadFilesByAssets(List<string> asset_list)
        {
            var file_item_list = GetNeedDownloadFilesByAssetNames(asset_list);
            FileDownloadMgr.AddTasks(file_item_list);
        }

        public static void DownloadFilesByTags(List<string> tags)
        {
            _SharedFileItemList.Clear();
            FileMgr.IsAllTagsReady(tags, _SharedFileItemList);
            FileDownloadMgr.AddTasks(_SharedFileItemList);
        }

        public static void DownloadAllFiles()
        {
            _SharedFileItemList.Clear();
            FileMgr.IsAllTagsReady(null, _SharedFileItemList);
            FileDownloadMgr.AddTasks(_SharedFileItemList);
        }
    }
}
