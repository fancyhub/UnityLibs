/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2020/7/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH.VFSManagement;
using System;
using System.Collections.Generic;
using System.IO;


namespace FH
{
    /// <summary>
    /// Zip 文件的信息头
    /// </summary>
    public class Lz4ZipEntryInfo
    {
        public readonly string FileName;
        //在整个文件的offset
        internal uint _Offset;
        //原来的文件大小
        internal uint _OrigSize;
        //在整个文件的压缩后的size
        internal uint _CompressedLength;
        //该文件是否被压缩了
        internal bool _Compressed = false;

        internal Lz4ZipEntryInfo(
            string name
            , uint offset
            , uint orig_len
            , uint compress_len
            , bool compressed)
        {
            FileName = name;
            _Offset = offset;
            _CompressedLength = compress_len;
            _OrigSize = orig_len;
            _Compressed = compressed;
        }

        public override int GetHashCode()
        {
            if (FileName == null)
                return 0;
            return FileName.GetHashCode();
        }

        public uint Offset => _Offset;
        public uint OrigSize => _OrigSize;
        public uint CompresedSize => _CompressedLength;
        public bool IsCompressed => _Compressed;
    }


    /*
    Header: 
        FileSign: UInt32
        FileVersion:UInt32                
        FileCount : UInt32

        FileHeader Repeated:
            FileNameLen:Uint16
            FileName:string,            
            FileOrigLen:UInt32
            FileOffset:UInt32,            
            FileCompressedLen:UInt32
            FileCompress:Bool
    Body:
        Repeated:
        FileContent:binary
    */

    public sealed class Lz4ZipFile
    {
        private const uint C_FILE_SIGN = 0x4C53475A; // 'LSGZ';
        private const uint C_FILE_VERSION = 0x1;
        private const uint C_MAX_FILE_COUNT = 10000;
        private const ushort C_MAX_FILE_NAME_LEN = 256;

        //文件列表
        private Lz4ZipEntryInfo[] _EntryList;
        //文件列表的缓存
        private Dictionary<string, Lz4ZipEntryInfo> _EntryDict;
        //原始的文件io
        private System.IO.Stream _ZipFileOrigStream;
        //最大文件的原始大小
        private uint _MaxOrigFileSize = 0;

        //gzipfile 只能读取一个文件，要保留上一个打开的子文件的stream，打开下一个的，会关闭上一个io
        private Stream _last_stream;
        private LZ4.LZ4Stream _lz4_stream_reader;

        private Lz4ZipFile(Stream file_stream, Lz4ZipEntryInfo[] file_list, uint max_size)
        {
            _ZipFileOrigStream = file_stream;

            _EntryList = file_list;
            _MaxOrigFileSize = max_size;
            _EntryDict = new Dictionary<string, Lz4ZipEntryInfo>(file_list.Length);

            foreach (var a in _EntryList)
            {
                _EntryDict.Add(a.FileName, a);
            }
        }

