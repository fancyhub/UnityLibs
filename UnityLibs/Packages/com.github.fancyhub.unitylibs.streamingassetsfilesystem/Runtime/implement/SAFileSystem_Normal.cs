
/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/9 14:08:21
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;



namespace FH
{
    public sealed class SAFileSystem_Normal : ISAFileSystem
    {
        public void Dispose()
        {

        }
        public Stream OpenRead(string file_path)
        {
            file_path = System.IO.Path.Combine(Application.streamingAssetsPath, file_path);
            if (!File.Exists(file_path))
                return null;
            return File.OpenRead(file_path);
        }

        public byte[] ReadAllBytes(string file_path)
        {
            file_path = System.IO.Path.Combine(Application.streamingAssetsPath, file_path);
            if (!File.Exists(file_path))
                return null;
            return File.ReadAllBytes(file_path);
        }

        public bool GetFileList(string dir_path, List<string> out_list)
        {
            if (string.IsNullOrEmpty(dir_path))
                return false;
            if (out_list == null)
                return false;

            dir_path = Path.Combine(Application.streamingAssetsPath, dir_path);
            if (!System.IO.Directory.Exists(dir_path))
                return true;

            var files = Directory.GetFiles(dir_path);
            if (files == null)
                return false;
            foreach (var p in files)
            {
                out_list.Add(Path.GetFileName(p));
            }
            return true;
        }
    }
}
