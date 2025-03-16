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
            Dictionary<string, BuildFileInfo> all_file = new Dictionary<string, BuildFileInfo>();

            bool hasError = false;
            foreach (var p in config.GetBuildSteps())
            {
                var file_list = p.Build(context);
                if (file_list != null)
                {
                    foreach (var p2 in file_list)
                    {
                        if (all_file.ContainsKey(p2.FileName))
                        {
                            Debug.LogError($"文件名 {p2.FileName} 重复");
                            hasError = true;
                        }
                        all_file[p2.FileName] = p2;
                    }
                }
            }
            if (hasError)
            {
                Debug.LogError("有错误, 停止处理");
                return;
            }

            //2. 复制到Cdn
            string cdn_dir = config.GetOutputDir(context.BuildTarget);
            FileManifest file_manifest = _CopyFiles(all_file, cdn_dir, config.GenGZ, config.DefaultExt);
            file_manifest.Version = VersionInfo.EdCreateResVersionInfo().ToResVersion();


            //3. 保存 File Manifest            
            string file_manifest_path = System.IO.Path.Combine(cdn_dir, FileManifest.CDefaultFileName);
            file_manifest_path = _FileNameAppendHash(file_manifest_path, file_manifest.Version);
            file_manifest.SaveTo(file_manifest_path);


            //4. save last version
            string last_version_path = System.IO.Path.Combine(cdn_dir, "last_version.txt");
            System.IO.File.WriteAllText(last_version_path, file_manifest.Version);


            //5. 复制到StreamingAssets
            List<FileManifest.FileItem> file_need_copy = config.GetFilesNeedToCopy2StreamingAssets(file_manifest);

            _SyncStreamingAssetsFolder(cdn_dir, file_need_copy);


            //6 复制FileManifest
            {
                string dst_file = FileSetting.StreamingAssetsDir + FileManifest.CDefaultFileName;
                System.IO.File.Copy(file_manifest_path, dst_file, true);
            }
        }

        private static FileManifest _CopyFiles(Dictionary<string, BuildFileInfo> src_files, string dst_dir, bool gen_gz_file, string default_ext)
        {
            FileManifest ret = new FileManifest();

            FileUtil.CreateDir(dst_dir);

            HashSet<string> all_tags_set = new HashSet<string>();
            List<BuildFileInfo> file_list = new List<BuildFileInfo>(src_files.Count);

            //1. 复制, 并且生成 tags, 文件映射关系
            foreach (var p in src_files)
            {
                //1.1 文件列表
                file_list.Add(p.Value);

                //1.2 获取所有的tags
                foreach (var p2 in p.Value.Tags)
                    all_tags_set.Add(p2);

                //1.3 复制
                string file_name = p.Key;
                string dest_file_name = _FileNameAppendHash(file_name, p.Value.FileHash, default_ext);
                string dest_file = System.IO.Path.Combine(dst_dir, dest_file_name);
                _CopySingleFile(p.Value.FilePath, dest_file, gen_gz_file);

                //1.4 添加
                var item = new FileManifest.FileItem()
                {
                    Name = file_name,
                    FullName = dest_file_name,
                    RelativePath = p.Value.FileRelativePath,
                    Size = _GetFileSize(dest_file),
                    UseGz = false,
                };

                string gz_path = dest_file + ".gz";
                if (gen_gz_file)
                {
                    int size = _GetFileSize(gz_path);
                    if (size < item.Size)
                    {
                        item.UseGz = true;
                        item.Size = size;
                    }
                }

                if (item.UseGz)
                    item.Crc32 = Crc32Helper.ComputeFile(gz_path);
                else
                    item.Crc32 = Crc32Helper.ComputeFile(dest_file);

                ret.Files.Add(item);
            }

            //2. tag
            foreach (var p in all_tags_set)
            {
                List<int> file_index_list = new List<int>();
                for (int i = 0; i < file_list.Count; i++)
                {
                    if (file_list[i].Tags.Contains(p))
                    {
                        file_index_list.Add(i);
                    }
                }

                ret.Tags.Add(new FileManifest.TagItem()
                {
                    Name = p,
                    Files = file_index_list.ToArray(),
                });
            }

            return ret;
        }

        private static void _SyncStreamingAssetsFolder(string cdn_dir, List<FileManifest.FileItem> file_names)
        {
            //1. 建立文件dict 
            Dictionary<string, FileManifest.FileItem> files_needed = new Dictionary<string, FileManifest.FileItem>(file_names.Count);
            foreach (var p in file_names)
            {
                if (string.IsNullOrEmpty(p.RelativePath))
                    files_needed[p.FullName] = p;
                else
                    files_needed[p.RelativePath] = p;
            }

            //2 删除不需要的文件
            string[] file_list = Array.Empty<string>();
            if(System.IO.Directory.Exists(FileSetting.StreamingAssetsDir))
                file_list= System.IO.Directory.GetFiles(FileSetting.StreamingAssetsDir, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var p in file_list)
            {
                if (p.EndsWith(".meta"))
                    continue;

                string file_name = System.IO.Path.GetFileName( p);
                if (files_needed.ContainsKey(file_name))
                    continue;
                System.IO.File.Delete(p);
                _DeleteFile(p + ".meta");
            }

            file_list = Array.Empty<string>();
            if (System.IO.Directory.Exists(FileSetting.StreamingAssetsRelativeFileDir))
                file_list = System.IO.Directory.GetFiles(FileSetting.StreamingAssetsRelativeFileDir, "*.*", SearchOption.AllDirectories);
            foreach (var full_path in file_list)
            {
                if (full_path.EndsWith(".meta"))
                    continue;

                string file_name = full_path.Substring(FileSetting.StreamingAssetsRelativeFileDir.Length);
                file_name = file_name.Replace("\\", "/");
                if (files_needed.ContainsKey(file_name))
                    continue;
                System.IO.File.Delete(full_path);
                _DeleteFile(full_path + ".meta");
            }

            //3 复制新文件
            foreach (var p in files_needed)
            {
                string src_file = System.IO.Path.Combine(cdn_dir, p.Value.FullName);
                string dst_file = FileSetting.StreamingAssetsDir + p.Value.FullName;
                if(!string.IsNullOrEmpty(p.Value.RelativePath))
                {
                    dst_file = FileSetting.StreamingAssetsRelativeFileDir + p.Value.RelativePath;
                }
                if (!System.IO.File.Exists(dst_file))
                {
                    FileUtil.CreateFileDir(dst_file);
                    System.IO.File.Copy(src_file, dst_file);
                }
            }
        }

        private static int _GetFileSize(string file_path)
        {
            System.IO.FileInfo info = new System.IO.FileInfo(file_path);
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

        private static void _CopySingleFile(string src_file, string dest_file, bool gen_gz_file)
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
                    var file_info_dest = new System.IO.FileInfo(dest_file);
                    var file_info_src = new System.IO.FileInfo(src_file);
                    if (file_info_dest.Length != file_info_src.Length)
                        need_copy = true;
                }
            }

            if (!need_copy)
            {
                return;
            }

            if (gen_gz_file)
            {
                string gz_file_dest = dest_file + ".gz";
                _DeleteFile(gz_file_dest);

                using FileStream fs_out = new FileStream(gz_file_dest, FileMode.OpenOrCreate, FileAccess.Write);
                using System.IO.Compression.GZipStream gz_stream = new System.IO.Compression.GZipStream(fs_out, System.IO.Compression.CompressionMode.Compress);
                using FileStream fs_in = File.OpenRead(src_file);
                fs_in.CopyTo(gz_stream);
                fs_in.Close();
                gz_stream.Close();
                fs_out.Close();
            }

            _DeleteFile(dest_file);
            File.Copy(src_file, dest_file, true);
            Debug.Log($"CopyFile {src_file} -> {dest_file}");
        }

        private static void _DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
