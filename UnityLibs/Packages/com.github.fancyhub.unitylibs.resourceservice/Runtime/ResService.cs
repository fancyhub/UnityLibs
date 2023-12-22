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
        public FH.FileMgrConfig FileMgrConfig;
        public FH.SceneMgrConfig SceneMgrConfig;
        public FH.BundleMgrConfig BundleMgrConfig;
        public FH.ResMgrConfig ResMgrConfig;
        // Start is called before the first frame update
        protected virtual void Awake()
        {
            UnityEngine.Application.targetFrameRate = 30;
            GameObject.DontDestroyOnLoad(gameObject);
            _Init();
        }

        private void _Init()
        {
            FileMgr.Init(FileMgrConfig);

#if UNITY_EDITOR
            if (Mode == EMode.AssetDatabase)
            {
                var bundle_config = FH.AssetBundleBuilder.Ed.AssetBundleBuilderConfig.GetDefault();
                List<(string path, string address)> asset_list = bundle_config.GetAssetCollector().GetAllAssets();

                IAssetLoader asset_loader = new Res.SampleAssetLoader.AssetLoader_AssetDatabase(asset_list, _AtlasTag2Path);
                ISceneLoader scene_loader = new FH.SceneManagement.SampleSceneLoader.SceneLoader_Assetdatabase();

                FH.ResMgr.InitMgr(ResMgrConfig, asset_loader);
                FH.SceneMgr.InitMgr(SceneMgrConfig,scene_loader);
                return;
            }
#endif

            {
                IBundleLoader bundle_loader = null;
                {
                    string pre_fix = "";
                    if (Application.platform == RuntimePlatform.WindowsPlayer)
                        pre_fix = "../../";
                    else
                        pre_fix = "Bundle/";
                    bundle_loader = new FH.ABManagement.SampleBundleLoader.BundleLoader_Dir(pre_fix + "Builder/Win", BunldeManifestName);
                }
                {
                    bundle_loader = new BundleLoader_FileMgr(FileMgr.Inst, BunldeManifestName);
                }

                BundleMgr.InitMgr(BundleMgrConfig, bundle_loader);

                IAssetLoader asset_loader = new FH.AssetLoader_Bundle(BundleMgr.Inst, _AtlasTag2Path);
                ISceneLoader scene_loader = new FH.SceneLoader_Bundle(BundleMgr.Inst);

                FH.ResMgr.InitMgr(ResMgrConfig, asset_loader);
                FH.SceneMgr.InitMgr(SceneMgrConfig,scene_loader);
            }
        }

        private string _AtlasTag2Path(string tag)
        {
            return $"Assets/Res/UI/Atlas/{tag}.spriteatlasv2";
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
