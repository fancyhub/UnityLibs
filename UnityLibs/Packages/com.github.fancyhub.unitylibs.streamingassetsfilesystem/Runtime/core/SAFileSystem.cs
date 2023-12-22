/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/9 14:08:21
 * Title   : 
 * Desc    : 解决 Android StreamingAssets 的读取, 不用Unity的方法
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using FH.StreamingAssetsFileSystem;

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
        public List<string> GetAllFileList();
    }

    public static class SAFileSystem
    {
#if UNITY_EDITOR
        private static string _EditorObbPath;
#endif        
        [Conditional("UNITY_EDITOR")]
        public static void EdSetObbPath(string obb_path)
        {
#if UNITY_EDITOR
            _EditorObbPath = obb_path;
#endif
    }

    private static ISAFileSystem _;

        public static ISAFileSystem Inst
        {
            get
            {


                if (_ == null)
                {
#if UNITY_ANDROID && !UNITY_EDITOR

                    if (UnityEngine.Application.dataPath.EndsWith("base.apk"))
                    {
                        _ = new SAFileSystem_Apk();
                    }
                    else
                    {
                        _ = new SAFileSystem_Obb(UnityEngine.Application.dataPath);
                    }

#elif UNITY_EDITOR
                    if(_EditorObbPath!=null)
                        _ = new SAFileSystem_Obb(_EditorObbPath);
                    else
                        _ = new SAFileSystem_Normal();
#else 
                    _ = new SAFileSystem_Normal();
#endif
                }

                return _;
            }
        }

        public static void GetAllFileList(List<string> out_file_list)
        {
            var inst = Inst;
            if (inst == null)
                return;
            if (out_file_list == null)
                return;
            out_file_list.AddRange(inst.GetAllFileList());
        }

        /// <summary>
        /// 不包括子文件夹
        /// </summary>        
        public static void GetFileList(string dir, bool include_sub_folder, List<string> out_file_list)
        {
            var inst = Inst;
            if (inst == null)
                return;
            if (out_file_list == null || dir == null)
                return;

            var file_list = inst.GetAllFileList();
            if (dir == string.Empty)
            {
                if (include_sub_folder)
                    out_file_list.AddRange(file_list);
                else
                {
                    foreach (var p in file_list)
                    {
                        if (p.IndexOf('/') < 0)
                            out_file_list.Add(p);
                    }
                }
            }
            else
            {
                if (!dir.EndsWith("/"))
                    dir = dir + "/";

                if (include_sub_folder)
                {
                    foreach (var p in file_list)
                    {
                        if (!p.StartsWith(dir))
                            continue;
                        out_file_list.Add(p);
                    }
                }
                else
                {
                    foreach (var p in file_list)
                    {
                        if (!p.StartsWith(dir))
                            continue;

                        if (p.IndexOf('/', dir.Length) < 0)
                            out_file_list.Add(p);
                    }
                }
            }
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
