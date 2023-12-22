
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

namespace FH.StreamingAssetsFileSystem
{
    internal sealed class SAFileSystem_Normal : ISAFileSystem
    {
        private List<string> _FileList;

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

        public List<string> GetAllFileList()
        {
            if (_FileList == null)
            {
                _FileList = new List<string>();
                string dir = Application.streamingAssetsPath;
                string[] files = System.IO.Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories);

                foreach (var p in files)
                {
                    if (p.EndsWith(".meta"))
                        continue;

                    string path = p.Substring(dir.Length + 1);
                    path = path.Replace('\\', '/');
                    _FileList.Add(path);
                }
            }
            return _FileList;
        }
    }
}
