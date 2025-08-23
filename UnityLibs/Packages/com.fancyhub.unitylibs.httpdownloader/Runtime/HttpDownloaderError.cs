/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13 15:48:38
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Net;

namespace FH
{
    public enum EHttpDownloaderError
    {
        OK,
        RemoteFileNotExist, //远程文件不存在
        HttpCode, //HTTP 的status 出错
        WebCode, //web的status 出错
        IOError, //写文件的时候出错
        UserCancer,//用户取消
        Unkown, //未知的错误     

        CRC,//校验CRC的时候有错误
    };

    public struct HttpDownloaderError
    {
        public EHttpDownloaderError Error;
        public HttpStatusCode HttpStatusCode;
        public WebExceptionStatus WebStatus;
        public string RemoteUrl;
        public string Msg;

        public HttpDownloaderError(string remote_url)
        {
            RemoteUrl = remote_url;
            Error = EHttpDownloaderError.OK;
            HttpStatusCode = HttpStatusCode.OK;
            WebStatus = WebExceptionStatus.Success;
            Msg = string.Empty;
        }

        public void SetCanceled()
        {
            Error = EHttpDownloaderError.UserCancer;
            Msg = string.Empty;
        }

        public void SetWebException(WebException e)
        {
            Error = EHttpDownloaderError.WebCode;
            WebStatus = e.Status;
            Msg = e.Message;
        }

        public void SetIOException(System.IO.IOException e)
        {
            Error = EHttpDownloaderError.IOError;
            Msg = e.Message;
        }

        public void SetUnkownException(Exception e)
        {
            Error = EHttpDownloaderError.Unkown;
            Msg = e.Message;
        }

        public void SetCrcError()
        {
            Error = EHttpDownloaderError.CRC;
            Msg = "Crc Error";
        }

        public void Reset()
        {
            Error = EHttpDownloaderError.OK;
            HttpStatusCode = HttpStatusCode.OK;
            WebStatus = WebExceptionStatus.Success;
            Msg = string.Empty;
        }

        public void PrintLog()
        {
            if (Error == EHttpDownloaderError.OK)
                return;

            switch (Error)
            {
                case EHttpDownloaderError.OK:
                    break;

                case EHttpDownloaderError.HttpCode:
                    HttpDownloaderLog._.E("{0},{1},{2},{3}", Error, HttpStatusCode, Msg, RemoteUrl);
                    break;

                case EHttpDownloaderError.WebCode:
                    HttpDownloaderLog._.E("{0},{1},{2},{3}", Error, WebStatus, Msg, RemoteUrl);
                    break;

                case EHttpDownloaderError.IOError:
                    HttpDownloaderLog._.E("{0},{1},{2}", Error, Msg, RemoteUrl);
                    break;

                case EHttpDownloaderError.Unkown:
                    HttpDownloaderLog._.E("{0},{1},{2}", Error, Msg, RemoteUrl);
                    break;

                default:
                    HttpDownloaderLog._.E("unkonw type " + Error);
                    break;
            }
        }
    }
}
