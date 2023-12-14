using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FH.AssetBundleManager.Builder
{
    public interface IAssetCollection
    {
        List<(string path, string address)> GetAllAssets();
    }

    public class AssetCollection : IAssetCollection
    {
        public string[] Dirs;
        public AssetCollection(params string[] dirs)
        {
            this.Dirs = dirs;
        }

        public List<(string path, string address)> GetAllAssets()
        {
            List<(string, string)> ret = new List<(string, string)>();

            foreach (var p in this.Dirs)
            {
                foreach (var p2 in System.IO.Directory.GetFiles(p, "*.*", System.IO.SearchOption.AllDirectories))
                {
                    if (p2.EndsWith(".meta"))
                        continue;

                    ret.Add((p2.Replace('\\', '/'), null));
                }
            }

            return ret;
        }
    }
}
