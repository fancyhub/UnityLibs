using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FH.FileManagement.Ed
{
    public static class Builder
    {
        [MenuItem("Tools/Files/Build")]
        public static void Build()
        {
            FileBuilderConfig config = FileBuilderConfig.GetDefault();
            Build(config, EditorUserBuildSettings.activeBuildTarget);
        }

        public static void Build(FileBuilderConfig config, UnityEditor.BuildTarget target)
        {
            //1. 步骤
            BuildContext context = new BuildContext();
            context.BuildTarget = target;

            foreach (var p in config.GetBuildSteps())
            {
                p.Build(context);
            }

            FileManifest file_manifest = new FileManifest();
            file_manifest.Version = VersionInfo.EdCreateResVersionInfo().ToResVersion();


            //2. 复制到Output
            var all_file_list = context.FileList;

            string dst_dir = System.IO.Path.Combine(config.OutputDir,context.Target2Name());
            FileUtil.CreateDir(dst_dir);

            HashSet<string> all_tags_set = new HashSet<string>();
            Dictionary<string, int> file_dict = new Dictionary<string, int>();
            foreach (var p in all_file_list)
            {
                foreach (var p2 in p.Tags)
                    all_tags_set.Add(p2);
                string file_name = System.IO.Path.GetFileName(p.FilePath);
                file_dict.Add(file_name, file_dict.Count);
            }

            foreach (var p in all_file_list)
            {
                string file_name = System.IO.Path.GetFileName(p.FilePath);
                string dest_file_name = _FileNameAppendHash(file_name, p.FileHash, config.DefaultExt);
                string dest_file = System.IO.Path.Combine(dst_dir, dest_file_name);
                _CopyFile(p.FilePath, dest_file, false);

                file_manifest.Files.Add(new FileManifest.FileItem()
                {
                    Name = file_name,
                    FullName = dest_file_name,
                    Size = _GetFileSize(dest_file),
                    UseGz = false,
                    GzSize = 0,
                });
            }

            foreach (var p in all_tags_set)
            {
                List<int> file_index_list = new List<int>();
                for (int i = 0; i < all_file_list.Count; i++)
                {
                    if (all_file_list[i].Tags.Contains(p))
                    {
                        file_index_list.Add(i);
                    }
                }

                file_manifest.Tags.Add(new FileManifest.TagItem()
                {
                    Name = p,
                    Files = file_index_list.ToArray(),
                });
            }

            //3. 保存 File Manifest
            string file_manifest_path = System.IO.Path.Combine(dst_dir, FileSetting.ManifestName);
            file_manifest_path = _FileNameAppendHash(file_manifest_path, file_manifest.Version);
            string content = UnityEngine.JsonUtility.ToJson(file_manifest, true);
            System.IO.File.WriteAllText(file_manifest_path, content);


            //4. save last version
            string last_version_path = System.IO.Path.Combine(dst_dir, "last_version.txt");
            System.IO.File.WriteAllText(last_version_path, file_manifest.Version);


            //5. 复制到StreamingAssets
            string streamingassets_dir = System.IO.Path.Combine("Assets/StreamingAssets", FileSetting.StreamingAssetsDir);
            HashSet<string> file_need_copy = new HashSet<string>();
            foreach (var p in file_manifest.Tags)
            {
                if (!config.TagsNeedCopy2StreamingAssets.Contains(p.Name))
                    continue;

                foreach (var p2 in p.Files)
                {
                    file_need_copy.Add(file_manifest.Files[p2].FullName);
                }
            }
            _SyncFoldFiles(dst_dir, streamingassets_dir, file_need_copy);


            //6 复制FileManifest
            {
                string dst_file = System.IO.Path.Combine(streamingassets_dir, FileSetting.ManifestName);
                System.IO.File.Copy(file_manifest_path, dst_file, true);
            }
        }

        private static void _SyncFoldFiles(string src_dir, string dest_dir, HashSet<string> file_names)
        {
            //1 删除不需要的文件

            FileUtil.CreateDir(dest_dir);
            string[] file_list = System.IO.Directory.GetFiles(dest_dir);
            foreach (var p in file_list)
            {
                if (p.EndsWith(".meta"))
                    continue;

                string file_name = System.IO.Path.GetFileName(p);
                if (file_names.Contains(file_name))
                    continue;
                System.IO.File.Delete(p);
                _delete_file(p + ".meta");
            }

            //5.3 复制新文件
            foreach (var p in file_names)
            {
                string src_file = System.IO.Path.Combine(src_dir, p);
                string dst_file = System.IO.Path.Combine(dest_dir, p);
                if (!System.IO.File.Exists(dst_file))
                    System.IO.File.Copy(src_file, dst_file);
            }
        }

        private static int _GetFileSize(string file_path)
        {
            System.IO.FileInfo info = new FileInfo(file_path);
            return (int)info.Length;
        }

        private static string _FileNameAppendHash(string file_name, string hash, string default_ext = null)
        {
            string ext = System.IO.Path.GetExtension(file_name);
            if (string.IsNullOrEmpty(ext))
            {
                if (default_ext != null)
                    return file_name + "_" + hash + default_ext;
                else
                    return file_name + "_" + hash;
            }

            file_name = file_name.Substring(0, file_name.Length - ext.Length);
            return file_name + "_" + hash + ext;
        }

        private static void _CopyFile(string src_file, string dest_file, bool gen_gz_file)
        {

            bool need_copy = false;
            //1. check 是否需要copy
            {
                //文件不存在，就复制
                if (!File.Exists(dest_file))
                {
                    need_copy = true;
                }
                else
                {
                    //可能是在上次的复制过程中，被打断
                    var file_info_dest = new FileInfo(dest_file);
                    var file_info_src = new FileInfo(src_file);
                    if (file_info_dest.Length != file_info_src.Length)
                        need_copy = true;
                }
            }

            if (!need_copy)
                return;


            if (gen_gz_file)
            {
                string gz_file_dest = dest_file + ".gz";
                _delete_file(gz_file_dest);

                using FileStream fs_out = new FileStream(gz_file_dest, FileMode.OpenOrCreate, FileAccess.Write);
                using System.IO.Compression.GZipStream gz_stream = new System.IO.Compression.GZipStream(fs_out, System.IO.Compression.CompressionMode.Compress);
                using FileStream fs_in = File.OpenRead(src_file);
                fs_in.CopyTo(gz_stream);
                fs_in.Close();
                gz_stream.Close();
                fs_out.Close();
            }

            _delete_file(dest_file);
            File.Copy(src_file, dest_file, true);
        }

        public static void _delete_file(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
