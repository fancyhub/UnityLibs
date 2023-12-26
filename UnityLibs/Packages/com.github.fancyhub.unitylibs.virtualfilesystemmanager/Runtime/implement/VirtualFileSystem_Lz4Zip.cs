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
        public Lz4ZipFile _ZipFile;
        public string _name;

        public VirtualFileSystem_Lz4Zip(string name, Lz4ZipFile zip_file)
        {
            _name = name;
            _ZipFile = zip_file;
        }

        public static VirtualFileSystem_Lz4Zip CreateFromFile(string name, string path)
        {
            Lz4ZipFile zip_file = Lz4ZipFile.LoadFromFile(path);
            if (zip_file == null)
                return null;
            return new VirtualFileSystem_Lz4Zip(name, zip_file);
        }

        public void Destroy()
        {
            _ZipFile.Close();
        }

        public bool Exist(string file_path)
        {
            return _ZipFile.FileExists(file_path);
        }

        public string Name => _name;

        /// <summary>
        /// 这个比较特殊，用的时候，一定要读取完之后，直接关闭
        /// 不能同时打开两个
        /// </summary>
        public Stream OpenRead(string file_path)
        {
            return _ZipFile.OpenRead(file_path);
        }

        public byte[] ReadAllBytes(string file_path)
        {
            return _ZipFile.ReadFileAllBytes(file_path);
        }

        public string ReadAllText(string file_path)
        {
            return _ZipFile.ReadAllText(file_path);
        }
    }
}
