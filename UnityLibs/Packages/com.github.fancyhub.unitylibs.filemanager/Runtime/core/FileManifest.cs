/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    [Serializable]
    public sealed class FileManifest
    {
        public const string CDefaultFileName = "file_manifest.json";
        private const string CRemoteFileName = "file_manifest_{0}.json";
        public static string GetRemoteFileName(string version)
        {
            return string.Format(CRemoteFileName, version.Trim());
        }

        public static FileManifest ReadFromText(string content)
        {
            try
            {
                return UnityEngine.JsonUtility.FromJson<FileManifest>(content);
            }
            catch (Exception e)
            {
                Log.E(e);
                return null;
            }
        }

        public string Version;

        [Serializable]
        public class FileItem
        {
            public string Name;
            public string FullName;
            public string RelativePath;
            public int Size;
            public uint Crc32;

            /// <summary>
            /// 如果是true, size 是Gz文件的大小, Crc32 是Gz文件的Crc            
            /// </summary>
            public bool UseGz;
        }

        [Serializable]
        public class TagItem
        {
            public string Name;
            public int[] Files = System.Array.Empty<int>();
        }

        public List<FileItem> Files = new List<FileItem>();
        public List<TagItem> Tags = new List<TagItem>();

        private static List<FileItem> _SharedFileList = new List<FileItem>();
        private static HashSet<int> _SharedSet = new HashSet<int>();
        public List<FileItem> GetFilesWithTag(string tag)
        {
            _SharedFileList.Clear();
            GetFilesWithTag(tag, _SharedFileList);
            return _SharedFileList;
        }

        public List<FileItem> GetFilesWithTags(HashSet<string> tags)
        {
            _SharedFileList.Clear();
            GetFilesWithTags(tags, _SharedFileList);
            return _SharedFileList;
        }

        public void GetFilesWithTags(HashSet<string> tags, List<FileItem> out_file_list)
        {
            if (tags == null)
                return;

            _SharedSet.Clear();
            foreach (var tag in tags)
            {
                TagItem item = FindTag(tag);
                if (item == null)
                    continue;
                foreach (var p in item.Files)
                {
                    _SharedSet.Add(p);
                }
            }

            foreach (var p in _SharedSet)
            {
                out_file_list.Add(Files[p]);
            }
        }

        public void GetFilesWithTag(string tag, List<FileItem> out_file_list)
        {
            TagItem item = FindTag(tag);

            if (item == null)
                return;
            foreach (var p in item.Files)
            {
                out_file_list.Add(Files[p]);
            }
        }

        private Dictionary<string, FileItem> _FileDict;
        private Dictionary<string, FileItem> _RelativeFileDict;
        public FileItem FindFile(string name)
        {
            _BuildDict();
            if (name == null)
                return null;

            _FileDict.TryGetValue(name, out FileItem item);
            return item;
        }

        public void FindFiles(HashSet<string> file_names, List<FileItem> all_file_items)
        {
            _BuildDict();
            if (file_names == null || file_names.Count == 0)
                return;

            foreach (var p in file_names)
            {
                if (p == null)
                    continue;
                _FileDict.TryGetValue(p, out FileItem item);
                if (item != null)
                {
                    all_file_items.Add(item);
                }
            }
        }

        public FileItem FindFileByRelativePath(string relative_path)
        {
            _BuildDict();
            if (relative_path == null)
                return null;
            _RelativeFileDict.TryGetValue(relative_path, out FileItem item);
            return item;
        }

        private void _BuildDict()
        {
            if (_FileDict == null)
            {
                _FileDict = new Dictionary<string, FileItem>(Files.Count);

                foreach (var p in Files)
                {
                    _FileDict.Add(p.Name, p);
                }
            }

            if (_RelativeFileDict == null)
            {
                _RelativeFileDict = new();
                foreach (var p in Files)
                {
                    if (string.IsNullOrEmpty(p.RelativePath))
                        continue;
                    _RelativeFileDict.Add(p.RelativePath, p);
                }
            }
        }

        private Dictionary<string, TagItem> _TagDict;
        public TagItem FindTag(string tag)
        {
            if (_TagDict == null)
            {
                _TagDict = new Dictionary<string, TagItem>(Tags.Count);
                foreach (var p in Tags)
                    _TagDict.Add(p.Name, p);
            }

            if (tag == null)
                return null;

            _TagDict.TryGetValue(tag, out TagItem item);
            return item;
        }

        public void SaveTo(string file_path)
        {
            string content = UnityEngine.JsonUtility.ToJson(this, true);
            System.IO.File.WriteAllText(file_path, content);
        }
    }
}