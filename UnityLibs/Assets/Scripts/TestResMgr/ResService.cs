using FH;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResService : MonoBehaviour
{

    public class BundleLoader : CPtrBase, IBundleLoader
    {
        public string Dir = "BundleCache";
        public EBundleFileStatus GetBundleFileStatus(string name)
        {
            string path = System.IO.Path.Combine(Dir, name);
            if (System.IO.File.Exists(path))            
                return EBundleFileStatus.Exist;            
            else
                return EBundleFileStatus.NoExist;
        }

        public string GetBundleFullPath(string name)
        {
            return System.IO.Path.Combine(Dir, name);
        }

        public Stream LoadBundleFile(string name)
        {
            return null;
        }

        protected override void OnRelease()
        {            
        }
    }

    public FH.ResMgrConfig Config;
    // Start is called before the first frame update
    void Awake()
    {
        BundleMgr.InitMgr(new BundleLoader(), BundleMgrManifest.LoadFromFile("BundleCache/manifest.json"));

        UnityEngine.Application.targetFrameRate = 30;
        //FH.ResMgr.InitMgr(new FH.Res.SampleAssetLoader.AssetLoader_Resource(), Config);
        FH.ResMgr.InitMgr(new FH.AssetLoader_Bundle(BundleMgr.Inst), Config);
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