        /// <summary>
        /// 如果在外部调用方法，要注意该流会被关闭
        /// </summary>
        public static unsafe Lz4ZipFile LoadFromStream(Stream stream_in)
        {
            if (stream_in == null) return null;
            if (!stream_in.CanRead) return null;
            System.IO.Stream file_stream = stream_in;

            Span<byte> temp_buff = stackalloc byte[4096];

            //1.读取文件头
            {
                int read_size = file_stream.Read(temp_buff.Slice(0, 8));
                if (read_size < 8)
                {
                    VfsLog._.E("Gzip load failed {0}", 0);
                    file_stream.Close();
                    return null;
                }

                uint file_sign = BitConverter.ToUInt32(temp_buff.Slice(0, 4));
                if (file_sign != C_FILE_SIGN)
                {
                    VfsLog._.E("Gzip load failed {0}", 1);
                    file_stream.Close();
                    return null;
                }
                uint file_version = BitConverter.ToUInt32(temp_buff.Slice(4, 4));
                if (file_version != C_FILE_VERSION)
                {
                    VfsLog._.E("Gzip load failed {0}", 2);
                    file_stream.Close();
                    return null;
                }
            }

            uint file_count = 0;
            //2. 读取 GZipFileInfo 的count
            {
                int read_size = file_stream.Read(temp_buff.Slice(0, 4));
                if (read_size < 4)
                {
                    VfsLog._.E("Lz4Zip load failed {0}", 3);
                    file_stream.Close();
                    return null;
                }

                file_count = BitConverter.ToUInt32(temp_buff.Slice(0, 4));
                if (file_count == 0 || file_count > C_MAX_FILE_COUNT)
                {
                    VfsLog._.E("Lz4Zip load failed {0}", 4);
                    file_stream.Close();
                    return null;
                }
            }

            //3. 读取 Lz4ZipEntryInfo 列表
            uint max_orig_file_size = 0;
            Lz4ZipEntryInfo[] file_list = new Lz4ZipEntryInfo[file_count];
            {
                //FileNameLen: Uint16
                //FileName:string,
                //FileOrigLen:UInt32

                //FileOffset:UInt32,            
                //FileCompressedLen: UInt32
                //FileCompress:Bool
                for (int i = 0; i < file_count; i++)
                {
                    //3.1 读取string的size
                    ushort string_len = 0;
                    {
                        int read_size = file_stream.Read(temp_buff.Slice(0, 2));
                        if (read_size < 2)
                        {
                            VfsLog._.E("Lz4Zip load failed {0}", 4);
                            file_stream.Close();
                            return null;
                        }
                        string_len = BitConverter.ToUInt16(temp_buff.Slice(0, 2));
                        if (string_len == 0 || string_len > C_MAX_FILE_NAME_LEN)
                        {
                            VfsLog._.E("Lz4Zip load failed {0}", 5);
                            file_stream.Close();
                            return null;
                        }
                    }

                    //3.2 读取 string，并且后面的几个字节也读取
                    {
                        //额外13个字节
                        int read_size = file_stream.Read(temp_buff.Slice(0, string_len + 13));
                        if (read_size < (string_len + 13))
                        {
                            VfsLog._.E("Lz4Zip load failed {0}", 6);
                            file_stream.Close();
                            return null;
                        }
                    }

                    string file_name = System.Text.Encoding.UTF8.GetString(temp_buff.Slice(0, string_len));
                    uint orig_len = BitConverter.ToUInt32(temp_buff.Slice(string_len, 4));

                    uint offset = BitConverter.ToUInt32(temp_buff.Slice(string_len + 4, 4));
                    uint compressed_len = BitConverter.ToUInt32(temp_buff.Slice(string_len + 8, 4));
                    bool compressed_flag = temp_buff[string_len + 12] == 1;

                    Lz4ZipEntryInfo file_info = new Lz4ZipEntryInfo(
                        file_name
                        , offset
                        , orig_len
                        , compressed_len
                        , compressed_flag);

                    file_list[i] = file_info;

                    if (orig_len > max_orig_file_size)
                    {
                        max_orig_file_size = orig_len;
                    }
                }
            }

            Lz4ZipFile ret = new Lz4ZipFile(file_stream, file_list, max_orig_file_size);
            return ret;
        }

        public static Lz4ZipFile LoadFromFile(string file)
        {
            VfsLog._.D("Load Lz4File {0}", file);
            if (!System.IO.File.Exists(file))
            {
                VfsLog._.E("file[{0}] doesn't exist!", file);
                return null;
            }
            FileStream fs = System.IO.File.OpenRead(file);
            Lz4ZipFile ret = LoadFromStream(fs);
            return ret;
        }

        public uint GetMaxFileSize()
        {
            return _MaxOrigFileSize;
        }

        public bool FileExists(string path)
        {
            if (_FindEntry(path) == null) return false;
            return true;
        }

        public Lz4ZipEntryInfo FindEntry(string file)
        {
            return _FindEntry(file);
        }

        public Stream OpenRead(string file)
        {
            if (_ZipFileOrigStream == null) return null;

            Lz4ZipEntryInfo file_info = _FindEntry(file);
            if (file_info == null) return null;

            if (null != _last_stream)
            {
                _last_stream.Close();
                _last_stream = null;
            }

            Stream stream_in = _get_inner_stream_reader(file_info);

            //外面套接一下
            _last_stream = new EntryReadStream(stream_in, file_info.OrigSize);

            return _last_stream;
        }

