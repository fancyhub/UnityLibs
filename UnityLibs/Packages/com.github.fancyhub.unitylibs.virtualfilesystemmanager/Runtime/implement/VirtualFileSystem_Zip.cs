/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/26
 * Title   : 
 * Desc    : 
*************************************************************************************/
using FH.VFSManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace FH
{
    public class VirtualFileSystem_Zip : IVirtualFileSystem
    {
        private const int CMaxStackBuffSize = 4096;

        public string _Name;
        public ZipArchive _ZipArchive;
        public VirtualFileSystem_Zip(string name, ZipArchive zipArchive)
        {
            _Name = name;
            _ZipArchive = zipArchive;
        }

        public static VirtualFileSystem_Zip CreateFromFile(string name, string file_path)
        {
            ZipArchive zipArchive = ZipFile.OpenRead(file_path);
            if (zipArchive == null)
            {
                VfsLog._.E("加载失败 {0}:{1}", name, file_path);
                return null;
            }

            return new VirtualFileSystem_Zip(name, zipArchive);
        }

        public string Name => _Name;

        public void Destroy()
        {
            _ZipArchive.Dispose();
        }

        public bool Exist(string file_path)
        {
            return _ZipArchive.GetEntry(file_path) != null;
        }

        public Stream OpenRead(string file_path)
        {
            ZipArchiveEntry entry = _ZipArchive.GetEntry(file_path);
            if (entry == null)
            {
                VfsLog._.D("{0}: Can't find {1}", _Name, file_path);
                return null;
            }

            return entry.Open();

        }

        public byte[] ReadAllBytes(string file_path)
        {
            ZipArchiveEntry entry = _ZipArchive.GetEntry(file_path);
            if (entry == null)
            {
                VfsLog._.D("{0}: Can't find {1}", _Name, file_path);
                return null;
            }

            byte[] ret = new byte[entry.Length];
            var stream = entry.Open();
            int count = stream.Read(ret, 0, ret.Length);
            stream.Close();

            if (count != entry.Length)
            {
                VfsLog._.E("{0} 读取文件出错,长度不一致,{1}", _Name, file_path);
                return null;
            }
            return ret;
        }

        public string ReadAllText(string file_path)
        {
            ZipArchiveEntry entry = _ZipArchive.GetEntry(file_path);
            if (entry == null)
            {
                VfsLog._.D("{0}: Can't find {1}", _Name, file_path);
                return null;
            }

            if (entry.Length <= CMaxStackBuffSize)
            {
                Span<byte> buff = stackalloc byte[(int)entry.Length];

                Stream stream = entry.Open();
                int count = stream.Read(buff);
                stream.Close();

                if (count != entry.Length)
                {
                    VfsLog._.E("{0} 读取文件出错,长度不一致,{1}", _Name, file_path);
                    return null;
                }

                return System.Text.Encoding.UTF8.GetString(buff);
            }
            else
            {
                byte[] buff = new byte[entry.Length];
                Stream stream = entry.Open();
                int count = stream.Read(buff, 0, buff.Length);
                stream.Close();


                if (count != entry.Length)
                {
                    VfsLog._.E("{0} 读取文件出错,长度不一致,{1}", _Name, file_path);
                    return null;
                }
                return System.Text.Encoding.UTF8.GetString(buff);
            }
        }
    }
}
