using System;
using System.Collections.Generic;

namespace FH
{
    public class AssetBundleLoader : IAssetLoader
    {
        public int PtrVer => throw new NotImplementedException();

        public string AtlasTag2Path(string atlasName)
        {
            throw new NotImplementedException();
        }

        public void Destroy()
        {
            throw new NotImplementedException();
        }

        public EAssetStatus GetAssetStatus(string path)
        {
            throw new NotImplementedException();
        }

        public IAssetRef Load(string path, bool sprite)
        {
            throw new NotImplementedException();
        }

        public IAssetRef LoadAsync(string path, bool sprite)
        {
            throw new NotImplementedException();
        }
    }


    public class Bundle 
    {
        public string Name;
        public string Path;
        public bool Downloaded;

        public Bundle[] Deps;
    }
}
