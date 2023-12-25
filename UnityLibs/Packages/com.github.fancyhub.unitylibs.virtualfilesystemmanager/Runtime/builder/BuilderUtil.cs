using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace FH.VFSManagement.Builder
{
    public static class BuilderUtil
    {
        public static string BuildZip(BuilderConfig config, string out_dir)
        {
            string path = System.IO.Path.Combine(out_dir, config.Name);
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);

            switch (config.Format)
            {
                case EDestFormat.Lz4ZipCompress:
                    {
                        var all_files = config.GetAllFiles();
                        Lz4ZipFile.CreateZipFile(all_files, path, true);
                        return path;
                    }

                case EDestFormat.Lz4ZipStore:
                    {
                        var all_files = config.GetAllFiles();
                        Lz4ZipFile.CreateZipFile(all_files, path, false);
                        return path;
                    }

                case EDestFormat.Zip:
                    {
                        List<(string reletive_file_path, FileInfo file_info)> all_files = config.GetAllFiles();
                        if(all_files.Count==0)
                        {
                            UnityEngine.Debug.LogWarning($"Build Zip, File Is Empty {config.Name}");
                        }

                        var zipArchive = ZipFile.Open(path, ZipArchiveMode.Create);
                        foreach (var f in all_files)
                        {
                            zipArchive.CreateEntryFromFile(f.file_info.FullName, f.reletive_file_path);
                        }
                        zipArchive.Dispose();
                        return path;
                    }
            }
            return null;
        }
    }
}
