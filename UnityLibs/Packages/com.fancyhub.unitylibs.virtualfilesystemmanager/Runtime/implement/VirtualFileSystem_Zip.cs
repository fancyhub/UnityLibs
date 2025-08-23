/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/26
 * Title   : 
 * Desc    : 
*************************************************************************************/
using FH.VFSManagement;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.IO.Compression;

namespace FH
{
    public class VirtualFileSystem_Zip : IVirtualFileSystem
    {
        private const int CMaxStackBuffSize = 4096;

        public Func<string, ZipArchive> _loader_func;
        public string _name;
        public ZipArchive _zip_archive;
        public VirtualFileSystem_Zip(string name, Func<string, ZipArchive> loader_func)
        {
            _name = name;
            _loader_func = loader_func;

            _zip_archive = loader_func(name);
            if (_zip_archive == null)
            {
                VFSManagement.VfsLog._.E("zip file is null, {0}", _name);
            }
        }

        public void Remount()
        {
            _CloseZipFile();
            _zip_archive = _loader_func(_name);
            if (_zip_archive == null)
            {
                VFSManagement.VfsLog._.E("zip file is null, {0}", _name);
            }
        }         

        public string Name => _name;

        public void Destroy()
        {
            _CloseZipFile();            
        }

        private void _CloseZipFile()
        {
            if (_zip_archive != null)
            {
                var t = _zip_archive;
                _zip_archive = null;
                t.Dispose();
            }
        }

        public bool Exist(string file_path)
        {
            if (_zip_archive == null)
                return false;
            return _zip_archive.GetEntry(file_path) != null;
        }

        public Stream OpenRead(string file_path)
        {
            if (_zip_archive == null)
                return null;
            ZipArchiveEntry entry = _zip_archive.GetEntry(file_path);
            if (entry == null)
            {
                VfsLog._.D("{0}: Can't find {1}", _name, file_path);
                return null;
            }

            return entry.Open();

        }

        public byte[] ReadAllBytes(string file_path)
        {
            if (_zip_archive == null)
                return null;

            ZipArchiveEntry entry = _zip_archive.GetEntry(file_path);
            if (entry == null)
            {
                VfsLog._.D("{0}: Can't find {1}", _name, file_path);
                return null;
            }

            byte[] ret = new byte[entry.Length];
            var stream = entry.Open();
            int count = stream.Read(ret, 0, ret.Length);
            stream.Close();

            if (count != entry.Length)
            {
                VfsLog._.E("{0} 读取文件出错,长度不一致,{1}", _name, file_path);
                return null;
            }
            return ret;
        }

        public string ReadAllText(string file_path)
        {
            if (_zip_archive == null)
                return null;

            ZipArchiveEntry entry = _zip_archive.GetEntry(file_path);
            if (entry == null)
            {
                VfsLog._.D("{0}: Can't find {1}", _name, file_path);
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
                    VfsLog._.E("{0} 读取文件出错,长度不一致,{1}", _name, file_path);
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
                    VfsLog._.E("{0} 读取文件出错,长度不一致,{1}", _name, file_path);
                    return null;
                }
                return System.Text.Encoding.UTF8.GetString(buff);
            }
        }
    }
}
