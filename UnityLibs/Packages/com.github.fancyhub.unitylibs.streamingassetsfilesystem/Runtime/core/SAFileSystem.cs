/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/9 14:08:21
 * Title   : 
 * Desc    : 解决 Android StreamingAssets 的读取, 不用Unity的方法
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using FH.StreamingAssetsFileSystem;
using UnityEngine;

namespace FH
{
    internal static class SAFileSystemDef
    {
        private static string _StreamingAssetsDir;
        public static string StreamingAssetsDir
        {
            get
            {
                if (_StreamingAssetsDir != null)
                    return _StreamingAssetsDir;

                _StreamingAssetsDir = Application.streamingAssetsPath;
                if (!_StreamingAssetsDir.EndsWith("/"))
                    _StreamingAssetsDir += "/";
                return _StreamingAssetsDir;
            }
        }

        public static bool CheckPath(string file_path)
        {
            if (string.IsNullOrEmpty(file_path))
            {
                UnityEngine.Debug.LogError($"SA can't read \"{file_path}\"");
                return false;
            }

            if (!file_path.StartsWith(StreamingAssetsDir))
            {
                UnityEngine.Debug.LogError($"SA can't read \"{file_path}\", it doesn't start with \"{StreamingAssetsDir}\"");
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 路径格式 都是 Application.StreamingAssetsPath 开始
    /// </summary>
    public interface ISAFileSystem : IDisposable
    {
        public Stream OpenRead(string file_path);
        public byte[] ReadAllBytes(string file_path);
        public List<string> GetAllFileList();
    }

    public static class SAFileSystem
    {

        private static string _EditorObbPath;
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void EdSetObbPath(string obb_path)
        {
#if UNITY_EDITOR
            _EditorObbPath = obb_path;
#endif
        }

        private static ISAFileSystem _;

        private static ISAFileSystem Inst
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

#else 
                    if (_EditorObbPath != null)
                        _ = new SAFileSystem_Obb(_EditorObbPath);
                    else
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
            {
                UnityEngine.Debug.LogError($"SA is not init");
                return;
            }
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
            {
                UnityEngine.Debug.LogError($"SA is not init");
                return;
            }
            if (!SAFileSystemDef.CheckPath(dir))
                return;

            if (out_file_list == null)
                return;

            var file_list = inst.GetAllFileList();

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

        public static byte[] ReadAllBytes(string file_path)
        {
            var inst = Inst;
            if (inst == null)
            {
                UnityEngine.Debug.LogError($"SA is not init");
                return System.Array.Empty<byte>();
            }

            if (!SAFileSystemDef.CheckPath(file_path))
                return null;

            return inst.ReadAllBytes(file_path);
        }

        public static Stream OpenRead(string file_path)
        {
            var inst = Inst;
            if (inst == null)
            {
                UnityEngine.Debug.LogError($"SA is not init");
                return null;
            }

            if (!SAFileSystemDef.CheckPath(file_path))
                return null;

            return inst.OpenRead(file_path);
        }
    }
}
