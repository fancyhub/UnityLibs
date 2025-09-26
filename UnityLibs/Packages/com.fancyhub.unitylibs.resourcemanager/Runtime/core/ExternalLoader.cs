/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/30
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;

namespace FH
{
    public partial interface IResMgr
    {
        public interface IExternalAssetRef : ICPtr
        {
            bool IsDone { get; }
            UnityEngine.Object Asset { get; }
            public string BundleName { get; }
        }

        /// <summary>
        /// 需要外部实现该接口
        /// </summary>
        public interface IExternalLoader : ICPtr
        {
            IExternalAssetRef LoadAsset(string path, Type unityAssetType);
            IExternalAssetRef LoadAssetAsync(string path, Type unityAssetType);

            string AtlasTag2Path(string atlasName);

            EAssetStatus GetAssetStatus(string path);
        }
    }
}