        /// <summary>
        /// 
        /// </summary>        
        /// <returns>-1 读取失败,要不然返回读取的文件大小</returns>
        public int ReadFileAllBytes(string file_path, byte[] buffer, int offset)
        {
            Lz4ZipEntryInfo file_info = _FindEntry(file_path);
            if (file_info == null) return -1;


            uint file_size = file_info.OrigSize;
            if ((buffer.Length - offset) < file_size)
            {
                VfsLog._.D("Lz4Zip Load File failde,becuase given buffer is too small");
                return -1;
            }

            Stream stream_in = _get_inner_stream_reader(file_info);

            int read_size = stream_in.Read(buffer, offset, (int)file_size);
            return read_size;
        }

        public string ReadAllText(string path)
        {
            Lz4ZipEntryInfo file_info = _FindEntry(path);
            if (file_info == null)
                return null;

            Stream stream_in = _get_inner_stream_reader(file_info);
            Span<byte> buff = stackalloc byte[(int)file_info.OrigSize];

            int read_size = stream_in.Read(buff);
            if (read_size < buff.Length)
                return null;
            return System.Text.Encoding.UTF8.GetString(buff);
        }

        public byte[] ReadFileAllBytes(string path)
        {
            Lz4ZipEntryInfo file_info = _FindEntry(path);
            return ReadFileAllBytes(file_info);
        }

        public byte[] ReadFileAllBytes(Lz4ZipEntryInfo file_info)
        {
            if (file_info == null) return null;

            Stream stream_in = _get_inner_stream_reader(file_info);
            byte[] buf = new byte[file_info.OrigSize];
            int read_size = stream_in.Read(buf, 0, buf.Length);
            if (read_size < buf.Length)
            {
                return null;
            }
            return buf;
        }

        public void Close()
        {
            if (null != _last_stream)
            {
                _last_stream.Close();
                _last_stream = null;
            }

            if (_ZipFileOrigStream != null)
            {
                _ZipFileOrigStream.Close();
                _ZipFileOrigStream = null;
            }
            _lz4_stream_reader = null;

            _EntryList = null;
        }

        public bool IsClosed()
        {
            if (_ZipFileOrigStream == null)
            {
                return true;
            }
            else
                return false;
        }

        #region Unzip 

        public void UnZip(string dest_folder)
        {
            if (_ZipFileOrigStream == null) return;

            if (null != _last_stream)
            {
                _last_stream.Close();
                _last_stream = null;
            }

            Lz4ZipEntryInfo[] file_list = _EntryList;
            byte[] buff = new byte[4096];

            for (int i = 0; i < file_list.Length; i++)
            {
                Lz4ZipEntryInfo file_info = file_list[i];

                //1. 创建文件夹
                string path = System.IO.Path.Combine(dest_folder, file_info.FileName);
                string folder = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                //2. 创建对应的流
                Stream stream_in = _get_inner_stream_reader(file_info);

                int len = (int)file_info.OrigSize;
                System.IO.FileStream fs_out = System.IO.File.OpenWrite(path);
                while (true)
                {
                    int read_size = len < buff.Length ? len : buff.Length;
                    stream_in.Read(buff, 0, read_size);
                    fs_out.Write(buff, 0, read_size);
                    len = len - read_size;
                    if (len <= 0)
                        break;
                }
                fs_out.Close();
            }
        }

        public static void UnZipFile(string zip_file, string dest_folder)
        {
            Lz4ZipFile file = Lz4ZipFile.LoadFromFile(zip_file);
            if (file == null) return;
            file.UnZip(dest_folder);
        }
        #endregion



        // 查找文件
        private Lz4ZipEntryInfo _FindEntry(string file_path)
        {
            if (file_path == null)
                return null;

            Lz4ZipEntryInfo ret = null;
            _EntryDict.TryGetValue(file_path, out ret);
            return ret;
        }

