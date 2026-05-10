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
        SizeMismatch,//下载文件大小和远端大小不一致
        PartialNotSupported,//服务器不支持安全的断点续传
        UnsupportedContentEncoding,//不支持的HTTP传输压缩
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
            WebStatus = e.Status;
            Msg = e.Message;

            if (e.Response is HttpWebResponse response)
            {
                HttpStatusCode status_code = response.StatusCode;
                HttpStatusCode = status_code;
                response.Close();
                Error = status_code == HttpStatusCode.NotFound
                    ? EHttpDownloaderError.RemoteFileNotExist
                    : EHttpDownloaderError.HttpCode;
                return;
            }

            Error = EHttpDownloaderError.WebCode;
        }

        public void SetHttpCode(HttpStatusCode status_code)
        {
            Error = status_code == HttpStatusCode.NotFound
                ? EHttpDownloaderError.RemoteFileNotExist
                : EHttpDownloaderError.HttpCode;
            HttpStatusCode = status_code;
        }

        public void SetIOException(System.IO.IOException e)
        {
            Error = EHttpDownloaderError.IOError;
            Msg = e.Message;
        }

        public void SetIOException(string msg)
        {
            Error = EHttpDownloaderError.IOError;
            Msg = msg ?? string.Empty;
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

        public void SetSizeMismatch(long local_file_size, long remote_file_size)
        {
            Error = EHttpDownloaderError.SizeMismatch;
            Msg = $"local_size:{local_file_size}, remote_size:{remote_file_size}";
        }

        public void SetPartialNotSupported(string msg)
        {
            Error = EHttpDownloaderError.PartialNotSupported;
            Msg = msg ?? string.Empty;
        }

        public void SetUnsupportedContentEncoding(string content_encoding)
        {
            Error = EHttpDownloaderError.UnsupportedContentEncoding;
            Msg = content_encoding ?? string.Empty;
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
                case EHttpDownloaderError.RemoteFileNotExist:
                    HttpDownloaderLog._.E("{0},{1},{2},{3}", Error, HttpStatusCode, Msg, RemoteUrl);
                    break;

                case EHttpDownloaderError.WebCode:
                    HttpDownloaderLog._.E("{0},{1},{2},{3}", Error, WebStatus, Msg, RemoteUrl);
                    break;

                case EHttpDownloaderError.IOError:
                case EHttpDownloaderError.UserCancer:
                case EHttpDownloaderError.CRC:
                case EHttpDownloaderError.SizeMismatch:
                case EHttpDownloaderError.PartialNotSupported:
                case EHttpDownloaderError.UnsupportedContentEncoding:
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
