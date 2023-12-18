/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace FH
{
    public class BundleLoader : CPtrBase, IBundleLoader
    {
        public string Dir = "BundleCache/Win";
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

}
