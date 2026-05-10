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
using System.Threading;

namespace FH
{
    public enum EHttpDownloaderPartialMode
    {
        Auto,
        Required,
        Disabled,
    }

    /// <summary>
    /// 本身是单线程的, 阻塞的, 需要外部套一下多线程
    /// </summary>
    public static class HttpDownloader
    {
        public delegate void OnFileSizeCallBack(long now_file_size, long total_file_size);

        private const int C_TIMEOUT_MS = 2000;//2000 毫秒，2秒

        public static RemoteCertificateValidationCallback ServerCertificateValidationCallback;
        public static byte[] RequestFileContent(string remote_uri)
        {
            HttpDownloaderError error = new HttpDownloaderError(remote_uri);
            byte[] bytes_ret = null;
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                request = _CreateRequest(remote_uri);
                response = request.GetResponse() as HttpWebResponse;
                if (response == null)
                {
                    error.SetIOException("response is null");
                    error.PrintLog();
                    return null;
                }

                HttpStatusCode status_code = response.StatusCode;
                if (status_code == HttpStatusCode.OK)
                {
                    using Stream http_stream = _GetResponseStream(response, error, out _);
                    if (http_stream == null)
                        return null;

                    using MemoryStream ms = new MemoryStream();
                    http_stream.CopyTo(ms);
                    bytes_ret = ms.ToArray();
                }
                else
                {
                    error.SetHttpCode(status_code);
                    error.PrintLog();
                }
            }
            catch (WebException e)
            {
                error.SetWebException(e);
                error.PrintLog();
            }
            catch (IOException e)
            {
                error.SetIOException(e);
                error.PrintLog();
            }
            catch (Exception e)
            {
                error.SetUnkownException(e);
                error.PrintLog();
            }
            finally
            {
                response?.Close();
                request?.Abort();
            }
            return bytes_ret;
        }

        /// <summary>
        /// 阻塞模式, 需要外面套
        /// </summary>
        public static long RequestFileLength(string remote_uri)
        {
            HttpDownloaderError err = new HttpDownloaderError(remote_uri);
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            long conent_len = -1;
            try
            {
                request = _CreateRequest(remote_uri);
                request.Method = "HEAD";

                response = request.GetResponse() as HttpWebResponse;
                if (response == null)
                {
                    err.SetIOException("response is null");
                }
                else if (response.StatusCode == HttpStatusCode.OK)
                {
                    conent_len = response.ContentLength;
                }
                else
                {
                    err.SetHttpCode(response.StatusCode);
                }
            }
            catch (WebException e)
            {
                err.SetWebException(e);
                err.PrintLog();
            }
            catch (Exception e)
            {
                err.SetUnkownException(e);
                err.PrintLog();
            }
            finally
            {
                response?.Close();
                request?.Abort();
            }
            return conent_len;
        }

        /// <summary>
        /// 阻塞模式, 需要外面套
        /// </summary>
        /// <param name="crc32">crc32 &gt;0, would check crc32</param>
        /// <returns></returns>
        public static HttpDownloaderError Download(
            string remote_uri,
            string local_file_path,
            OnFileSizeCallBack file_size_cb = null,
            uint crc32 = 0,
            CancellationToken cancel_token = default,
            EHttpDownloaderPartialMode partial_mode = EHttpDownloaderPartialMode.Auto)
        {
            long remote_file_size = RequestFileLength(remote_uri);
            HttpDownloaderError error = new HttpDownloaderError(remote_uri);

            for (; ; )
            {
                if (cancel_token.IsCancellationRequested)
                {
                    error.SetCanceled();
                    return error;
                }

                if (remote_file_size >= 0 && File.Exists(local_file_path))
                {
                    FileInfo existing_file = new FileInfo(local_file_path);
                    if (existing_file.Length == remote_file_size)
                    {
                        return _CheckDownloadedFile(remote_uri, local_file_path, crc32);
                    }

                    if (partial_mode != EHttpDownloaderPartialMode.Disabled && existing_file.Length > remote_file_size)
                    {
                        HttpDownloaderLog._.E("下载的size > remote_size,删除后重新下载 local_size:{0},remote_size:{1},{2}", existing_file.Length, remote_file_size, remote_uri);
                        File.Delete(local_file_path);
                    }
                }

                error = _DownloadPartial(
                    remote_uri,
                    local_file_path,
                    remote_file_size,
                    file_size_cb,
                    cancel_token,
                    partial_mode,
                    out bool retry_without_partial,
                    out bool response_content_decoded);

                if (retry_without_partial)
                {
                    File.Delete(local_file_path);
                    error = _DownloadPartial(
                        remote_uri,
                        local_file_path,
                        remote_file_size,
                        file_size_cb,
                        cancel_token,
                        EHttpDownloaderPartialMode.Disabled,
                        out _,
                        out response_content_decoded);
                }

                if (error.Error != EHttpDownloaderError.OK)
                    return error;

                if (!File.Exists(local_file_path))
                {
                    error.SetIOException("downloaded file does not exist");
                    error.PrintLog();
                    return error;
                }

                FileInfo fi = new FileInfo(local_file_path);
                if (remote_file_size >= 0 && !response_content_decoded)
                {
                    if (fi.Length < remote_file_size)
                    {
                        HttpDownloaderLog._.E("下载的size < remote_size ，要继续下载 local_size:{0},remote_size:{1},{2}", fi.Length, remote_file_size, remote_uri);
                        continue;
                    }
                    else if (fi.Length > remote_file_size)
                    {
                        error.SetSizeMismatch(fi.Length, remote_file_size);
                        error.PrintLog();
                        return error;
                    }
                }

                return _CheckDownloadedFile(remote_uri, local_file_path, crc32);
            }
        }


