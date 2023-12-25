/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/15
 * Title   : 
 * Desc    : 
*************************************************************************************/
using FH.VFSManagement;
using System;
using System.Collections.Generic;
using System.IO;

namespace FH
{
    public class FileSystem_Dir: IVirtualFileSystem
    {
        public string _root_dir;
        public string _dir_prefix;
        public string _name;

        /// <summary>
        /// 如果 dir : a/b <para/>
        /// 那么读取的文件名： c.txt => a/b/c.txt <para/>
        /// 
        /// sub_dir: e <para/>
        /// 那么读取的文件名 必须是 e/c.txt 要不然读取不了 <para/>
        /// 路径就变成 e/c.txt => a/b/e/c.txt <para/>
        /// 
        /// sub_dir 没有，就默认 sub_dir 为./ <para/>
        /// </summary>
        public static FileSystem_Dir Create(string name, string dir, string sub_dir)
        {
            if (string.IsNullOrEmpty(dir))
                return null;

            if (!Directory.Exists(dir))
                return null;

            string full_root_path = Path.GetFullPath(dir);
            full_root_path = full_root_path.Replace('\\', '/');
            if (!full_root_path.EndsWith("/"))
                full_root_path = full_root_path + "/";


            string sub_path = full_root_path;
            if (!string.IsNullOrEmpty(sub_dir))
            {
                sub_dir = Path.Combine(full_root_path, sub_dir);
                sub_path = Path.GetFullPath(sub_dir);

                sub_path = sub_path.Replace('\\', '/');
                if (!sub_path.EndsWith("/"))
                    sub_path = sub_path + "/";
            }

            //必须是子集
            if (sub_path.IndexOf(full_root_path) < 0)
                return null;

            string dir_prefix = sub_path.Substring(full_root_path.Length);
            return new FileSystem_Dir(name, full_root_path, dir_prefix);
        }

        public FileSystem_Dir(string name, string root_dir, string dir_prefix)
        {
            _root_dir = root_dir;
            _dir_prefix = dir_prefix;
            _name = name;
        }

        public void Destroy()
        {
            _root_dir = null;
            _dir_prefix = null;
        }

        public bool Exist(string file_path)
        {
            if (!file_path.StartsWith(_dir_prefix))
                return false;

            file_path = _root_dir + file_path;
            return File.Exists(file_path);
        }

        public string Name => _name;

        public string GetRealFilePath(string file_path)
        {
            if (!file_path.StartsWith(_dir_prefix))
                return null;
            file_path = _root_dir + file_path;
            if (File.Exists(file_path))
                return file_path;
            return null;
        }

        public string EdGetRealFilePath(string file_path)
        {
            if (!file_path.StartsWith(_dir_prefix))
                return null;
            file_path = _root_dir + file_path;
            return file_path;
        }

        public Stream OpenRead(string file_path)
        {
            if (!file_path.StartsWith(_dir_prefix))
                return null;
            file_path = _root_dir + file_path;
            if (!File.Exists(file_path))
            {
                VfsLog._.E("找不到  {0}", file_path);
                return null;
            }
            return File.Open(file_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        }

        public byte[] ReadAllBytes(string file_path)
        {
            Stream stream = OpenRead(file_path);
            if (stream == null)
                return null;


            int len = (int)stream.Length;
            byte[] ret = new byte[len];

            stream.Read(ret, 0, len);
            stream.Close();
            return ret;
        }
    }
}