        //获取内部的读取流，会根据 file info 是否压缩来判断 使用什么流
        private Stream _get_inner_stream_reader(Lz4ZipEntryInfo file_info)
        {
            if (null != _last_stream)
            {
                _last_stream.Close();
                _last_stream = null;
            }
            _ZipFileOrigStream.Seek(file_info.Offset, SeekOrigin.Begin);

            if (!file_info._Compressed)
                return _ZipFileOrigStream;

            if (null == _lz4_stream_reader)
            {
                _lz4_stream_reader = new LZ4.LZ4Stream(
                    _ZipFileOrigStream
                    , System.IO.Compression.CompressionMode.Decompress
                    , LZ4.LZ4StreamFlags.IsolateInnerStream);
            }
            else
                _lz4_stream_reader.ResetRead();
            return _lz4_stream_reader;
        }

        #region Create GZ File

        /// <summary>
        ///  创建一个Zip 文件
        ///   CreateZipFile(root_dir, "*.lua", dest_gz_file, false);
        /// </summary>
        /// <param name="root_dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="dest_zip_file_path"></param>
        /// <param name="compressed"></param>
        public static void CreateZipFile(string root_dir, string searchPattern, string dest_zip_file_path, bool compressed = false)
        {
            var input_file_list = Creator.FormatInputFiles(root_dir, searchPattern);
            Creator.CreateZipFile(input_file_list, dest_zip_file_path, compressed);
        }

        public static void CreateZipFile(List<(string inner_path, FileInfo src_file)> input_files, string dest_zip_file_path, bool compressed = false)
        {
            Creator.CreateZipFile(input_files, dest_zip_file_path, compressed);
        }

        //单独的创建 zip 的类
        private static class Creator
        {

            private class FileInfoWrite
            {
                public Lz4ZipEntryInfo EntryInfo;
                public FileInfo FileInfo;
                public uint _self_offset;
                public uint _self_size;
            }

            public static List<(string inner_path, FileInfo src_file)> FormatInputFiles(string root_dir, string searchPattern)
            {
                DirectoryInfo di = new DirectoryInfo(root_dir);
                FileInfo[] files = di.GetFiles(searchPattern, SearchOption.AllDirectories);
                if (files.Length == 0 || files.Length > C_MAX_FILE_COUNT)
                    return null;

                string full_root_path = di.FullName;
                full_root_path = full_root_path.Replace('\\', '/');
                if (!full_root_path.EndsWith("/"))
                    full_root_path = full_root_path + "/";

                List<(string inner_path, FileInfo src_file)> ret = new List<(string inner_path, FileInfo src_file)>();
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file_info = files[i];
                    string file_full_name = file_info.FullName;
                    file_full_name = file_full_name.Replace('\\', '/');

                    if (!file_full_name.StartsWith(full_root_path))
                    {
                        continue;
                    }
                    string file_name = file_full_name.Substring(full_root_path.Length);


                    ret.Add((file_name, file_info));
                }
                return ret;
            }

            //主要入口
            public static void CreateZipFile(List<(string inner_path, FileInfo src_file)> input_files, string dest_zip_file_path, bool compressed = false)
            {
                List<FileInfoWrite> zip_file_info_list = new List<FileInfoWrite>(input_files.Count);
                Dictionary<string, FileInfoWrite> file_map = new Dictionary<string, FileInfoWrite>(input_files.Count);

                //1. 生成Dict
                foreach (var input in input_files)
                {
                    Lz4ZipEntryInfo entry_info = new Lz4ZipEntryInfo(
                        input.inner_path
                        , 0
                        , (uint)input.src_file.Length
                        , (uint)input.src_file.Length
                        , compressed);

                    FileInfoWrite write_info = new FileInfoWrite()
                    {
                        EntryInfo = entry_info,
                        FileInfo = input.src_file,
                    };

                    zip_file_info_list.Add(write_info);
                    file_map.Add(input.inner_path, write_info);
                }

                //2. 创建stream，如果原来文件存在，删除
                if (System.IO.File.Exists(dest_zip_file_path))
                {
                    System.IO.File.Delete(dest_zip_file_path);
                }
                Stream fout = File.OpenWrite(dest_zip_file_path);

                /*
        Header: 
           FileSign: UInt32
           FileVersion:UInt32

           FileCount : UInt32


        Body:
           Repeated:
           FileContent:binary
        */
                //3. 写入文件头
                {
                    byte[] data_bytes = BitConverter.GetBytes(Lz4ZipFile.C_FILE_SIGN);
                    fout.Write(data_bytes, 0, data_bytes.Length);

                    data_bytes = BitConverter.GetBytes(Lz4ZipFile.C_FILE_VERSION);
                    fout.Write(data_bytes, 0, data_bytes.Length);
                }


                //4. 写入文件列表
                {
                    _WriteFileList(fout, zip_file_info_list);
                }

                //5. 写入文件
                //如果压缩率低于 该值，就改用非压缩的方法
                float compress_ratio = 0.9f;
                byte[] temp_buffer = new byte[4096];
                for (int i = 0; i < zip_file_info_list.Count; i++)
                {
                    FileInfoWrite zip_file_info = zip_file_info_list[i];
                    _WriteSingleFile(fout, zip_file_info, temp_buffer, compress_ratio);
                }
                fout.Close();
            }


