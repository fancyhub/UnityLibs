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
        public Lz4ZipFile _gzip_file;
        public string _name;         

        public VirtualFileSystem_Lz4Zip(string name, Lz4ZipFile gzip_file)
        {
            _name = name;
            _gzip_file = gzip_file;
        }

        public void Destroy()
        {
            _gzip_file.Close();
        }

        public bool Exist(string file_path)
        {
            return _gzip_file.FileExists(file_path);
        }

        public string Name => _name;        

        public Stream OpenRead(string file_path)
        {
            return _gzip_file.OpenRead(file_path);
        }

        public byte[] ReadAllBytes(string file_path)
        {
            return _gzip_file.ReadFileAllBytes(file_path);
        }
    }
}
