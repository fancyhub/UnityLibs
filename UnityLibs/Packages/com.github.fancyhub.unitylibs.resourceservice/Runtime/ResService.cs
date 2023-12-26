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

        protected virtual void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
            UnityEngine.Application.targetFrameRate = 30;
            StartCoroutine(_Init());
        }

        private System.Collections.IEnumerator _Init()
        {
            LogRecorderMgr.Init();


#if UNITY_EDITOR
            if (Mode == EMode.AssetDatabase)
            {
                var bundle_config = FH.AssetBundleBuilder.Ed.AssetBundleBuilderConfig.GetDefault();
                List<(string path, string address)> asset_list = bundle_config.GetAssetCollector().GetAllAssets();

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
                yield break;
            }
#endif

            {
                //FH.SAFileSystem.EdSetObbPath(@"E:\fancyHub\UnityLibs\UnityLibs\Bundle\Player\Android\split.main.obb");
                FileMgr.Init(FileMgrConfig);
                

                IBundleMgr.IExternalLoader bundle_loader = new FH.SampleExternalLoader.BundleExternalLoader_FileMgr(FileMgr.Inst, FH.BundleManifest.DefaultFileName);

                BundleMgr.InitMgr(BundleMgrConfig, bundle_loader);

                IResMgr.IExternalLoader res_loader = new FH.SampleExternalLoader.ResExternalLoader_Bundle(BundleMgr.Inst, _AtlasTag2Path);
                ISceneMgr.IExternalLoader scene_loader = new FH.SampleExternalLoader.SceneExternalLoader_Bundle(BundleMgr.Inst);

                FH.ResMgr.InitMgr(ResMgrConfig, res_loader);
                FH.SceneMgr.InitMgr(SceneMgrConfig, scene_loader);

                VfsMgr.InitMgr(VfsMgrConfig);
                yield return FileMgr.GetExtractOperation();

                if (VfsBuilderConfig != null)
                {
                    
                    foreach (var p in VfsBuilderConfig.Items)
                    {
                        switch (p.Format)
                        {
                            case VFSManagement.Builder.BuilderConfig.EFormat.ZipStore:
                            case VFSManagement.Builder.BuilderConfig.EFormat.ZipCompress:
                                throw new Exception("目前不支持");
                                break;

                            case VFSManagement.Builder.BuilderConfig.EFormat.Lz4ZipStore:
                            case VFSManagement.Builder.BuilderConfig.EFormat.Lz4ZipCompress:
                                Lz4ZipFile zip_file = Lz4ZipFile.LoadFromFile(FileMgr.GetFilePath(p.Name));
                                if (zip_file != null)
                                {
                                    VirtualFileSystem_Lz4Zip fs_zip = new VirtualFileSystem_Lz4Zip(p.Name, zip_file);
                                    VfsMgr.Mount(fs_zip);
                                }
                                break;
                        }
                    }
                }
            }

        }

        private string _AtlasTag2Path(string tag)
        {
            return string.Format(AtlasPathFormater, tag);
        }


        // Update is called once per frame
        void Update()
        {
            FH.ResMgr.Update();
            FH.SceneMgr.Update();
        }

        public void OnDestroy()
        {
            FH.ResMgr.Destroy();
        }
    }
}