            private static void _WriteFileList(Stream fout, List<FileInfoWrite> zip_file_info_list)
            {
                byte[] data_bytes = BitConverter.GetBytes(zip_file_info_list.Count);
                fout.Write(data_bytes, 0, data_bytes.Length);
                fout.Flush();

                for (int i = 0; i < zip_file_info_list.Count; i++)
                {
                    long befor_pos = fout.Position;

                    //FileNameLen: Uint16
                    //FileName:string,
                    //FileOrigLen:UInt32

                    //FileOffset:UInt32,            
                    //FileCompressedLen: UInt32
                    //FileCompress:Bool

                    FileInfoWrite fi = zip_file_info_list[i];
                    //写文件名
                    {
                        byte[] name_bytes = System.Text.Encoding.UTF8.GetBytes(fi.EntryInfo.FileName);
                        byte[] temp_bytes = BitConverter.GetBytes((ushort)name_bytes.Length);
                        fout.Write(temp_bytes, 0, temp_bytes.Length);
                        fout.Write(name_bytes, 0, name_bytes.Length);
                    }

                    {
                        byte[] temp_bytes = BitConverter.GetBytes(fi.EntryInfo.OrigSize);
                        fout.Write(temp_bytes, 0, temp_bytes.Length);

                        temp_bytes = BitConverter.GetBytes(fi.EntryInfo.Offset);
                        fout.Write(temp_bytes, 0, temp_bytes.Length);

                        temp_bytes = BitConverter.GetBytes(fi.EntryInfo.CompresedSize);
                        fout.Write(temp_bytes, 0, temp_bytes.Length);

                        if (fi.EntryInfo._Compressed)
                        {
                            fout.WriteByte(1);
                        }
                        else
                            fout.WriteByte(0);
                    }
                    fout.Flush();
                    long now_pos = fout.Position;
                    fi._self_size = (uint)(now_pos - befor_pos);
                    fi._self_offset = (uint)befor_pos;
                }
            }


            private static void _WriteSingleFile(
                Stream fout
                , FileInfoWrite zip_file_info
                , byte[] temp_buffer
                , float compress_ratio)
            {
                //1. 先记录当前的offset
                zip_file_info.EntryInfo._Offset = (uint)fout.Position;

                //2. 开始写文件        
                if (zip_file_info.EntryInfo._Compressed)
                {
                    long file_size_write = _WriteSingleFile_Compressed(new EmptyStreamWriter(), zip_file_info.FileInfo, temp_buffer);

                    //如果压缩率太低了，就改用非压缩模式
                    if (file_size_write >= (zip_file_info.EntryInfo._OrigSize * compress_ratio))
                    {
                        zip_file_info.EntryInfo._Compressed = false;
                    }
                }

                if (zip_file_info.EntryInfo._Compressed)
                {
                    long file_size_write = _WriteSingleFile_Compressed(fout, zip_file_info.FileInfo, temp_buffer);
                    zip_file_info.EntryInfo._CompressedLength = (uint)file_size_write;
                }
                else
                {
                    long file_size_write = _WriteSingleFile_No_Compressed(fout, zip_file_info.FileInfo, temp_buffer);
                    zip_file_info.EntryInfo._CompressedLength = (uint)file_size_write;
                }


                //3. 回写 文件的偏移和 压缩后的size
                long offset_now = fout.Position;
                long offset = zip_file_info._self_size + zip_file_info._self_offset - 9;
                fout.Seek(offset, SeekOrigin.Begin);

                byte[] temp_bytes = BitConverter.GetBytes(zip_file_info.EntryInfo.Offset);
                fout.Write(temp_bytes, 0, temp_bytes.Length);

                temp_bytes = BitConverter.GetBytes(zip_file_info.EntryInfo.CompresedSize);
                fout.Write(temp_bytes, 0, temp_bytes.Length);

                if (zip_file_info.EntryInfo._Compressed)
                    fout.WriteByte(1);
                else
                    fout.WriteByte(0);
                fout.Flush();

                fout.Seek(offset_now, SeekOrigin.Begin);
            }