        private static HttpDownloaderError _CheckDownloadedFile(string remote_uri, string local_file_path, uint crc32)
        {
            HttpDownloaderError error = new HttpDownloaderError(remote_uri);
            if (crc32 == 0)
                return error;

            try
            {
                uint file_crc32 = Crc32Helper.ComputeFile(local_file_path);
                if (crc32 == file_crc32)
                    return error;

                File.Delete(local_file_path);
                error.SetCrcError();
                error.PrintLog();
            }
            catch (IOException e)
            {
                error.SetIOException(e);
                error.PrintLog();
            }
            catch (Exception e)
            {
                error.SetUnkownException(e);
                error.PrintLog();
            }
            return error;
        }


        /// <summary>
        /// partial true：断点续传
        /// </summary>
        private static unsafe HttpDownloaderError _DownloadPartial(
            string remote_uri,
            string local_file_path,
            long total_file_size,
            OnFileSizeCallBack file_size_cb,
            CancellationToken cancel_token,
            EHttpDownloaderPartialMode partial_mode,
            out bool retry_without_partial,
            out bool response_content_decoded)
        {
            HttpDownloaderError ret = new HttpDownloaderError(remote_uri);
            retry_without_partial = false;
            response_content_decoded = false;

            HttpWebRequest request = null;
            HttpWebResponse response = null;
            FileStream fs_out = null;

            try
            {
                FileUtil.CreateFileDir(local_file_path);

                fs_out = new FileStream(local_file_path, FileMode.OpenOrCreate, FileAccess.Write);
                long file_size = fs_out.Seek(0, SeekOrigin.End);
                bool resume_requested = partial_mode != EHttpDownloaderPartialMode.Disabled && file_size > 0;
                if (!resume_requested)
                {
                    fs_out.SetLength(0);
                    file_size = 0;
                }

                request = _CreateRequest(remote_uri);
                if (resume_requested)
                {
                    request.AddRange(file_size);
                    file_size_cb?.Invoke(file_size, total_file_size);
                }

                response = request.GetResponse() as HttpWebResponse;
                if (response == null)
                {
                    ret.SetIOException("response is null");
                    ret.PrintLog();
                    return ret;
                }

                HttpStatusCode status_code = response.StatusCode;
                if (status_code == HttpStatusCode.OK || status_code == HttpStatusCode.PartialContent)
                {
                    bool content_encoded = _HasContentEncoding(response);
                    response_content_decoded = content_encoded;

                    if (resume_requested && status_code == HttpStatusCode.PartialContent)
                    {
                        if (!_IsContentRangeStartValid(response, file_size))
                        {
                            if (partial_mode == EHttpDownloaderPartialMode.Required)
                            {
                                ret.SetPartialNotSupported($"invalid Content-Range:{response.GetResponseHeader("Content-Range")}");
                                ret.PrintLog();
                                return ret;
                            }

                            retry_without_partial = true;
                            return ret;
                        }

                        if (content_encoded)
                        {
                            if (partial_mode == EHttpDownloaderPartialMode.Required)
                            {
                                ret.SetPartialNotSupported($"PartialContent with Content-Encoding:{response.ContentEncoding}");
                                ret.PrintLog();
                                return ret;
                            }

                            retry_without_partial = true;
                            return ret;
                        }
                    }

                    if (resume_requested && status_code == HttpStatusCode.OK)
                    {
                        if (partial_mode == EHttpDownloaderPartialMode.Required)
                        {
                            ret.SetPartialNotSupported($"server returned {status_code}");
                            ret.PrintLog();
                            return ret;
                        }

                        HttpDownloaderLog._.W("服务器未返回PartialContent,重置本地临时文件后重新下载 {0}", remote_uri);
                        fs_out.SetLength(0);
                        file_size = 0;
                        file_size_cb?.Invoke(file_size, total_file_size);
                    }

                    Stream http_stream = _GetResponseStream(response, ret, out bool stream_content_decoded);
                    if (http_stream == null)
                        return ret;
                    response_content_decoded = stream_content_decoded;

                    Span<byte> buff = stackalloc byte[1024];
                    for (; ; )
                    {
                        if (cancel_token.IsCancellationRequested)
                        {
                            ret.SetCanceled();
                            break;
                        }

                        int read_size = http_stream.Read(buff);
                        if (read_size <= 0)
                            break;

                        fs_out.Write(buff.Slice(0, read_size));
                        file_size += read_size;
                        file_size_cb?.Invoke(file_size, response_content_decoded ? -1 : total_file_size);
                    }

                    http_stream.Close();
                    response.Close();
                    response = null;

                    fs_out.Close();
                    fs_out = null;

                    request.Abort();
                    request = null;
                }
                else
                {
                    ret.SetHttpCode(status_code);
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

                try
                {
                    request?.Abort();
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

        private static Stream _GetResponseStream(
            HttpWebResponse response,
            HttpDownloaderError error,
            out bool content_decoded)
        {
            content_decoded = false;

            Stream ret = response.GetResponseStream();
            if (ret == null)
            {
                error.SetIOException("response stream is null");
                error.PrintLog();
                return null;
            }

            string content_encoding = response.ContentEncoding;
            if (string.IsNullOrWhiteSpace(content_encoding))
                return ret;

            string[] encodings = content_encoding.Split(',');
            for (int i = encodings.Length - 1; i >= 0; i--)
            {
                string encoding = encodings[i].Trim();
                if (string.IsNullOrEmpty(encoding) || string.Equals(encoding, "identity", StringComparison.OrdinalIgnoreCase))
                    continue;

                content_decoded = true;
                if (string.Equals(encoding, "gzip", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(encoding, "x-gzip", StringComparison.OrdinalIgnoreCase))
                {
                    ret = new System.IO.Compression.GZipStream(ret, System.IO.Compression.CompressionMode.Decompress);
                    continue;
                }

                if (string.Equals(encoding, "deflate", StringComparison.OrdinalIgnoreCase))
                {
                    ret = new System.IO.Compression.DeflateStream(ret, System.IO.Compression.CompressionMode.Decompress);
                    continue;
                }

                ret.Close();
                error.SetUnsupportedContentEncoding(content_encoding);
                error.PrintLog();
                return null;
            }

            return ret;
        }

        private static bool _IsContentRangeStartValid(HttpWebResponse response, long expect_start)
        {
            string content_range = response.GetResponseHeader("Content-Range");
            if (string.IsNullOrWhiteSpace(content_range))
                return false;

            content_range = content_range.Trim();
            const string prefix = "bytes ";
            if (!content_range.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;

            int start_index = prefix.Length;
            int dash_index = content_range.IndexOf('-', start_index);
            if (dash_index < 0)
                return false;

            string start_text = content_range.Substring(start_index, dash_index - start_index).Trim();
            if (!long.TryParse(start_text, out long start))
                return false;

            return start == expect_start;
        }

        private static bool _HasContentEncoding(HttpWebResponse response)
        {
            string content_encoding = response.ContentEncoding;
            if (string.IsNullOrWhiteSpace(content_encoding))
                return false;

            string[] encodings = content_encoding.Split(',');
            for (int i = 0; i < encodings.Length; i++)
            {
                string encoding = encodings[i].Trim();
                if (string.IsNullOrEmpty(encoding) || string.Equals(encoding, "identity", StringComparison.OrdinalIgnoreCase))
                    continue;

                return true;
            }

            return false;
        }

        private static HttpWebRequest _CreateRequest(string remote_uri)
        {
            HttpWebRequest request = WebRequest.Create(remote_uri) as HttpWebRequest;
            if (request == null)
                throw new NotSupportedException($"invalid http uri: {remote_uri}");

            request.AllowAutoRedirect = true;
            request.KeepAlive = true;
            request.Timeout = C_TIMEOUT_MS;
            request.AutomaticDecompression = DecompressionMethods.None;
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            request.Proxy = WebRequest.GetSystemWebProxy();

            if (ServerCertificateValidationCallback != null)
                request.ServerCertificateValidationCallback = ServerCertificateValidationCallback;

            return request;
        }
    }
}
