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
        public string Version;

        [Serializable]
        public class FileItem
        {
            public string Name;
            public string FullName;
            public int Size;
            public bool UseGz;
            public int GzSize;
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
        public FileItem FindFile(string name)
        {
            if (_FileDict == null)
            {
                _FileDict = new Dictionary<string, FileItem>(Files.Count);

                foreach (var p in Files)
                {
                    _FileDict.Add(p.Name, p);
                }
            }
            if (name == null)
                return null;

            _FileDict.TryGetValue(name, out FileItem item);
            return item;
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
    }
}