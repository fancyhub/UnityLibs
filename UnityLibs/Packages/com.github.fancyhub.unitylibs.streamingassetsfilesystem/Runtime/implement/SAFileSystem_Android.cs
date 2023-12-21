/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/9 14:08:21
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Android;

namespace FH
{
#if UNITY_ANDROID
    // https://docs.unity3d.com/2019.3/Documentation/Manual/AndroidJavaSourcePlugins.html
    // https://docs.unity3d.com/2019.3/Documentation/Manual/WindowsPlayerCPlusPlusSourceCodePluginsForIL2CPP.html
    public sealed class SAFileSystem_Anroid : ISAFileSystem
    {
        public const string C_JAVA_CLASS = "com.github.fancyhub.nativeio.JNIContext";
        public const string C_JAVA_CLASS_INIT_FUNC = "Init";

        private AndroidJavaClass _JNIContext;
        public SAFileSystem_Anroid()
        {
        }



        //只是针对 StreamingAssets 里面的文件
        public Stream OpenRead(string file_path)
        {
            if (string.IsNullOrEmpty(file_path))
                return null;
            _InitAndroidJNIContext();
            IntPtr fhandle = AndroidNativeIO.native_io_file_open(file_path);
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

        public bool GetFileList(string dir_path, List<string> out_list)
        {
            if (string.IsNullOrEmpty(dir_path))
                return false;
            if (out_list == null)
                return false;

            _InitAndroidJNIContext();
            IntPtr fhandle = AndroidNativeIO.native_io_dir_open(dir_path);
            if (fhandle == IntPtr.Zero)
                return false;

            for (; ; )
            {
                IntPtr p_str = AndroidNativeIO.native_io_dir_next_file(fhandle);
                if (p_str == IntPtr.Zero)
                    break;
                string file_name = Marshal.PtrToStringAnsi(p_str);
                out_list.Add(file_name);
            }
            AndroidNativeIO.native_io_dir_close(fhandle);
            return true;
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
            public const string C_DLL_NAME = "fhnativeio";

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr native_io_file_open(string file_path);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern void native_io_file_close(IntPtr fhandle);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern long native_io_file_get_len(IntPtr fhandle);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern long native_io_file_seek(IntPtr fhandle, long offset, int whence);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern int native_io_file_read(IntPtr fhandle, IntPtr buff, int count);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr native_io_dir_open(string file_path);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr native_io_dir_next_file(IntPtr fhandle);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern void native_io_dir_close(IntPtr fhandle);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr native_io_malloc(int count);

            [DllImport(C_DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
            public static extern void native_io_free(IntPtr buff_handle);
        }

        internal class AndroidStreamingAssetStream : System.IO.Stream
        {
            public const int C_BUFF_SIZE = 1024;
            public IntPtr _fhandle;
            public IntPtr _buf_handle;
            public long _len;
            public long _pos;

            public AndroidStreamingAssetStream(System.IntPtr fhandle)
            {
                _fhandle = fhandle;
                _len = AndroidNativeIO.native_io_file_get_len(fhandle);
                _buf_handle = AndroidNativeIO.native_io_malloc(C_BUFF_SIZE);
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
                    AndroidNativeIO.native_io_file_close(_fhandle);
                    _fhandle = IntPtr.Zero;
                }

                if (_buf_handle != IntPtr.Zero)
                {
                    AndroidNativeIO.native_io_free(_buf_handle);
                    _buf_handle = IntPtr.Zero;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                if (!disposing)
                {
                    if (_fhandle != IntPtr.Zero)
                    {
                        AndroidNativeIO.native_io_file_close(_fhandle);
                        _fhandle = IntPtr.Zero;
                    }

                    if (_buf_handle != IntPtr.Zero)
                    {
                        AndroidNativeIO.native_io_free(_buf_handle);
                        _buf_handle = IntPtr.Zero;
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
                int ret_count = 0;
                for (; count > 0;)
                {
                    int count_2_read = System.Math.Min(count, C_BUFF_SIZE);
                    int readed_count = AndroidNativeIO.native_io_file_read(_fhandle, _buf_handle, count_2_read);
                    if (readed_count == 0)
                        break;

                    Marshal.Copy(_buf_handle, buffer, offset, readed_count);

                    _pos += readed_count;
                    offset += readed_count;
                    ret_count += readed_count;
                    count -= count_2_read;
                }
                return ret_count;
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
#endif
}
