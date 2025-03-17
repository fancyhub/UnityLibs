/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH.ABManagement;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace FH
{
    public partial interface IBundleMgr
    {
        public enum EBundleFileStatus
        {
            None, //不存在
            Ready,
            Remote,
        }

        public interface IExternalLoader : ICPtr
        {
            /// <summary>
            /// 如果返回null, 使用 AssetBundle.LoadFromFile
            /// 如果返回正确的值, 使用 AssetBundle.LoadFromStream
            /// </summary>
            public Stream LoadBundleFile(string name);
            public string GetBundleFilePath(string name);
            public EBundleFileStatus GetBundleFileStatus(string name);

            public BundleManifest LoadManifest();
        }
    }


    public interface IBundle
    {
        public string Name { get; }

        public void GetAllDeps(List<IBundle> deps);

        public bool IsDownloaded();

        public int IncRefCount();
        public int DecRefCount();
        public int RefCount { get; }

        public T LoadAsset<T>(string path) where T : UnityEngine.Object;

        public AssetBundleRequest LoadAssetAsync<T>(string path) where T : UnityEngine.Object;
    }

    public partial interface IBundleMgr : ICPtr
    {
        public IBundle LoadBundleByAsset(string asset);

        public IBundle FindBundleByAsset(string asset);

        public IBundleMgr.EBundleFileStatus GetBundleStatus(IBundle bundle);

        public void GetAllBundles(List<IBundle> bundles);
    }

    public static class BundleMgr
    {
        private static CPtr<IBundleMgr> _;

        public static IBundleMgr Inst { get { return _.Val; } }

        public static bool InitMgr(IBundleMgr.Config config, IBundleMgr.IExternalLoader external_loader, bool disable_in_editor)
        {
            if (config == null)
            {
                BundleLog.E("BundleMgrConfig is Null");
                return false;
            }

            if (_.Val != null)
            {
                BundleLog.E("BundleMgr 已经创建了");
                return false;
            }
            
            BundleLog.SetMasks(config.LogLvl);

            if (disable_in_editor && Application.isEditor)
            {                
                _ = new ABManagement.BundleMgrImplementEmpty();
                return true;
            }

            if (external_loader == null)
            {
                BundleLog.E("BundlerLoader is Null");
                return false;
            }
            var manifest = external_loader.LoadManifest();
            if (manifest == null)
            {
                BundleLog.E("bundle manifest is Null");
                return false;
            }

            ABManagement.BundleMgrImplement mgr = new ABManagement.BundleMgrImplement();
            mgr.Init(external_loader, manifest);
            _ = mgr;
            return true;
        }

        public static void Destroy()
        {
            _.Destroy();
        }

        public static IBundle LoadBundleByAsset(string asset)
        {
            var mgr = _.Val;
            if (mgr == null)
            {
                BundleLog.E("BundleMgr is null");
                return null;
            }

            return mgr.LoadBundleByAsset(asset);
        }

        private static Dictionary<string, IBundle> _S_TempDict = new Dictionary<string, IBundle>();
        private static List<IBundle> _S_TempList = new List<IBundle>();
        private static List<IBundle> _S_TempList2 = new List<IBundle>();

        public static void GetAllBundles(List<string> asset_list, List<IBundle> out_bundle_list)
        {
            out_bundle_list.Clear();

            var mgr = _.Val;
            if (mgr == null)
            {
                BundleLog.E("BundleMgr is null");
                return;
            }

            _S_TempDict.Clear();
            foreach (var p in asset_list)
            {
                IBundle bundle = mgr.FindBundleByAsset(p);
                if (bundle == null)
                    continue;
                if (_S_TempDict.ContainsKey(bundle.Name))
                    continue;
                _S_TempDict.Add(bundle.Name, bundle);

                _S_TempList.Clear();
                bundle.GetAllDeps(_S_TempList);
                foreach (var p2 in _S_TempList)
                {
                    _S_TempDict[p2.Name] = bundle;
                }
            }
            foreach (var p in _S_TempDict)
            {
                out_bundle_list.Add(p.Value);
            }
        }

        public static void GetAllNeedDownload(List<string> asset_list, List<IBundle> out_bundle_list)
        {
            out_bundle_list.Clear();
            var mgr = _.Val;
            if (mgr == null)
            {
                BundleLog.E("BundleMgr is null");
                return;
            }

            GetAllBundles(asset_list, _S_TempList2);

            foreach (var p in _S_TempList2)
            {
                var status = mgr.GetBundleStatus(p);

                switch (status)
                {
                    default:
                    case IBundleMgr.EBundleFileStatus.None:
                        BundleLog.E("Bundle: {0}, Status: {1}, 有问题", p.Name, status);
                        break;

                    case IBundleMgr.EBundleFileStatus.Ready:
                        break;

                    case IBundleMgr.EBundleFileStatus.Remote:
                        out_bundle_list.Add(p);
                        break;
                }
            }
        }
    }
}
