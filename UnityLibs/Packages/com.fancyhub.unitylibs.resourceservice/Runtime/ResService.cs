using UnityEngine;
using System.Collections.Generic;
using System;

namespace FH
{
    public partial class ResService : MonoBehaviour
    {
        public enum EMode
        {
            AssetDatabase,
            Bundle,
        }

        [Serializable]
        public class ResServiceConfig
        {
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
        }

        public ResServiceConfig Config;
        private static ResServiceConfig GConfig;

        protected virtual void Awake()
        {
            GConfig = Config;
            UnityEngine.Application.targetFrameRate = 30;
            StartCoroutine(_Init());
        }

        // Update is called once per frame
        void Update()
        {
            FH.ResMgr.Update();
            FH.SceneMgr.Update();
            FH.FileDownloadMgr.Update();
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
            return string.Format(Config.AtlasPathFormater, tag);
        }

        private System.Collections.IEnumerator _Init()
        {
            DateUtil.FixCultureInfo();

            var mode = Config.Mode;
            if (!Application.isEditor)
                mode = EMode.Bundle;

            Log.AutoInit();

            _InitBase(mode);

            var extraOP = FileMgr.GetExtractOperation();
            if (!extraOP.IsDone)
                yield return extraOP;

            _MountVfs(mode, Config.VfsBuilderConfig);
            //TableMgr.Init(Config.TableLogLvl, new VfsTableReaderBinCreator("Table/"));
            TableMgr.Init(Config.TableLogLvl, new VfsTableReaderCsvCreator("Table/"));

            var langSettingAsset = ResMgr.Load(LangSettingAsset.CPath).Get<LangSettingAsset>();
            LocMgr.Init(Config.LocLogLvl, langSettingAsset);
            LocMgr.FuncLoader = TableMgr.LoadTranslation;

            InitDebug();
        }

        private void _InitBase(EMode mode)
        {
            //FH.SAFileSystem.EdSetObbPath(@"E:\fancyHub\UnityLibs\UnityLibs\Bundle\Player\Android\split.main.obb");
            FileMgr.InitMgr(Config.FileMgrConfig, mode == EMode.AssetDatabase);
            BundleMgr.InitMgr(Config.BundleMgrConfig, new SampleExternalLoader.BundleExternalLoader_FileMgr(FileMgr.Inst, BundleManifest.DefaultFileName), mode == EMode.AssetDatabase);

            FileDownloadMgr.Init(Config.FileDownloadMgrConfig);
            FileDownloadMgr.SetCallBack(_OnFileDonwloaded);

            //Res Manager
            {
                IResMgr.IExternalLoader res_loader = null;
                if (mode == EMode.AssetDatabase)
                {
#if UNITY_EDITOR
                    List<(string path, string address)> asset_list = null;
                    //var bundle_config = FH.AssetBundleBuilder.Ed.AssetBundleBuilderConfig.GetDefault();
                    //asset_list = bundle_config.GetAssetCollector().GetAllAssets();
                    res_loader = new SampleExternalLoader.ResExternalLoader_Composite(new SampleExternalLoader.ResExternalLoader_AssetDatabase(asset_list, _AtlasTag2Path));
#endif
                }
                else
                {
                    res_loader = new SampleExternalLoader.ResExternalLoader_Composite(new SampleExternalLoader.ResExternalLoader_Bundle(BundleMgr.Inst, _AtlasTag2Path));
                }
                ResMgr.InitMgr(Config.ResMgrConfig, res_loader);
            }


            //scene Manager
            {
                ISceneMgr.IExternalLoader scene_loader = null;
                if (mode == EMode.AssetDatabase)
                {
#if UNITY_EDITOR
                    scene_loader = new SampleExternalLoader.SceneExternaLoader_Assetdatabase();
#endif
                }
                else
                {
                    scene_loader = new SampleExternalLoader.SceneExternalLoader_Bundle(BundleMgr.Inst);
                }
                SceneMgr.InitMgr(Config.SceneMgrConfig, scene_loader);
            }

            VfsMgr.InitMgr(Config.VfsMgrConfig);
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
                switch (p.Format)
                {
                    default:
                        Log.E("unkown format {0}", p.Format);
                        break;

                    case VFSManagement.Builder.BuilderConfig.EFormat.ZipStore:
                    case VFSManagement.Builder.BuilderConfig.EFormat.ZipCompress:
                        VirtualFileSystem_Zip fs_zip = new VirtualFileSystem_Zip(p.Name, _LoadZipForVFS);
                        VfsMgr.Mount(fs_zip);
                        break;

                    case VFSManagement.Builder.BuilderConfig.EFormat.Lz4ZipStore:
                    case VFSManagement.Builder.BuilderConfig.EFormat.Lz4ZipCompress:
                        VirtualFileSystem_Lz4Zip fs_lz4zip = new VirtualFileSystem_Lz4Zip(p.Name, _LoadLz4ZipForVFS);
                        VfsMgr.Mount(fs_lz4zip);
                        break;
                }
            }
        }

        private static Lz4ZipFile _LoadLz4ZipForVFS(string name)
        {
            var status = FileMgr.FindFile(name, out var full_path, out var file_loc);

            switch (file_loc)
            {
                default:
                    Log.E("找不到文件 {0}", name);
                    return null;
                case EFileLocation.Persistent:
                    return Lz4ZipFile.LoadFromFile(full_path);

                case EFileLocation.StreamingAssets:
                    return Lz4ZipFile.LoadFromStream(SAFileSystem.OpenRead(full_path));

                case EFileLocation.Remote:
                    Log.E("Lz4ZipFile can't read remote,{0}:{1}", name, full_path);
                    return null;
            }
        }

        private static System.IO.Compression.ZipArchive _LoadZipForVFS(string name)
        {
            FileMgr.FindFile(name, out var file_path, out var file_loc);

            switch (file_loc)
            {
                default:
                    Log.E("找不到文件 {0}", name);
                    return null;

                case EFileLocation.StreamingAssets:
                    Log.E("ZipFile can't read in StreamingAssets,{0}:{1}", name, file_path);
                    return null;

                case EFileLocation.Persistent:
                    if (!System.IO.File.Exists(file_path))
                    {
                        Log.E("File Not Exist {0}:{1}", name, file_path);
                        return null;
                    }

                    try
                    {
                        System.IO.Compression.ZipArchive zipArchive = System.IO.Compression.ZipFile.OpenRead(file_path);
                        if (zipArchive == null)
                        {
                            Log.E("加载失败 {0}:{1}", name, file_path);
                            return null;
                        }
                        return zipArchive;
                    }
                    catch (Exception e)
                    {
                        Log.E(e);
                        return null;
                    }

                case EFileLocation.Remote:
                    Log.E("ZipFile can't read remote,{0}:{1}", name, file_path);
                    return null;
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
