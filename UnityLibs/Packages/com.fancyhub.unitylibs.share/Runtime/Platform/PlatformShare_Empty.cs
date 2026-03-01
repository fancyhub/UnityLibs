/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;


namespace FH
{   
    internal class PlatformShare_Empty: IPlatformShareUtil
    {
        public EShareCopyImageResult CopyLocalImage2Gallery(string srcFillePath, string destFileName)
        {
            return EShareCopyImageResult.Unkown;
        }

        public void Share(string title, string text, string imageFilePath)
        {
        }

        public void StartScreenshotListen(Action callBack)
        {       
        }

        public void StopScreenshotListen()
        {
            
        }
    }
}
