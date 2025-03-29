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
        public interface IExternalRef : ICPtr
        {
            bool IsDone { get; }
            UnityEngine.Object Asset { get; }
        }

        /// <summary>
        /// 需要外部实现该接口
        /// </summary>
        public interface IExternalLoader : ICPtr
        {
            IExternalRef Load(string path, Type unityAssetType);
            IExternalRef LoadAsync(string path, Type unityAssetType);

            string AtlasTag2Path(string atlasName);

            EAssetStatus GetAssetStatus(string path);
        }
    }
}
