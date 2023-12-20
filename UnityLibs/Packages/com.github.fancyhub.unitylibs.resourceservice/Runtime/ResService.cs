using UnityEngine;
using System.Collections.Generic;

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
        public FH.ResMgrConfig Config;
        // Start is called before the first frame update
        protected virtual void Awake()
        {
            UnityEngine.Application.targetFrameRate = 30;
            GameObject.DontDestroyOnLoad(gameObject);
            _Init();
        }

        private void _Init()
        {

#if UNITY_EDITOR
            if (Mode == EMode.AssetDatabase)
            {
                var bundle_config = FH.AssetBundleBuilder.Ed.AssetBundleBuilderConfig.GetDefault();
                List<(string path, string address)> asset_list = bundle_config.GetAssetCollector().GetAllAssets();

                IAssetLoader asset_loader = new Res.SampleAssetLoader.AssetLoader_AssetDatabase(asset_list, _AtlasTag2Path);
                ISceneLoader scene_loader = new FH.SceneManagement.SampleSceneLoader.SceneLoader_Assetdatabase();

                FH.ResMgr.InitMgr(asset_loader, Config);
                FH.SceneMgr.InitMgr(scene_loader);
                return;
            }
#endif

            {
                string pre_fix = "";
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                    pre_fix = "../../";
                else
                    pre_fix = "Bundle/";


                IBundleLoader bundle_loader = new BundleLoader(pre_fix + "Builder/Win");
                BundleMgrManifest manifest = BundleMgrManifest.LoadFromFile(pre_fix + "Builder/Win/manifest.json");
                BundleMgr.InitMgr(bundle_loader, manifest);

                IAssetLoader asset_loader = new FH.AssetLoader_Bundle(BundleMgr.Inst, _AtlasTag2Path);
                ISceneLoader scene_loader = new FH.SceneLoader_Bundle(BundleMgr.Inst);
                
                FH.ResMgr.InitMgr(asset_loader, Config);
                FH.SceneMgr.InitMgr(scene_loader);
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
