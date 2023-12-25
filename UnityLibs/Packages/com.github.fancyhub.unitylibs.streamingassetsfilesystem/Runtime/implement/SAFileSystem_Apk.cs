/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/9 14:08:21
 * Title   : 
 * Desc    : 
*************************************************************************************/

#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Android;

namespace FH.StreamingAssetsFileSystem
{

    // https://docs.unity3d.com/2019.3/Documentation/Manual/AndroidJavaSourcePlugins.html
    // https://docs.unity3d.com/2019.3/Documentation/Manual/WindowsPlayerCPlusPlusSourceCodePluginsForIL2CPP.html
    internal sealed class SAFileSystem_Apk : ISAFileSystem
    {
        private static string[] _S_IgnoreFileList = new string[] { "/", "device_features/", "license/", "bin/Data/" };

        private const string C_JAVA_CLASS = "com.github.fancyhub.nativeio.JNIContext";
        private const string C_JAVA_CLASS_INIT_FUNC = "Init";
        private const string C_JAVA_Func_FetchAllFiles = "FetchAllFiles";

        private List<string> _FileList;
        private AndroidJavaClass _JNIContext;
        private string _StreamingAssetsDir;
        public SAFileSystem_Apk()
        {
            _StreamingAssetsDir = Application.streamingAssetsPath;
            if (!_StreamingAssetsDir.EndsWith("/"))
                _StreamingAssetsDir += "/";
        }

        //只是针对 StreamingAssets 里面的文件
        public Stream OpenRead(string file_path)
        {
            if (string.IsNullOrEmpty(file_path))
                return null;
            if (!file_path.StartsWith(_StreamingAssetsDir))
                return null;

            _InitAndroidJNIContext();
            string file_relative_path = file_path.Substring(_StreamingAssetsDir.Length);
            IntPtr fhandle = AndroidNativeIO.fh_native_io_file_open(file_relative_path, (int)AndroidNativeIO.EAssetOpenMode.AASSET_MODE_STREAMING);
            if (fhandle == IntPtr.Zero)
                return null;
            return new AndroidStreamingAssetStream(fhandle);
        }

        public byte[] ReadAllBytes(string file_path)
        {
            Stream stream = OpenRead(file_path);
            if (stream == null)
                return null;
            int len = (int)stream.Length;
            byte[] ret = new byte[len];
            int count = stream.Read(ret, 0, len);
            stream.Close();

            if (count != len)
            {
                UnityEngine.Debug.AssertFormat(count == len, "读取长度有问题, {0}, 只读取了 {1}, 获取的长度是 {2}", file_path, count, len);
                return null;
            }
            return ret;
        }


        private const int C_FILE_NAME_BUFF_SIZE = 1024;
        public unsafe List<string> GetAllFileList()
        {
            if (_FileList != null)
                return _FileList;
            _InitAndroidJNIContext();
            if (_JNIContext == null)
                return null;
            _JNIContext.CallStatic(C_JAVA_Func_FetchAllFiles);

            int count = AndroidNativeIO.fh_native_io_get_file_count();
            _FileList = new List<string>(count);
            {
                byte* buff = stackalloc byte[C_FILE_NAME_BUFF_SIZE];
                for (int i = 0; i < count; i++)
                {
                    int name_len = AndroidNativeIO.fh_native_io_get_file(i, buff, C_FILE_NAME_BUFF_SIZE);
                    if (name_len < 0)
                        continue;
                    string file_name = System.Text.Encoding.UTF8.GetString(buff, name_len);

                    bool ignore = false;
                    foreach (var p in _S_IgnoreFileList)
                    {
                        if (file_name.StartsWith(p))
                        {
                            ignore = true;
                            break;
                        }
                    }

                    if (!ignore)
                        _FileList.Add(_StreamingAssetsDir + file_name);
                }
            }
            return _FileList;
        }

        private void _InitAndroidJNIContext()
        {
            if (_JNIContext != null)
                return;
            _JNIContext = new AndroidJavaClass(C_JAVA_CLASS);
            _JNIContext.CallStatic(C_JAVA_CLASS_INIT_FUNC);
        }

        public void Dispose()
        {

        }

        internal static class AndroidNativeIO
        {
            //https://developer.android.google.cn/ndk/reference/group/asset
            public enum EAssetOpenMode
            {
                AASSET_MODE_UNKNOWN = 0,
                AASSET_MODE_RANDOM = 1,
                AASSET_MODE_STREAMING = 2,
                AASSET_MODE_BUFFER = 3
            }


            public const string C_DLL_NAME = "fhnativeio";

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr fh_native_io_file_open(string file_path, int mode);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern void fh_native_io_file_close(IntPtr fhandle);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern long fh_native_io_file_get_len(IntPtr fhandle);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern long fh_native_io_file_seek(IntPtr fhandle, long offset, int whence);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int fh_native_io_file_read(IntPtr fhandle, byte[] buff, int offset, int count);

            /// <summary>
            /// Get File Count
            /// </summary>
            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int fh_native_io_get_file_count();

            /// <summary>
            /// Get File Name
            /// </summary>            
            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern unsafe int fh_native_io_get_file(int index, byte* buff, int buff_size);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr fh_native_io_malloc(int count);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern void fh_native_io_free(IntPtr buff_handle);
        }

        internal class AndroidStreamingAssetStream : System.IO.Stream
        {
            public IntPtr _fhandle;
            public long _len;
            public long _pos;

            public AndroidStreamingAssetStream(System.IntPtr fhandle)
            {
                _fhandle = fhandle;
                _len = AndroidNativeIO.fh_native_io_file_get_len(fhandle);
                _pos = 0;
            }

            ~AndroidStreamingAssetStream()
            {
                // 此处只需要释放非托管代码即可，因为GC调用时该对象资源可能还不需要释放
                Dispose(false);
            }

            public override void Close()
            {
                base.Close();
                if (_fhandle != IntPtr.Zero)
                {
                    AndroidNativeIO.fh_native_io_file_close(_fhandle);
                    _fhandle = IntPtr.Zero;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (!disposing)
                {
                    if (_fhandle != IntPtr.Zero)
                    {
                        AndroidNativeIO.fh_native_io_file_close(_fhandle);
                        _fhandle = IntPtr.Zero;
                    }
                }
            }

            public override bool CanRead { get { return _fhandle != IntPtr.Zero; } }

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => _len;

            public override long Position { get { return _pos; } set { } }

            public override void Flush()
            {

            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (buffer == null)
                    return 0;

                if (offset >= buffer.Length)
                    return 0;
                count = Math.Min(buffer.Length - offset, count);
                if (count <= 0)
                    return 0;

                int readed_count = AndroidNativeIO.fh_native_io_file_read(_fhandle, buffer, offset, count);
                _pos += readed_count;
                count -= readed_count;
                return readed_count;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _pos;
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }
        }
    }
}
#endif
