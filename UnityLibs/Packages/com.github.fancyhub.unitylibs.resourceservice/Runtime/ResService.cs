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
        public FH.FileMgrConfig FileMgrConfig;
        public FH.BundleMgrConfig BundleMgrConfig;
        public FH.ResMgrConfig ResMgrConfig;
        public FH.SceneMgrConfig SceneMgrConfig;


        protected virtual void Awake()
        {
            GameObject.DontDestroyOnLoad(gameObject);
            UnityEngine.Application.targetFrameRate = 30;
            _Init();
        }

        private void _Init()
        {
            LogRecorderMgr.Init();
            FileMgr.Init(FileMgrConfig);

#if UNITY_EDITOR
            if (Mode == EMode.AssetDatabase)
            {
                var bundle_config = FH.AssetBundleBuilder.Ed.AssetBundleBuilderConfig.GetDefault();
                List<(string path, string address)> asset_list = bundle_config.GetAssetCollector().GetAllAssets();

                IAssetLoader asset_loader = new Res.SampleAssetLoader.AssetLoader_AssetDatabase(asset_list, _AtlasTag2Path);
                ISceneLoader scene_loader = new FH.SceneManagement.SampleSceneLoader.SceneLoader_Assetdatabase();

                FH.ResMgr.InitMgr(ResMgrConfig, asset_loader);
                FH.SceneMgr.InitMgr(SceneMgrConfig, scene_loader);
                return;
            }
#endif

            {
                IBundleLoader bundle_loader = new BundleLoader_FileMgr(FileMgr.Inst, BunldeManifestName);

                BundleMgr.InitMgr(BundleMgrConfig, bundle_loader);

                IAssetLoader asset_loader = new FH.AssetLoader_Bundle(BundleMgr.Inst, _AtlasTag2Path);
                ISceneLoader scene_loader = new FH.SceneLoader_Bundle(BundleMgr.Inst);

                FH.ResMgr.InitMgr(ResMgrConfig, asset_loader);
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
