/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using FH.AssetBundleBuilder.Ed;
using System.Collections.Generic;
using System.IO;

namespace FH.Res.Ed
{
    public class PostBuild_ProcessFileMgr : BuilderPostBuild
    {
        public string DefaultExt = ".bytes";
        public string DestFolder = "Bundle/Server";

        public override void OnPostBuild(PostBuildContext context)
        {
            //1. 生成文件
            var graph = context.AssetGraph;

            string src_dir = context.Config.GetOutputDir(context.Target);
            string dst_dir = System.IO.Path.Combine(DestFolder, context.Config.Target2Name(context.Target));
            FileUtil.CreateDir(dst_dir);

            FileManifest file_manifest = new FileManifest();
            file_manifest.Version = VersionInfo.EdCreateResVersionInfo().ToResVersion();
            foreach (var p in graph.Bundles)
            {
                string src_file = System.IO.Path.Combine(src_dir, p.Name);
                string hash = context.Manifest.GetAssetBundleHash(p.Name).ToString();

                string dest_file_name = _FileNameAppendHash(p.Name, hash, DefaultExt);
                string dest_file = System.IO.Path.Combine(dst_dir, dest_file_name);

                _CopyFile(src_file, dest_file, false);
                file_manifest.Files.Add(new FileManifest.FileItem()
                {
                    Name = p.Name,
                    FullName = dest_file_name,
                    Size = _GetFileSize(dest_file),
                    UseGz = false,
                    GzSize = 0,
                });
            }


            //2. Bundle Manifest
            {
                string src_file = System.IO.Path.Combine(src_dir, ResService.BunldeManifestName);

                string hash = MD5Helper.ComputeFile(src_file);
                string dst_file_name = _FileNameAppendHash(ResService.BunldeManifestName, hash, DefaultExt);
                string dst_file = System.IO.Path.Combine(dst_dir, dst_file_name);
                _CopyFile(src_file, dst_file, false);

                file_manifest.Files.Add(new FileManifest.FileItem()
                {
                    Name = ResService.BunldeManifestName,
                    FullName = dst_file_name,
                    Size = _GetFileSize(dst_file),
                    UseGz = false,
                    GzSize = 0,
                });
            }

            //3. Save File Manifest
            string file_manifest_path = System.IO.Path.Combine(dst_dir, FileSetting.ManifestName);
            file_manifest_path = _FileNameAppendHash(file_manifest_path, file_manifest.Version);
            string content = UnityEngine.JsonUtility.ToJson(file_manifest, true);
            System.IO.File.WriteAllText(file_manifest_path, content);


            //4. save last version
            string last_version_path = System.IO.Path.Combine(dst_dir, "last_version.txt");
            System.IO.File.WriteAllText(last_version_path, file_manifest.Version);


            //5. Copy To Streaming Assets
            {
                //5.1 获取需要的文件列表
                HashSet<string> fileNeeds = new HashSet<string>();
                foreach (var p in file_manifest.Files)
                {
                    fileNeeds.Add(p.FullName);
                }
                fileNeeds.Add(FileSetting.ManifestName);

                //5.2 删除不需要的文件
                string streamingassets_dir = System.IO.Path.Combine("Assets/StreamingAssets", FileSetting.StreamingAssetsDir);
                FileUtil.CreateDir(streamingassets_dir);
                string[] file_list = System.IO.Directory.GetFiles(streamingassets_dir);
                foreach (var p in file_list)
                {
                    if (p.EndsWith(".meta"))
                        continue;

                    string file_name = System.IO.Path.GetFileName(p);
                    if (fileNeeds.Contains(file_name))
                        continue;
                    System.IO.File.Delete(p);
                    _delete_file(p + ".meta");
                }

                //5.3 复制新文件
                foreach (var p in file_manifest.Files)
                {
                    string src_file = System.IO.Path.Combine(dst_dir, p.FullName);
                    string dst_file = System.IO.Path.Combine(streamingassets_dir, p.FullName);
                    if (!System.IO.File.Exists(dst_file))
                        System.IO.File.Copy(src_file, dst_file);
                }

                //5.4 复制FileManifest
                {
                    string dst_file = System.IO.Path.Combine(streamingassets_dir, FileSetting.ManifestName);
                    System.IO.File.Copy(file_manifest_path, dst_file,true);
                }
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


    public static class MD5Helper
    {
        private static System.Security.Cryptography.MD5 _Md5 = System.Security.Cryptography.MD5.Create();
        private static System.Text.StringBuilder _StringBuilder = new System.Text.StringBuilder();

        public static string ComputeFile(string file_path)
        {
            using var fs = System.IO.File.OpenRead(file_path);

            byte[] hash = _Md5.ComputeHash(fs);

            _StringBuilder.Clear();
            foreach (var b in hash)
                _StringBuilder.Append(b.ToString("x2"));

            return _StringBuilder.ToString();
        }

        public static string ComputeString(string file_path)
        {
            using var fs = System.IO.File.OpenRead(file_path);


            byte[] hash = _Md5.ComputeHash(fs);

            _StringBuilder.Clear();
            foreach (var b in hash)
                _StringBuilder.Append(b.ToString("x2"));

            return _StringBuilder.ToString();
        }
    }
}
