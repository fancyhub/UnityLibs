using UnityEngine;
using System.Collections.Generic;
using System;

namespace FH
{
    public partial class ResService
    {
        private static List<FileManifest.FileItem> _SharedFileItemList = new();
        private static List<string> _SharedStringList = new();

        public static List<FileManifest.FileItem> GetNeedDownloadFilesByAssetNames(List<string> asset_list)
        {
            _SharedFileItemList.Clear();

            //1. 获取要下载的BundleList
            BundleMgr.GetAllNeedDownload(asset_list, _SharedStringList);
            if (_SharedStringList.Count == 0)
                return _SharedFileItemList;

            //2. 获取要下载的 FileItem
            _SharedFileItemList.Clear();
            FileMgr.IsAllFilesReady(_SharedStringList, _SharedFileItemList);

            return _SharedFileItemList;
        }

        public static List<FileManifest.FileItem> GetNeedDownloadFilesByAssetTags(List<string> tags)
        {
            _SharedFileItemList.Clear();
            FileMgr.IsAllFilesReady(tags, _SharedFileItemList);

            return _SharedFileItemList;
        }

        public static void DownloadFilesByAssets(List<string> asset_list)
        {
            var file_item_list = GetNeedDownloadFilesByAssetNames(asset_list);
            FileDownloadMgr.AddTasks(file_item_list);
        }

        public static List<FileDownloadJobInfo> DownloadFilesByTags(List<string> tags)
        {
            _SharedFileItemList.Clear();
            FileMgr.IsAllTagsReady(tags, _SharedFileItemList);

            List<FileDownloadJobInfo> ret = new List<FileDownloadJobInfo>();
            FileDownloadMgr.AddTasks(_SharedFileItemList, ret);
            return ret;
        }

        public static List<FileDownloadJobInfo> DownloadFilesByTags(FileManifest file_manifest, List<string> tags)
        {
            _SharedFileItemList.Clear();
            FileMgr.IsAllTagsReady(file_manifest, tags, _SharedFileItemList);

            List<FileDownloadJobInfo> ret = new List<FileDownloadJobInfo>();
            FileDownloadMgr.AddTasks(_SharedFileItemList, ret);
            return ret;
        }

        public static List<FileDownloadJobInfo> DownloadAllFiles()
        {
            _SharedFileItemList.Clear();
            FileMgr.IsAllTagsReady(null, _SharedFileItemList);
            List<FileDownloadJobInfo> ret = new List<FileDownloadJobInfo>();
            FileDownloadMgr.AddTasks(_SharedFileItemList, ret);
            return ret;
        }

        public static List<FileDownloadJobInfo> DownloadAllFiles(FileManifest file_manifest)
        {
            _SharedFileItemList.Clear();
            FileMgr.IsAllTagsReady(file_manifest, null, _SharedFileItemList);
            List<FileDownloadJobInfo> ret = new List<FileDownloadJobInfo>();
            FileDownloadMgr.AddTasks(_SharedFileItemList, ret);
            return ret;
        }
#if UNITY_2023_2_OR_NEWER
        public static async Awaitable<FileManifest> FetchRemoteFileManifest(string version)
        {
            string file_name = FileManifest.GetRemoteFileName(version);
            string dir = GConfig.FileDownloadMgrConfig.ServerUrl;
            if (!dir.EndsWith("/"))
                dir += "/";
            string full_path = dir + FileSetting.Platform.ToString() + "/" + file_name;

            try
            {
                if (!full_path.StartsWith("http://") && !full_path.StartsWith("https://"))
                {
                    string text = System.IO.File.ReadAllText(full_path);
                    return FileManifest.ReadFromText(text);
                }
                else
                {
                    await Awaitable.BackgroundThreadAsync();

                    byte[] content = HttpDownloader.RequestFileContent(full_path);
                    string text = System.Text.Encoding.UTF8.GetString(content);

                    await Awaitable.MainThreadAsync();
                    return FileManifest.ReadFromText(text);
                }
            }
            catch (Exception e)
            {
                Log.E(e);
                return null;
            }

        }

