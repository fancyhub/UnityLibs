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
    public class VirtualFileSystem_Dir : IVirtualFileSystem
    {
        public string _name;

        public List<(string root_dir, string sub_dir_name)> _Dirs = new List<(string root_dir, string sub_dir_name)>();

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
        public static VirtualFileSystem_Dir Create(string name, string root_dir, string sub_dir_name)
        {
            VirtualFileSystem_Dir ret = new VirtualFileSystem_Dir(name);

            ret.AddDir(root_dir, sub_dir_name);
            return ret;
        }

        public VirtualFileSystem_Dir(string name)
        {
            _name = name;
        }

        public void Remount()
        {
            //Do nothing
        }

        public bool AddDir(string root_dir, string sub_dir_name)
        {
            if (string.IsNullOrEmpty(root_dir))
            {
                VfsLog._.E("root_dir is null or empty");
                return false;
            }


            if (!Directory.Exists(root_dir))
            {
                VfsLog._.E("root_dir is not exist {0}", root_dir);
                return false;
            }

            string full_root_path = Path.GetFullPath(root_dir);
            full_root_path = full_root_path.Replace('\\', '/');
            if (!full_root_path.EndsWith("/"))
                full_root_path = full_root_path + "/";

            string sub_path = full_root_path;
            if (!string.IsNullOrEmpty(sub_dir_name))
            {
                sub_path = Path.Combine(full_root_path, sub_dir_name);
                sub_path = Path.GetFullPath(sub_path);
                sub_path = sub_path.Replace('\\', '/');
                if (!sub_path.EndsWith("/"))
                    sub_path = sub_path + "/";
            }

            //必须是子集
            if (sub_path.IndexOf(full_root_path) < 0)
            {
                VfsLog._.E("添加Dir 失败 {0},{1}, 子文件名必须是 root_dir的子目录", root_dir, sub_dir_name);
                return false;
            }

            if (!Directory.Exists(sub_path))
            {
                VfsLog._.E("添加Dir 失败 子目录{0}  不存在", sub_path);
                return false;
            }

            sub_dir_name = sub_path.Substring(full_root_path.Length);


            VfsLog._.D("Add Dir[{0},{1}] to {2} ", full_root_path, sub_dir_name, _name);
            _Dirs.Add((full_root_path, sub_dir_name));
            return true;
        }

        public void Destroy()
        {
            _Dirs.Clear();
        }

        public string Name => _name;

        public bool Exist(string file_path)
        {
            if (string.IsNullOrEmpty(file_path))
            {
                VfsLog._.E("file path is null or empty");
                return false;
            }

            foreach (var p in _Dirs)
            {
                if (!file_path.StartsWith(p.sub_dir_name))
                    continue;

                string full_file_path = p.root_dir + file_path;
                if (File.Exists(full_file_path))
                    return true;
            }
            return false;
        }

        public Stream OpenRead(string file_path)
        {
            if (string.IsNullOrEmpty(file_path))
            {
                VfsLog._.E("file path is null or empty");
                return null;
            }

            foreach (var p in _Dirs)
            {
                if (!file_path.StartsWith(p.sub_dir_name))
                    continue;

                string full_file_path = p.root_dir + file_path;
                if (File.Exists(full_file_path))
                    return File.Open(full_file_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                else
                {
                    VfsLog._.D("不存在 {0}->{1}", file_path, full_file_path);
                }
            }
            return null;
        }

        public string ReadAllText(string file_path)
        {
            if (string.IsNullOrEmpty(file_path))
            {
                VfsLog._.E("file path is null or empty");
                return null;
            }

            foreach (var p in _Dirs)
            {
                if (!file_path.StartsWith(p.sub_dir_name))
                    continue;

                string full_file_path = p.root_dir + file_path;
                if (File.Exists(full_file_path))
                {
                    return System.IO.File.ReadAllText(full_file_path);
                }
                else
                {
                    VfsLog._.D("不存在 {0}->{1}", file_path, full_file_path);
                }
            }
            return null;
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
