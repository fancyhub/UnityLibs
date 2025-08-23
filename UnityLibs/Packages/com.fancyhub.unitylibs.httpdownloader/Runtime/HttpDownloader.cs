/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13 15:48:07
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Net;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace FH
{
    /// <summary>
    /// 本身是单线程的, 阻塞的, 需要外部套一下
    /// </summary>
    public static class HttpDownloader
    {
        public delegate void OnFileSizeCallBack(long now_file_size, long total_file_size);

        private const int C_TIMEOUT_MS = 2000;//2000 毫秒，2秒

        public static RemoteCertificateValidationCallback ServerCertificateValidationCallback;
        public static byte[] RequestFileContent(string remote_uri)
        {
            _Init();

            HttpDownloaderError _Error = new HttpDownloaderError(remote_uri);
            HttpWebRequest request = HttpWebRequest.Create(remote_uri) as HttpWebRequest;
            //request.ConnectionGroupName = C_HTTP_Group_Name;
            request.AllowAutoRedirect = true;
            request.KeepAlive = true;
            request.Timeout = C_TIMEOUT_MS;
            request.AutomaticDecompression = DecompressionMethods.None;
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            request.Proxy = WebRequest.GetSystemWebProxy();

            byte[] bytes_ret = null;
            HttpWebResponse response = null;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
                HttpStatusCode status_code = response.StatusCode;

                if (status_code == HttpStatusCode.OK)
                {
                    Stream http_stream = response.GetResponseStream();
                    MemoryStream ms = new MemoryStream();
                    http_stream.CopyTo(ms);

                    http_stream.Close();
                    response.Close();
                    request.Abort();

                    bytes_ret = ms.ToArray();
                }
                else
                {
                    _Error.Error = EHttpDownloaderError.HttpCode;
                    _Error.HttpStatusCode = status_code;
                    _Error.PrintLog();
                }

                response.Close();
                response = null;
            }
            catch (WebException e)
            {
                _Error.SetWebException(e);
                _Error.PrintLog();
            }
            catch (IOException e)
            {
                _Error.SetIOException(e);
                _Error.PrintLog();
            }
            catch (Exception e)
            {
                _Error.SetUnkownException(e);
                _Error.PrintLog();
            }
            return bytes_ret;
        }

        /// <summary>
        /// 阻塞模式, 需要外面套
        /// </summary>
        public static long RequestFileLength(string remote_uri)
        {
            _Init();

            HttpDownloaderError err = new HttpDownloaderError(remote_uri);

            HttpWebRequest request = HttpWebRequest.Create(remote_uri) as HttpWebRequest;

            //request.ConnectionGroupName = C_HTTP_Group_Name;
            request.Timeout = C_TIMEOUT_MS;
            request.AutomaticDecompression = DecompressionMethods.None;
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            request.KeepAlive = true;
            request.AllowAutoRedirect = true;
            request.Method = "HEAD";
            request.Proxy = WebRequest.GetSystemWebProxy();

            long conent_len = -1;

            try
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    conent_len = response.ContentLength;
                }
                else
                {
                    err.Error = EHttpDownloaderError.HttpCode;
                    err.HttpStatusCode = response.StatusCode;
                }
                response.Close();
                request.Abort();
            }
            catch (WebException e)
            {
                err.SetWebException(e);

            }
            catch (Exception e)
            {
                err.SetUnkownException(e);
            }
            return conent_len;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="crc32">crc32 &gt;0, would check crc32</param>
        /// <returns></returns>
        public static HttpDownloaderError Download(
            string remote_uri,
            string local_file_path,
            OnFileSizeCallBack file_size_cb = null,
            uint crc32 = 0,
            CancellationToken cancel_token = default)
        {
            long remote_file_size = RequestFileLength(remote_uri);
            _Init();
            for (; ; )
            {
                HttpDownloaderError error = _DownloadPartial(remote_uri, local_file_path, remote_file_size, file_size_cb, cancel_token);

                if (error.Error != EHttpDownloaderError.OK)
                    return error;

                if (!File.Exists(local_file_path))
                    continue;

                FileInfo fi = new FileInfo(local_file_path);
                if (fi.Length < remote_file_size)
                {
                    HttpDownloaderLog._.E("下载的size < remote_size ，要继续下载 local_size:{0},remote_size:{1},{2}", fi.Length, remote_file_size, remote_uri);
                    continue;
                }
                else if (fi.Length > remote_file_size)
                {
                    HttpDownloaderLog._.E("下载的size > remote_size,先返回 local_size:{0},remote_size:{1},{2}", fi.Length, remote_file_size, remote_uri);
                    return error;
                }


                //Check Crc32
                if (crc32 > 0)
                {
                    uint file_crc32 = Crc32Helper.ComputeFile(local_file_path);
                    if (crc32 == file_crc32)
                        return error;

                    //删除
                    File.Delete(local_file_path);
                    error.SetCrcError();
                }

                return error;
            }
        }


        /// <summary>
        /// partial true：断点续传
        /// </summary>        
        private static unsafe HttpDownloaderError _DownloadPartial(
            string remote_uri,
            string local_file_path,
            long total_file_size,
            OnFileSizeCallBack file_size_cb,
            CancellationToken cancel_token)
        {
            HttpDownloaderError ret = new HttpDownloaderError(remote_uri);

            HttpWebResponse response = null;
            FileStream fs_out = new FileStream(local_file_path, FileMode.OpenOrCreate, FileAccess.Write);
            long file_size = fs_out.Seek(0, SeekOrigin.End);

            try
            {
                HttpWebRequest request = HttpWebRequest.Create(remote_uri) as HttpWebRequest;
                //request.ConnectionGroupName = C_HTTP_Group_Name;
                request.AllowAutoRedirect = true;
                request.KeepAlive = true;
                request.Timeout = C_TIMEOUT_MS;
                request.AutomaticDecompression = DecompressionMethods.None;
                request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                request.Proxy = WebRequest.GetSystemWebProxy();

                if (file_size > 0)
                {
                    request.AddRange((int)file_size);
                    file_size_cb?.Invoke(file_size, total_file_size);
                }

                response = request.GetResponse() as HttpWebResponse;
                HttpStatusCode status_code = response.StatusCode;
                if (status_code == HttpStatusCode.OK || status_code == HttpStatusCode.PartialContent)
                {
                    string content_encoding = response.ContentEncoding.ToLower();
                    Stream http_stream = response.GetResponseStream();

                    Span<byte> buff = stackalloc byte[1024];

                    int read_size = http_stream.Read(buff);
                    while (read_size > 0)
                    {
                        fs_out.Write(buff.Slice(0, read_size));
                        read_size = http_stream.Read(buff);
                        file_size += read_size;
                        file_size_cb?.Invoke(file_size, total_file_size);

                        if (cancel_token.IsCancellationRequested)
                        {
                            ret.SetCanceled();
                            break;
                        }
                    }

                    response.Close();
                    response = null;

                    fs_out.Close();
                    fs_out = null;

                    request.Abort();
                    request = null;
                }
                else
                {
                    ret.Error = EHttpDownloaderError.HttpCode;
                    ret.HttpStatusCode = status_code;
                    ret.PrintLog();
                }
            }
            catch (WebException e)
            {
                ret.SetWebException(e);
                ret.PrintLog();
            }
            catch (IOException e)
            {
                ret.SetIOException(e);
                ret.PrintLog();
            }
            catch (Exception e)
            {
                ret.SetUnkownException(e);
                ret.PrintLog();
            }
            finally
            {
                try
                {
                    if (response != null)
                    {
                        response.Close();
                        response = null;
                    }
                }
                catch (Exception e2)
                {
                    HttpDownloaderLog._.E(e2.Message);
                }
            }

            if (fs_out != null)
            {
                try
                {
                    fs_out.Close();
                    fs_out = null;
                }
                catch (Exception e)
                {
                    HttpDownloaderLog._.E(e.Message);
                }
            }
            return ret;
        }

        private static void _Init()
        {
            if (ServerCertificateValidationCallback == null)
                ServerCertificateValidationCallback = _DefaultServerCertificateValidationCallback;

            if (ServicePointManager.ServerCertificateValidationCallback != ServerCertificateValidationCallback)
                ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
        }

        // 总是接受
        private static bool _DefaultServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; }
    }
}
