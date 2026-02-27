/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

#if UNITY_EDITOR
using PlatformShare = FH.PlatformShare_Empty;
#elif UNITY_ANDROID
    using PlatformShare = FH.PlatformShare_Android;
#elif UNITY_IOS
    using PlatformShare = FH.PlatformShare_IOS;
#else
    using PlatformShare = FH.PlatformShare_Empty;
#endif

namespace FH
{
    public enum EShareCopyImageResult
    {
        OK,
        NoPermission,
        Unkown,
    }

    internal interface IPlatformShareUtil
    {        
        public void StartScreenshotListen(System.Action callBack);
        public EShareCopyImageResult CopyLocalImage2Gallery(string srcFillePath, string destFileName);
    }

    public static class ShareUtil
    {
        private static IPlatformShareUtil _Share;

        private static IPlatformShareUtil GetShare()
        {
            if (_Share == null)
            {
                _Share = new PlatformShare();
            }
            return _Share;
        }

        public static void StartScreenshotListen(System.Action callBack)
        {
            GetShare()?.StartScreenshotListen(callBack);
        }
        

        public static EShareCopyImageResult CopyLocalImage2Gallery(string srcFillePath, string destFileName)
        {
            var inst = GetShare();
            if (inst == null)
                return EShareCopyImageResult.Unkown;
            
            return inst.CopyLocalImage2Gallery(srcFillePath, destFileName);    
        }
    }
}