/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

#if UNITY_ANDROID || UNITY_EDITOR

namespace FH
{
   
    internal class PlatformShare_Empty: IPlatformShareUtil
    {
        public EShareCopyImageResult CopyLocalImage2Gallery(string srcFillePath, string destFileName)
        {
            return EShareCopyImageResult.Unkown;
        }

        public void StartScreenshotListen(Action callBack)
        {       
        }
    }
}
#endif