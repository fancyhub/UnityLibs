using UnityEngine;
using System.Collections.Generic;

namespace FH
{
    public class ResService : MonoBehaviour
    {
        public const string BunldeManifestName = "bundle_manifest.json";
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


        protected virtual void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
            UnityEngine.Application.targetFrameRate = 30;
            _Init();
        }

        private void _Init()
        {
            LogRecorderMgr.Init();

#if UNITY_EDITOR
            if (Mode == EMode.AssetDatabase)
            {
                var bundle_config = FH.AssetBundleBuilder.Ed.AssetBundleBuilderConfig.GetDefault();
                List<(string path, string address)> asset_list = bundle_config.GetAssetCollector().GetAllAssets();

                IResMgr.IExternalLoader res_loader = new FH.ResManagement.SampleExternalLoader.ResExternalLoader_AssetDatabase(asset_list, _AtlasTag2Path);
                ISceneMgr.IExternalLoader scene_loader = new FH.SceneManagement.SampleExternalLoader.SceneExternaLoader_Assetdatabase();

                FH.ResMgr.InitMgr(ResMgrConfig, res_loader);
                FH.SceneMgr.InitMgr(SceneMgrConfig, scene_loader);
                return;
            }
#endif

            {
                //FH.SAFileSystem.EdSetObbPath(@"E:\fancyHub\UnityLibs\UnityLibs\Bundle\Player\Android\split.main.obb");
                FileMgr.Init(FileMgrConfig);

                IBundleMgr.IExternalLoader bundle_loader = new BundleExternalLoader_FileMgr(FileMgr.Inst, BunldeManifestName);

                BundleMgr.InitMgr(BundleMgrConfig, bundle_loader);

                IResMgr.IExternalLoader res_loader = new FH.ResExternalLoader_Bundle(BundleMgr.Inst, _AtlasTag2Path);
                ISceneMgr.IExternalLoader scene_loader = new FH.SceneExternalLoader_Bundle(BundleMgr.Inst);

                FH.ResMgr.InitMgr(ResMgrConfig, res_loader);
                FH.SceneMgr.InitMgr(SceneMgrConfig, scene_loader);
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
