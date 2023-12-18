using FH;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FH
{
    public class ResService : MonoBehaviour
    {
        public FH.ResMgrConfig Config;
        // Start is called before the first frame update
        protected virtual void Awake()
        {
            BundleMgr.InitMgr(new BundleLoader(), BundleMgrManifest.LoadFromFile("Bundle/Builder/Win/manifest.json"));

            UnityEngine.Application.targetFrameRate = 30;
            //FH.ResMgr.InitMgr(new FH.Res.SampleAssetLoader.AssetLoader_Resource(), Config);
            FH.ResMgr.InitMgr(new FH.AssetLoader_Bundle(BundleMgr.Inst, (a) =>
            {
                return $"Assets/Res/UI/Atlas/{a}.spriteatlasv2";
            }), Config);
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
