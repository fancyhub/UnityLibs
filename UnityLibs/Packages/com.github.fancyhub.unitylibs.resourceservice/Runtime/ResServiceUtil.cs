using UnityEngine;
using System.Collections.Generic;
using System;

namespace FH
{
    public partial class ResService
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

        public static async Awaitable Switch(FileManifest file_manifest)
        {
            //1. 检查 file_manifest 是否合法
            if (file_manifest == null || GConfig == null)
                return;

            //1.1 版本和当前的要不一致
            if (FileMgr.GetCurrentManifest().Version == file_manifest.Version)
                return;

            //1.2 基础的文件要已经下载好了
            if (!FileMgr.IsAllTagsReady(file_manifest, GConfig.FileMgrConfig.BaseTags))
            {
                Log.E("有资源未下载");
                return;
            }

            //2. 检查 Res, Scene 是否有正在处于加载的逻辑, 并等待结束
            var res_mgr = ResMgr.Inst;
            var scene_mgr = SceneMgr.Inst;
            var res_op = res_mgr.BeginUpgrade();
            var scene_op = scene_mgr.BeginUpgrade();

            for (; ; )
            {
                if (!res_op.IsDone)
                    await Awaitable.NextFrameAsync();
                else
                    break;
            }

            for (; ; )
            {
                if (!scene_op.IsDone)
                    await Awaitable.NextFrameAsync();
                else
                    break;
            }

            //3. 更新 manifest
            if (!FileMgr.Upgrade(file_manifest))
            {
                res_mgr.EndUpgrade(false);
                scene_mgr.EndUpgrade(false);
                return;
            }

            //4. 更新 bundle 
            BundleMgr.Inst?.Upgrade();

            //5. 结束 res mgr & scene mgr 的upgrade
            res_mgr.EndUpgrade(true);
            scene_mgr.EndUpgrade(true);

            //6. vfs 重新挂载
            VfsMgr.UnMountAll();
            _MountVfs(GConfig.Mode, GConfig.VfsBuilderConfig);

            //7. TableMgr 重新加载
            TableMgr.ReloadAll();
            //TableMgr            
        }
    }
}
