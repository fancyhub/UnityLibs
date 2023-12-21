/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/9 14:08:21
 * Title   : 
 * Desc    : 解决 Android StreamingAssets 的读取, 不用Unity的方法
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace FH
{
    /// <summary>
    /// 路径格式
    /// 比如 Assets/StreamingAssets/a/b.txt
    /// 读取的路径应该是 a/b.txt
    /// </summary>
    public interface ISAFileSystem : IDisposable
    {
        public Stream OpenRead(string file_path);
        public byte[] ReadAllBytes(string file_path);
        public bool GetFileList(string dir_path, List<string> out_list);
    }

    public static class SAFileSystem
    {
        private static ISAFileSystem _;

        public static ISAFileSystem Inst
        {
            get
            {


                if (_ == null)
                {
#if UNITY_ANDROID && !UNITY_EDITOR
                    _ = new SAFileSystem_Anroid();
#else
                    _ = new SAFileSystem_Normal();
#endif
                }

                return _;
            }
        }

        public static bool GetFileList(string dir_path, List<string> out_list)
        {
            var inst = Inst;
            if (inst == null)
                return false;
            return inst.GetFileList(dir_path, out_list);
        }

        public static byte[] ReadAllBytes(string file_path)
        {
            var inst = Inst;
            if (inst == null)
                return System.Array.Empty<byte>();
            return inst.ReadAllBytes(file_path);
        }        
    }
}
