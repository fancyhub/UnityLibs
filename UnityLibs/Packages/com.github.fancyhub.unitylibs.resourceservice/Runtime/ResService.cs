using UnityEngine;

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
            if (!Application.isEditor)
                Mode = EMode.Bundle;

            IAssetLoader asset_loader = null;
            switch (Mode)
            {
                case EMode.AssetDatabase:
                    asset_loader = _CreateAssetLoader_AssetDatabase();
                    break;
                case EMode.Bundle:
                    asset_loader = _CreateAssetLoader_WithBundle();
                    break;
            }

            UnityEngine.Application.targetFrameRate = 30;
            //FH.ResMgr.InitMgr(new FH.Res.SampleAssetLoader.AssetLoader_Resource(), Config);
            FH.ResMgr.InitMgr(asset_loader, Config);
        }

        private IAssetLoader _CreateAssetLoader_WithBundle()
        {
            string pre_fix = "";
            if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                pre_fix = "../../";
            }
            else
                pre_fix = "Bundle/";

            IBundleLoader bundle_loader = new BundleLoader(pre_fix+"Builder/Win");
            BundleMgrManifest manifest = BundleMgrManifest.LoadFromFile(pre_fix + "Builder/Win/manifest.json");
            BundleMgr.InitMgr(bundle_loader, manifest);
            IAssetLoader asset_loader = new FH.AssetLoader_Bundle(BundleMgr.Inst, _AtlasTag2Path);
            return asset_loader;
        }

        private IAssetLoader _CreateAssetLoader_AssetDatabase()
        {
#if UNITY_EDITOR            
            var config= FH.AssetBundleBuilder.Ed.AssetBundleBuilderConfig.GetDefault();
            var asset_list= config.GetAssetCollector().GetAllAssets();
            return new Res.SampleAssetLoader.AssetLoader_AssetDatabase(asset_list, _AtlasTag2Path);
#else
            return null;
#endif
        }

        private string _AtlasTag2Path(string tag)
        {
            return $"Assets/Res/UI/Atlas/{tag}.spriteatlasv2";
        }


        // Update is called once per frame
        void Update()
        {
            FH.ResMgr.Update();
        }

        public void OnDestroy()
        {
            FH.ResMgr.Destroy();
        }
    }

}
