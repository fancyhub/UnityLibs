
/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/9 14:08:21
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;

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
            if (!SAFileSystemDef.CheckPath(file_path))
                return null;

            string path = CAssetDir + file_path.Substring(SAFileSystemDef.StreamingAssetsDir.Length);

            return Zip64EntryStream.Create(_ObbPath, path);
        }

        public byte[] ReadAllBytes(string file_path)
        {
            if (!SAFileSystemDef.CheckPath(file_path))
                return null;

            string path = CAssetDir + file_path.Substring(SAFileSystemDef.StreamingAssetsDir.Length);
            try
            {
                using Stream stream = OpenRead(file_path);
                if (stream == null)
                    return null;

                if (stream.Length > int.MaxValue)
                    throw new NotSupportedException($"ReadAllBytes doesn't support files larger than {int.MaxValue} bytes: {path}");

                byte[] ret = new byte[stream.Length];
                int offset = 0;
                while (offset < ret.Length)
                {
                    int len = stream.Read(ret, offset, ret.Length - offset);
                    if (len <= 0)
                        break;
                    offset += len;
                }

                if (offset != ret.Length)
                    throw new EndOfStreamException($"读取长度和Entry不一致: {path}");

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
                using Zip64FileReader zipFile = new Zip64FileReader(_ObbPath);

                foreach (var p in zipFile.Entries)
                {
                    string path = p.FullName;
                    if (!path.StartsWith(CAssetDir))
                        continue;

                    string file_path = SAFileSystemDef.StreamingAssetsDir + path.Substring(CAssetDir.Length);
                    _FileList.Add(file_path);
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }
            return _FileList;
        }


        internal class Zip64EntryStream : Stream, IDisposable
        {
            private Zip64FileReader _Reader;
            private Stream _Stream;

            public static Zip64EntryStream Create(string zip_file_path, string path)
            {
                Zip64FileReader zipArchive = null;
                try
                {
                    zipArchive = new Zip64FileReader(zip_file_path);

                    Zip64FileReader.Entry entry = zipArchive.GetEntry(path);
                    if (entry == null)
                    {
                        zipArchive.Dispose();
                        zipArchive = null;
                        return null;
                    }

                    Stream stream = zipArchive.OpenEntry(entry);
                    if (stream == null)
                    {
                        zipArchive.Dispose();
                        zipArchive = null;
                        return null;
                    }

                    return new Zip64EntryStream()
                    {
                        _Reader = zipArchive,
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
                _Reader?.Dispose();
                _Reader = null;
            }
        }
    }
}