            public static long _WriteSingleFile_Compressed(
                Stream fout
                , FileInfo file_info
                , byte[] temp_buffer)
            {
                long start_pos = fout.Position;

                var stream_out = new LZ4.LZ4Stream(fout
                      , System.IO.Compression.CompressionMode.Compress
                      , LZ4.LZ4StreamFlags.IsolateInnerStream);

                Stream stream_in = file_info.OpenRead();
                int read_size = stream_in.Read(temp_buffer, 0, temp_buffer.Length);
                while (read_size > 0)
                {
                    stream_out.Write(temp_buffer, 0, read_size);
                    read_size = stream_in.Read(temp_buffer, 0, temp_buffer.Length);
                }
                stream_in.Close();
                stream_out.Close();
                fout.Flush();
                return fout.Position - start_pos;
            }

            public static long _WriteSingleFile_No_Compressed(
                Stream fout
                , FileInfo file_info
                , byte[] temp_buffer)
            {
                Stream stream_in = file_info.OpenRead();
                long start_pos = fout.Position;

                int read_size = stream_in.Read(temp_buffer, 0, temp_buffer.Length);
                while (read_size > 0)
                {
                    fout.Write(temp_buffer, 0, read_size);
                    read_size = stream_in.Read(temp_buffer, 0, temp_buffer.Length);
                }
                stream_in.Close();
                fout.Flush();
                return fout.Position - start_pos;
            }
        }

        //空的stream writer，在创建 gzip的时候，用来判断写入长度的
        private class EmptyStreamWriter : Stream
        {
            public long _pos = 0;
            public override bool CanRead { get { return false; } }

            public override bool CanSeek { get { return false; } }

            public override bool CanWrite { get { return true; } }

            public override long Length { get { return _pos; } }

            public override long Position { get { return _pos; } set => throw new NotImplementedException(); }

            public override void Flush() { }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return 0;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _pos += count;
            }
        }

        #endregion


        //一个套接stream
        private class EntryReadStream : Stream
        {
            public Stream _orig_stream;
            public uint _file_len;
            public long _pos;
            public EntryReadStream(Stream orig, uint file_len)
            {
                _orig_stream = orig;
                _file_len = file_len;
                _pos = 0;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_orig_stream == null)
                    return 0;

                long remain = _file_len - _pos;
                //取最小值
                remain = remain < count ? remain : count;

                int ret = _orig_stream.Read(buffer, offset, (int)remain);
                _pos += ret;
                return ret;
            }

            public override void Close() { _orig_stream = null; }

            public override long Seek(long offset, SeekOrigin origin) { throw new System.NotSupportedException(); }

            public override long Length { get { return _file_len; } }

            public override long Position { get { return _pos; } set { throw new System.NotSupportedException(); } }

            public override void Flush() { return; }

            public override void SetLength(long value) { return; }

            public override void Write(byte[] buffer, int offset, int count) { return; }

            public override bool CanRead { get { return null != _orig_stream; } }

            public override bool CanSeek { get { return false; } }

            public override bool CanWrite { get { return false; } }
        }
    }
}
