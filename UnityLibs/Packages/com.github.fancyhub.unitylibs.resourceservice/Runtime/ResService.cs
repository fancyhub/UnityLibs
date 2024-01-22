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
        }

        private string _AtlasTag2Path(string tag)
        {
            return string.Format(AtlasPathFormater, tag);
        }

        private System.Collections.IEnumerator _Init()
        {
            Log.AutoInit();
            
#if UNITY_EDITOR
            if (Mode == EMode.AssetDatabase)
            {
                List<(string path, string address)> asset_list = null;
                //var bundle_config = FH.AssetBundleBuilder.Ed.AssetBundleBuilderConfig.GetDefault();
                //asset_list = bundle_config.GetAssetCollector().GetAllAssets();

                IResMgr.IExternalLoader res_loader = new FH.SampleExternalLoader.ResExternalLoader_AssetDatabase(asset_list, _AtlasTag2Path);
                ISceneMgr.IExternalLoader scene_loader = new FH.SampleExternalLoader.SceneExternaLoader_Assetdatabase();

                FH.ResMgr.InitMgr(ResMgrConfig, res_loader);
                FH.SceneMgr.InitMgr(SceneMgrConfig, scene_loader);
                VfsMgr.InitMgr(VfsMgrConfig);

                if (VfsBuilderConfig != null)
                {
                    foreach (var p in VfsBuilderConfig.Items)
                    {
                        FH.VirtualFileSystem_Dir fs = new VirtualFileSystem_Dir(p.Name);
                        foreach (var p2 in p.Dirs)
                        {
                            fs.AddDir(p2.RootDir, p2.SpecSubDir);
                        }
                        VfsMgr.Mount(fs);
                    }
                }

                TableMgr.Init(TableLogLvl, new VfsTableReaderCsvCreator("Table/"));
                LocMgr.InitLog(LocLogLvl);
                LocMgr.FuncLoader = TableMgr.LoadTranslation;

                yield break;
            }
#endif

            {
                //FH.SAFileSystem.EdSetObbPath(@"E:\fancyHub\UnityLibs\UnityLibs\Bundle\Player\Android\split.main.obb");
                FileMgr.Init(FileMgrConfig);
                FileDownloadMgr.Init(FileDownloadMgrConfig);

                IBundleMgr.IExternalLoader bundle_loader = new FH.SampleExternalLoader.BundleExternalLoader_FileMgr(FileMgr.Inst, FH.BundleManifest.DefaultFileName);

                BundleMgr.InitMgr(BundleMgrConfig, bundle_loader);

                IResMgr.IExternalLoader res_loader = new FH.SampleExternalLoader.ResExternalLoader_Bundle(BundleMgr.Inst, _AtlasTag2Path);
                ISceneMgr.IExternalLoader scene_loader = new FH.SampleExternalLoader.SceneExternalLoader_Bundle(BundleMgr.Inst);

                FH.ResMgr.InitMgr(ResMgrConfig, res_loader);
                FH.SceneMgr.InitMgr(SceneMgrConfig, scene_loader);

                VfsMgr.InitMgr(VfsMgrConfig);

                Log.D("Wait Extra Android Operation");
                yield return FileMgr.GetExtractOperation();
                Log.D("Extra Android Operation Done");

                if (VfsBuilderConfig != null)
                {
                    foreach (var p in VfsBuilderConfig.Items)
                    {
                        FileMgr.FindFile(p.Name, out var full_path);
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

                TableMgr.Init(TableLogLvl, new VfsTableReaderBinCreator("Table/"));
                LocMgr.InitLog(LocLogLvl);
                LocMgr.FuncLoader = TableMgr.LoadTranslation;
            }
        }
    }
}
