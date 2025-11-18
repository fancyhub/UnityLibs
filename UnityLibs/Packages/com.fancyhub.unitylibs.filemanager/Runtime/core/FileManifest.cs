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
        private static Dictionary<int, int> _SharedSet = new();
        public List<FileItem> GetFilesWithTag(string tag)
        {
            _SharedFileList.Clear();
            GetFilesWithTag(tag, _SharedFileList);
            return _SharedFileList;
        }

        public enum ETagRelation
        {
            Or, //该文件只需要包含指定tags里面的任意一个就行
            And, //该文件要包含所有指定的tags
            InverseOr, //该文件不能包含指定tags里面的任意一个
            InverseAnd, //该文件不能包含所有指定的tags
        }

        public List<FileItem> GetFilesWithTags(HashSet<string> tags, ETagRelation tag_relation = ETagRelation.Or)
        {
            _SharedFileList.Clear();
            GetFilesWithTags(tags, tag_relation, _SharedFileList);
            return _SharedFileList;
        }

        public void GetFilesWithTags(HashSet<string> tags, ETagRelation tag_relation, List<FileItem> out_file_list)
        {
            if (tags == null)
                return;

            _SharedSet.Clear();
            int total_tags_count = 0;
            switch (tag_relation)
            {
                default:
                    break;
                case ETagRelation.Or:
                case ETagRelation.InverseOr:
                    total_tags_count = 1;
                    foreach (var tag in tags)
                    {
                        TagItem item = FindTag(tag);
                        if (item == null)
                            continue;
                        foreach (var p in item.Files)
                            _SharedSet[p] = 1;
                    }
                    break;

                case ETagRelation.And:
                case ETagRelation.InverseAnd:
                    total_tags_count = tags.Count;
                    foreach (var tag in tags)
                    {
                        TagItem item = FindTag(tag);
                        if (item == null || item.Files.Length == 0)
                        {
                            _SharedSet.Clear();
                            break;
                        }

                        foreach (var p in item.Files)
                        {
                            if (_SharedSet.TryGetValue(p, out var t))
                                _SharedSet[p] = t + 1;
                            else
                                _SharedSet.Add(p, 1);
                        }
                    }
                    break;
            }

            //反向选择
            if (tag_relation == ETagRelation.InverseAnd || tag_relation == ETagRelation.InverseOr)
            {
                for (int i = 0; i < Files.Count; i++)
                {
                    if (_SharedSet.TryGetValue(i, out var t) && t == total_tags_count)
                    {
                        _SharedSet.Remove(i);
                    }
                    else
                    {
                        _SharedSet[i] = total_tags_count;
                    }
                }
            }


            foreach (var p in _SharedSet)
            {
                if (p.Value == total_tags_count)
                    out_file_list.Add(Files[p.Key]);
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