using UnityEngine;
using System.Collections.Generic;
using System;

namespace FH
{
    public class ResService : MonoBehaviour
    {
        public enum EMode
        {
            AssetDatabase,
            Bundle,
        }

        public EMode Mode = EMode.AssetDatabase;
        public string AtlasPathFormater = "Assets/Res/UI/Atlas/{0}.spriteatlasv2";
        public FH.IFileMgr.Config FileMgrConfig;
        public FH.IBundleMgr.Config BundleMgrConfig;
        public FH.IResMgr.Config ResMgrConfig;
        public FH.ISceneMgr.Config SceneMgrConfig;
        public FH.IVfsMgr.Config VfsMgrConfig;
        public FH.VFSManagement.Builder.BuilderConfig VfsBuilderConfig;
        public FH.IFileDownloadMgr.Config FileDownloadMgrConfig;
        public ELogLvl TableLogLvl = ELogLvl.Info;
        public ELogLvl LocLogLvl = ELogLvl.Info;


        protected virtual void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
            UnityEngine.Application.targetFrameRate = 30;
            StartCoroutine(_Init());
        }

        // Update is called once per frame
        void Update()
        {
            FH.ResMgr.Update();
            FH.SceneMgr.Update();
            FileDownloadMgr.Update();
        }

        public void OnDestroy()
        {
            FH.TableMgr.Destroy();
            FH.ResMgr.Destroy();
            FH.VfsMgr.Destroy();
            FH.BundleMgr.Destroy();
        }

        private string _AtlasTag2Path(string tag)
        {
            return string.Format(AtlasPathFormater, tag);
        }

        private System.Collections.IEnumerator _Init()
        {
            var mode = Mode;
            if (!Application.isEditor)
                mode = EMode.Bundle;

            Log.AutoInit();

            _InitBase(mode);

            var extraOP = FileMgr.GetExtractOperation();
            if (!extraOP.IsDone)
                yield return extraOP;

            _MountVfs(mode, VfsBuilderConfig);
            TableMgr.Init(TableLogLvl, new VfsTableReaderBinCreator("Table/"));
            LocMgr.InitLog(LocLogLvl);
            LocMgr.FuncLoader = TableMgr.LoadTranslation;
        }

        private void _InitBase(EMode mode)
        {
            //FH.SAFileSystem.EdSetObbPath(@"E:\fancyHub\UnityLibs\UnityLibs\Bundle\Player\Android\split.main.obb");
            FileMgr.InitMgr(FileMgrConfig, mode == EMode.AssetDatabase);
            BundleMgr.InitMgr(BundleMgrConfig, new SampleExternalLoader.BundleExternalLoader_FileMgr(FileMgr.Inst, BundleManifest.DefaultFileName), mode == EMode.AssetDatabase);

            FileDownloadMgr.Init(FileDownloadMgrConfig);
            FileDownloadMgr.SetCallBack(_OnFileDonwloaded);

            IResMgr.IExternalLoader res_loader = null;
            ISceneMgr.IExternalLoader scene_loader = null;

            if (mode == EMode.AssetDatabase)
            {
#if UNITY_EDITOR
                List<(string path, string address)> asset_list = null;
                //var bundle_config = FH.AssetBundleBuilder.Ed.AssetBundleBuilderConfig.GetDefault();
                //asset_list = bundle_config.GetAssetCollector().GetAllAssets();
                res_loader = new SampleExternalLoader.ResExternalLoader_Composite(new SampleExternalLoader.ResExternalLoader_AssetDatabase(asset_list, _AtlasTag2Path));

                scene_loader = new SampleExternalLoader.SceneExternaLoader_Assetdatabase();
#endif
            }
            else
            {
                res_loader = new SampleExternalLoader.ResExternalLoader_Composite(new SampleExternalLoader.ResExternalLoader_Bundle(BundleMgr.Inst, _AtlasTag2Path));
                scene_loader = new SampleExternalLoader.SceneExternalLoader_Bundle(BundleMgr.Inst);
            }

            ResMgr.InitMgr(ResMgrConfig, res_loader);
            SceneMgr.InitMgr(SceneMgrConfig, scene_loader);
            VfsMgr.InitMgr(VfsMgrConfig);
        }

        private static void _MountVfs(EMode mode, VFSManagement.Builder.BuilderConfig config)
        {
            if (config == null)
                return;
            if (mode == EMode.AssetDatabase)
            {
                foreach (var p in config.Items)
                {
                    FH.VirtualFileSystem_Dir fs = new VirtualFileSystem_Dir(p.Name);
                    foreach (var p2 in p.Dirs)
                    {
                        fs.AddDir(p2.RootDir, p2.SpecSubDir);
                    }
                    VfsMgr.Mount(fs);
                }
                return;
            }

            foreach (var p in config.Items)
            {
                FileMgr.FindFile(p.Name, out var full_path, out var _);
                if (full_path == null)
                {
                    Log.E("找不到文件 {0}", p.Name);
                    continue;
                }

                switch (p.Format)
                {
                    case VFSManagement.Builder.BuilderConfig.EFormat.ZipStore:
                    case VFSManagement.Builder.BuilderConfig.EFormat.ZipCompress:
                        {
                            VirtualFileSystem_Zip fs_zip = VirtualFileSystem_Zip.CreateFromFile(p.Name, full_path);
                            VfsMgr.Mount(fs_zip);
                        }
                        break;

                    case VFSManagement.Builder.BuilderConfig.EFormat.Lz4ZipStore:
                    case VFSManagement.Builder.BuilderConfig.EFormat.Lz4ZipCompress:
                        {
                            VirtualFileSystem_Lz4Zip fs_zip = VirtualFileSystem_Lz4Zip.CreateFromFile(p.Name, full_path);
                            VfsMgr.Mount(fs_zip);
                        }
                        break;
                }
            }
        }


        private static void _OnFileDonwloaded(FileDownloadJobInfo info)
        {
            if (info.Status != EFileDownloadStatus.Succ)
                return;
            FileManifest.FileItem file_item = info.GetUserData<FileManifest.FileItem>();
            if (file_item == null)
                return;

            FileMgr.OnFileDownloaded(file_item);
        }
    }
}
