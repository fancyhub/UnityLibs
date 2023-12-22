
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
using System.IO.Compression;
using System.Reflection;

namespace FH.StreamingAssetsFileSystem
{
    internal sealed class SAFileSystem_Obb : ISAFileSystem
    {
        private const string CAssetDir = "assets/";
        private string _ObbPath;

        private List<string> _FileList;

        public SAFileSystem_Obb(string ObbPath)
        {
            _ObbPath = ObbPath;
        }

        public void Dispose()
        {

        }
        public Stream OpenRead(string file_path)
        {
            if (string.IsNullOrEmpty(file_path))
                return null;

            string path = System.IO.Path.Combine(CAssetDir, file_path);

            return ZipEntryStream.Create(_ObbPath, path);
        }

        public byte[] ReadAllBytes(string file_path)
        {
            if (string.IsNullOrEmpty(file_path))
                return null;

            try
            {
                string path = System.IO.Path.Combine(CAssetDir, file_path);
                using ZipArchive archive = ZipFile.OpenRead(_ObbPath);
                ZipArchiveEntry entry = archive.GetEntry(path);


                Stream stream = entry.Open();

                byte[] ret = new byte[entry.Length];
                int len = stream.Read(ret, 0, ret.Length);
                if (len != entry.Length)
                {
                    UnityEngine.Debug.Assert(len == entry.Length, "读取长度和Entry不一致");
                    return null;
                }
                return ret;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return null;
            }
        }

        public List<string> GetAllFileList()
        {
            if (_FileList != null)
                return _FileList;
            _FileList = new List<string>();
            try
            {
                using ZipArchive zipFile = ZipFile.OpenRead(_ObbPath);

                foreach (var p in zipFile.Entries)
                {
                    string path = p.FullName;
                    if (!path.StartsWith(CAssetDir))
                        continue;
                    _FileList.Add(path.Substring(CAssetDir.Length));
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            return _FileList;
        }


        internal class ZipEntryStream : Stream, IDisposable
        {
            private ZipArchive _Archive;
            private Stream _Stream;

            public static ZipEntryStream Create(string zip_file_path, string path)
            {
                ZipArchive zipArchive = null;
                try
                {
                    zipArchive = ZipFile.OpenRead(zip_file_path);

                    ZipArchiveEntry entry = zipArchive.GetEntry(path);
                    if (entry == null)
                    {
                        zipArchive.Dispose();
                        zipArchive = null;
                        return null;
                    }

                    Stream stream = entry.Open();
                    if (stream == null)
                    {
                        zipArchive.Dispose();
                        zipArchive = null;
                        return null;
                    }

                    return new ZipEntryStream()
                    {
                        _Archive = zipArchive,
                        _Stream = stream
                    };
                }
                catch (Exception e)
                {
                    zipArchive?.Dispose();
                    UnityEngine.Debug.LogException(e);
                    return null;
                }
            }

            public override bool CanRead => _Stream.CanRead;

            public override bool CanSeek => _Stream.CanSeek;

            public override bool CanWrite => _Stream.CanWrite;

            public override long Length => _Stream.Length;

            public override long Position { get => _Stream.Position; set => _Stream.Position = value; }

            public override void Flush()
            {
                _Stream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _Stream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _Stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _Stream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _Stream.Write(buffer, offset, count);
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                _Stream?.Dispose();
                _Stream = null;
                _Archive?.Dispose();
                _Archive = null;
            }
        }
    }
}