        public static async Awaitable Upgrade(FileManifest file_manifest)
        {
            TagLog Log = TagLog.Create("Upgrade", ELogLvl.Debug);
            try
            {
                //1. 检查 file_manifest 是否合法
                if (file_manifest == null || GConfig == null)
                {
                    Log.E("null");
                    return;
                }

                //1.1 版本和当前的要不一致
                if (FileMgr.GetCurrentManifest().Version == file_manifest.Version)
                {
                    Log.E("version is same");
                    return;
                }

                //1.2 基础的文件要已经下载好了
                if (!FileMgr.IsAllTagsReady(file_manifest, GConfig.FileMgrConfig.BaseTags))
                {
                    Log.E("有资源未下载");
                    return;
                }

                //2. 检查 Res, Scene 是否有正在处于加载的逻辑, 并等待结束
                var res_mgr = ResMgr.Inst;
                var scene_mgr = SceneMgr.Inst;

                Log.D("Begin ResMgr upgrade");
                var res_op = res_mgr.BeginUpgrade();
                Log.D("Begin SceneMgr upgrade");
                var scene_op = scene_mgr.BeginUpgrade();

                Log.D("Wait Resmgr async is all done");
                for (; ; )
                {
                    if (!res_op.IsDone)
                        await Awaitable.NextFrameAsync();
                    else
                        break;
                }

                Log.D("Wait SceneMgr async is all done");
                for (; ; )
                {
                    if (!scene_op.IsDone)
                        await Awaitable.NextFrameAsync();
                    else
                        break;
                }

                //3. 更新 manifest
                Log.D("begin FileMgr upgrade");
                if (!FileMgr.Upgrade(file_manifest))
                {
                    Log.E("FileMgr upgrade failed");
                    res_mgr.EndUpgrade(false);
                    scene_mgr.EndUpgrade(false);
                    return;
                }

                //4. 更新 bundle 
                Log.D("Upgrade BundleMgr");
                BundleMgr.Inst?.Upgrade();

                //5. 结束 res mgr & scene mgr 的upgrade
                Log.D("End Resmgr upgrade");
                res_mgr.EndUpgrade(true);
                Log.D("End Scene Mgr upgrade");
                scene_mgr.EndUpgrade(true);

                //6. vfs 重新挂载
                Log.D("remount vfs");
                VfsMgr.RemountAll();

                //7. TableMgr 重新加载
                Log.D("reload table mgr");
                TableMgr.ReloadAll();
                //TableMgr            

                LocMgr.Reload();


                FileMgr.DeleteUnusedFiles(file_manifest);
            }
            catch (Exception e)
            {
                Log.E(e);
            }
        }
#endif

        public static System.Collections.IEnumerator FetchRemoteFileManifestRoutine(string version, Action<FileManifest> call_back)
        {
            string file_name = FileManifest.GetRemoteFileName(version);
            string full_path = FileDownloadMgr.GetRemoteFileFullPath(file_name);


            if (!full_path.StartsWith("http://") && !full_path.StartsWith("https://"))
            {
                string text = System.IO.File.ReadAllText(full_path);
                call_back.Invoke(FileManifest.ReadFromText(text));
                yield break;
            }
            else
            {
                FileManifest file_manifest = null;
                var task = TaskQueue.AddTask(() =>
                {
                    byte[] content = HttpDownloader.RequestFileContent(full_path);
                    string text = System.Text.Encoding.UTF8.GetString(content);
                    file_manifest = FileManifest.ReadFromText(text);
                });


                for (; ; )
                {
                    if (task.IsDone())
                        break;
                    yield return null;
                }
                call_back(file_manifest);
            }
        }

        public static System.Collections.IEnumerator UpgradeRoutine(FileManifest file_manifest)
        {
            TagLog Log = TagLog.Create("Upgrade", ELogLvl.Debug);

            //1. 检查 file_manifest 是否合法
            if (file_manifest == null || GConfig == null)
            {
                Log.E("null");
                yield break;
            }

            //1.1 版本和当前的要不一致
            if (FileMgr.GetCurrentManifest().Version == file_manifest.Version)
            {
                Log.E("version is same");
                yield break;
            }

            //1.2 基础的文件要已经下载好了
            if (!FileMgr.IsAllTagsReady(file_manifest, GConfig.FileMgrConfig.BaseTags))
            {
                Log.E("有资源未下载");
                yield break;
            }

            //2. 检查 Res, Scene 是否有正在处于加载的逻辑, 并等待结束
            var res_mgr = ResMgr.Inst;
            var scene_mgr = SceneMgr.Inst;

            Log.D("Begin ResMgr upgrade");
            var res_op = res_mgr.BeginUpgrade();
            Log.D("Begin SceneMgr upgrade");
            var scene_op = scene_mgr.BeginUpgrade();

            Log.D("Wait Resmgr async is all done");
            yield return res_op;

            Log.D("Wait SceneMgr async is all done");
            yield return scene_op;

            //3. 更新 manifest
            Log.D("begin FileMgr upgrade");
            if (!FileMgr.Upgrade(file_manifest))
            {
                Log.E("FileMgr upgrade failed");
                res_mgr.EndUpgrade(false);
                scene_mgr.EndUpgrade(false);
                yield break;
            }

            //4. 更新 bundle 
            Log.D("Upgrade BundleMgr");
            BundleMgr.Inst?.Upgrade();

            //5. 结束 res mgr & scene mgr 的upgrade
            Log.D("End Resmgr upgrade");
            res_mgr.EndUpgrade(true);
            Log.D("End Scene Mgr upgrade");
            scene_mgr.EndUpgrade(true);

            //6. vfs 重新挂载
            Log.D("remount vfs");
            VfsMgr.RemountAll();

            //7. TableMgr 重新加载
            Log.D("reload table mgr");
            TableMgr.ReloadAll();
            //TableMgr            

            LocMgr.Reload();

            FileMgr.DeleteUnusedFiles(file_manifest);
        }
    }
}
