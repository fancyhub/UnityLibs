/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/15
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

namespace FH
{
    public class VirtualFileSystem_Lz4Zip : IVirtualFileSystem
    {
        public Func<string, Lz4ZipFile> _loader_func;
        public Lz4ZipFile _zip_file;
        public string _name;

        public VirtualFileSystem_Lz4Zip(string name, Func<string, Lz4ZipFile> loader_func)
        {
            _name = name;
            _loader_func = loader_func;


            _zip_file = _loader_func(_name);
            if (_zip_file == null)
            {
                VFSManagement.VfsLog._.E("lz4zip file is null, {0}", _name);
            }
        }

        public void Remount()
        {
            _CloseZipFile();
            _zip_file = _loader_func(_name);
            if (_zip_file == null)
            {
                VFSManagement.VfsLog._.E("lz4zip file is null, {0}", _name);
            }
        }

        public void Destroy()
        {
            _CloseZipFile();
        }

   
        public bool Exist(string file_path)
        {
            if (_zip_file == null)
                return false;
            return _zip_file.FileExists(file_path);
        }

        public string Name => _name;

        /// <summary>
        /// 这个比较特殊，用的时候，一定要读取完之后，直接关闭
        /// 不能同时打开两个
        /// </summary>
        public Stream OpenRead(string file_path)
        {
            if (_zip_file == null)
                return null;
            return _zip_file.OpenRead(file_path);
        }

        public byte[] ReadAllBytes(string file_path)
        {
            if (_zip_file == null)
                return null;
            return _zip_file.ReadFileAllBytes(file_path);
        }

        public string ReadAllText(string file_path)
        {
            if (_zip_file == null)
                return null;
            return _zip_file.ReadAllText(file_path);
        }

        private void _CloseZipFile()
        {
            if (_zip_file != null)
            {
                var t = _zip_file;
                _zip_file = null;
                t.Close();
            }
        }

    }
}
