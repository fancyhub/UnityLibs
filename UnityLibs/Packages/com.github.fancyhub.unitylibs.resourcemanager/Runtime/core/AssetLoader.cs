/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

 
namespace FH
{
    public enum EAssetStatus
    {
        Exist,
        NotExist,
        NotDownloaded,
    }

    public interface IAssetRef : ICPtr
    {
        bool IsDone { get; }
        UnityEngine.Object Asset { get; }
    }

    /// <summary>
    /// 需要外部实现该接口
    /// </summary>
    public interface IAssetLoader : ICPtr
    {
        IAssetRef Load(string path, bool sprite);
        IAssetRef LoadAsync(string path, bool sprite);

        string AtlasTag2Path(string atlasName);

        EAssetStatus GetAssetStatus(string path);
    }
}
