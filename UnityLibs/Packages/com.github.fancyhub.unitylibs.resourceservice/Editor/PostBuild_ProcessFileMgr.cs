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
using UnityEditor;

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
            Dictionary<string, string> dict = new Dictionary<string, string>();

            string src_dir = context.Config.GetOutputDir(context.Target);
            string dst_dir = System.IO.Path.Combine(DestFolder, context.Config.Target2Name(context.Target));
            FileUtil.CreateDir(dst_dir);

            FileManifest file_manifest = new FileManifest();
            foreach (var p in graph.Bundles)
            {
                string src_file = System.IO.Path.Combine(src_dir, p.Name);
                string hash = context.Manifest.GetAssetBundleHash(p.Name).ToString();

                string dest_file_name = _FileNameAppendHash(p.Name, hash, DefaultExt);
                string dest_file = System.IO.Path.Combine(dst_dir, dest_file_name);

                _CopyFile(src_file, dest_file, false);

                dict.Add(p.Name, dest_file_name);
                file_manifest.Files.Add(new FileManifest.FileItem()
                {
                    Name = p.Name,
                    FileName = dest_file_name,
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

                dict.Add(ResService.BunldeManifestName, dst_file_name);

                file_manifest.Files.Add(new FileManifest.FileItem()
                {
                    Name = ResService.BunldeManifestName,
                    FileName = dst_file_name,
                    Size = _GetFileSize(dst_file),
                    UseGz = false,
                    GzSize = 0,
                });
            }

            //3. Save File Manifest
            {
                string path = System.IO.Path.Combine(dst_dir, FileSetting.ManifestName);
                string content=UnityEngine.JsonUtility.ToJson(file_manifest,true);
                System.IO.File.WriteAllText(path, content);
            }
        }

        private static int _GetFileSize(string file_path)
        {
            System.IO.FileInfo info = new FileInfo(file_path);
            return (int)info.Length;
        }

        private static string _FileNameAppendHash(string file_name, string hash, string default_ext)
        {
            string ext = System.IO.Path.GetExtension(file_name);
            if (string.IsNullOrEmpty(ext))
                return file_name + "_" + hash + default_ext;

            file_name = file_name.Substring(0, file_name.Length - ext.Length);
            return file_name + "_" + hash + ext;
        }

        private static void _CopyFile(string src_file, string dest_file, bool gen_gz_file)
        {
            UnityEngine.Debug.Log($"{src_file} -> {dest_file}");

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
