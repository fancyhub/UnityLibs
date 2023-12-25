/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2020/7/31
 * Title   : 
 * Desc    : 
*************************************************************************************/

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
        public string _file_name;
        public int _hash_code;
        //在整个文件的offset
        public uint _offset;
        //原来的文件大小
        public uint _orig_len;
        //在整个文件的压缩后的size
        public uint _compressed_len;
        //该文件是否被压缩了
        public bool _compressed = false;

        public Lz4ZipEntryInfo(
            string name
            , uint offset
            , uint orig_len
            , uint compress_len
            , bool compressed)
        {
            _file_name = name;
            _hash_code = name.GetHashCode();
            _offset = offset;
            _compressed_len = compress_len;
            _orig_len = orig_len;
            _compressed = compressed;
        }

        public override int GetHashCode()
        {
            return _hash_code;
        }

        public string GetFileName()
        {
            return _file_name;
        }

        public uint GetOffset()
        {
            return _offset;
        }

        public uint GetFileOrigSize()
        {
            return _orig_len;
        }

        public uint GetFileCompresedSize()
        {
            return _compressed_len;
        }
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
        public static UInt32 C_FILE_SIGN = 0x4C53475A; // 'LSGZ';
        public static UInt32 C_FILE_VERSION = 0x1;
        public static UInt32 C_MAX_FILE_COUNT = 10000;
        public static UInt16 C_MAX_FILE_NAME_LEN = 256;

        //压缩用的输入文件
        public struct ZipFileInput
        {
            public string _root_dir;
            public FileInfo[] _files;
        }

        //文件列表
        public Lz4ZipEntryInfo[] _file_list;
        //文件列表的缓存
        public Dictionary<string, Lz4ZipEntryInfo> _file_dict;
        //原始的文件io
        public System.IO.Stream _file_stream;
        //最大文件的原始大小
        public uint _max_orig_file_size = 0;
        //gzipfile 只能读取一个文件，要保留上一个打开的子文件的stream，打开下一个的，会关闭上一个io
        private Stream _last_stream;

        public LZ4.LZ4Stream _lz4_stream_reader;

        private Lz4ZipFile(Stream file_stream, Lz4ZipEntryInfo[] file_list, uint max_size)
        {
            _file_stream = file_stream;

            _file_list = file_list;
            _max_orig_file_size = max_size;
            _file_dict = new Dictionary<string, Lz4ZipEntryInfo>(file_list.Length);

            foreach (var a in _file_list)
            {
                _file_dict.Add(a.GetFileName(), a);
            }
        }

        /// <summary>
        /// 如果在外部调用方法，要注意该流会被关闭
        /// </summary>
        public static Lz4ZipFile LoadFile(Stream stream_in)
        {
            if (stream_in == null) return null;
            if (!stream_in.CanRead) return null;
            System.IO.Stream file_stream = stream_in;

            byte[] temp_buff = new byte[4096];

            //1.读取文件头
            {
                int read_size = file_stream.Read(temp_buff, 0, 8);
                if (read_size < 8)
                {
                    Log.E("Gzip load failed {0}", 0);
                    file_stream.Close();
                    return null;
                }

                uint file_sign = BitConverter.ToUInt32(temp_buff, 0);
                if (file_sign != C_FILE_SIGN)
                {
                    Log.E("Gzip load failed {0}", 1);
                    file_stream.Close();
                    return null;
                }
                uint file_version = BitConverter.ToUInt32(temp_buff, 4);
                if (file_version != C_FILE_VERSION)
                {
                    Log.E("Gzip load failed {0}", 2);
                    file_stream.Close();
                    return null;
                }
            }

            uint file_count = 0;
            //2. 读取 GZipFileInfo 的count
            {
                int read_size = file_stream.Read(temp_buff, 0, 4);
                if (read_size < 4)
                {
                    Log.E("Gzip load failed {0}", 3);
                    file_stream.Close();
                    return null;
                }

                file_count = BitConverter.ToUInt32(temp_buff, 0);
                if (file_count == 0 || file_count > C_MAX_FILE_COUNT)
                {
                    Log.E("Gzip load failed {0}", 4);
                    file_stream.Close();
                    return null;
                }
            }

            //3. 读取 GZipFileInfo 列表
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
                        int read_size = file_stream.Read(temp_buff, 0, 2);
                        if (read_size < 2)
                        {
                            Log.E("Gzip load failed {0}", 4);
                            file_stream.Close();
                            return null;
                        }
                        string_len = BitConverter.ToUInt16(temp_buff, 0);
                        if (string_len == 0 || string_len > C_MAX_FILE_NAME_LEN)
                        {
                            Log.E("Gzip load failed {0}", 5);
                            file_stream.Close();
                            return null;
                        }
                    }

                    //3.2 读取 string，并且后面的几个字节也读取
                    {
                        //额外13个字节
                        int read_size = file_stream.Read(temp_buff, 0, string_len + 13);
                        if (read_size < (string_len + 13))
                        {
                            Log.E("Gzip load failed {0}", 6);
                            file_stream.Close();
                            return null;
                        }
                    }

                    string file_name = System.Text.Encoding.UTF8.GetString(temp_buff, 0, string_len);
                    uint orig_len = BitConverter.ToUInt32(temp_buff, string_len);

                    uint offset = BitConverter.ToUInt32(temp_buff, string_len + 4);
                    uint compressed_len = BitConverter.ToUInt32(temp_buff, string_len + 8);
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

        public static Lz4ZipFile LoadFile(string file)
        {
            if (!System.IO.File.Exists(file))
            {
                Log.E("file[{0}] doesn't exist!", file);
                return null;
            }

            FileStream fs = System.IO.File.OpenRead(file);

            Lz4ZipFile ret = LoadFile(fs);
            return ret;
        }

        public uint GetMaxFileSize()
        {
            return _max_orig_file_size;
        }

        public bool FileExists(string path)
        {
            if (_find_file_info(path) == null) return false;
            return true;
        }

        public Lz4ZipEntryInfo FindFile(string file)
        {
            return _find_file_info(file);
        }

        public Stream OpenRead(string file)
        {
            if (_file_stream == null) return null;

            Lz4ZipEntryInfo file_info = _find_file_info(file);
            if (file_info == null) return null;

            if (null != _last_stream)
            {
                _last_stream.Close();
                _last_stream = null;
            }

            Stream stream_in = _get_inner_stream_reader(file_info);

            //外面套接一下
            _last_stream = new GzipFileReadStream(stream_in, file_info.GetFileOrigSize());

            return _last_stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="path"></param>
        /// <returns>-1 读取失败,要不然返回读取的文件大小</returns>
        public int ReadFileAllBytes(byte[] buffer, int offset, string path)
        {
            Lz4ZipEntryInfo file_info = _find_file_info(path);
            if (file_info == null) return -1;


            uint file_size = file_info.GetFileOrigSize();
            if ((buffer.Length - offset) < file_size)
            {
                Log.D("Gzip Load File failde,becuase given buffer is too small");
                return -1;
            }

            Stream stream_in = _get_inner_stream_reader(file_info);

            int read_size = stream_in.Read(buffer, offset, (int)file_size);
            return read_size;
        }

        public byte[] ReadFileAllBytes(string path)
        {
            Lz4ZipEntryInfo file_info = _find_file_info(path);
            return ReadFileAllBytes(file_info);
        }

        public byte[] ReadFileAllBytes(Lz4ZipEntryInfo file_info)
        {
            if (file_info == null) return null;

            Stream stream_in = _get_inner_stream_reader(file_info);

            byte[] buf = new byte[file_info.GetFileOrigSize()];
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

            if (_file_stream != null)
            {
                _file_stream.Close();
                _file_stream = null;
            }
            _lz4_stream_reader = null;

            _file_list = null;
        }

        public bool IsClosed()
        {
            if (_file_stream == null)
            {
                return true;
            }
            else
                return false;
        }

        public void UnZip(string dest_folder)
        {
            if (_file_stream == null) return;

            if (null != _last_stream)
            {
                _last_stream.Close();
                _last_stream = null;
            }

            Lz4ZipEntryInfo[] file_list = _file_list;
            byte[] buff = new byte[4096];

            for (int i = 0; i < file_list.Length; i++)
            {
                Lz4ZipEntryInfo file_info = file_list[i];

                //1. 创建文件夹
                string path = System.IO.Path.Combine(dest_folder, file_info.GetFileName());
                string folder = System.IO.Path.GetDirectoryName(path);
                if (!System.IO.Directory.Exists(folder))
                    System.IO.Directory.CreateDirectory(folder);

                //2. 创建对应的流
                Stream stream_in = _get_inner_stream_reader(file_info);

                int len = (int)file_info.GetFileOrigSize();
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

        #region Unzip 

        public static void UnZipFile(string zip_file, string dest_folder)
        {
            Lz4ZipFile file = Lz4ZipFile.LoadFile(zip_file);
            if (file == null) return;
            file.UnZip(dest_folder);
        }
        #endregion

        #region Create GZ File

        /// <summary>
        ///  创建一个Zip 文件
        ///   CreateZipFile(root_dir, "*.lua", dest_gz_file, false);
        /// </summary>
        /// <param name="root_dir"></param>
        /// <param name="searchPattern"></param>
        /// <param name="dest_gz_file"></param>
        /// <param name="compressed"></param>
        public static void CreateZipFile(string root_dir, string searchPattern, string dest_gz_file, bool compressed = false)
        {
            DirectoryInfo di = new DirectoryInfo(root_dir);
            FileInfo[] files = di.GetFiles(searchPattern, SearchOption.AllDirectories);
            if (files.Length == 0 || files.Length > C_MAX_FILE_COUNT)
                return;

            CreateZipFile(root_dir, files, dest_gz_file, compressed);
        }


        public static void CreateZipFile(string root_dir, FileInfo[] files, string dest_gz_file, bool compressed = false)
        {
            GZipFileCreater.CreateZipFile(root_dir, files, dest_gz_file, compressed);
        }

        public static void CreateZipFile(
          List<Lz4ZipFile.ZipFileInput> input_files,
          string dest_gz_file,
          bool compressed = false)
        {
            GZipFileCreater.CreateZipFile(input_files, dest_gz_file, compressed);
        }

        #endregion

        // 查找文件
        public Lz4ZipEntryInfo _find_file_info(string path)
        {
            Lz4ZipEntryInfo ret = null;
            _file_dict.TryGetValue(path, out ret);
            return ret;
        }

        //获取内部的读取流，会根据 file info 是否压缩来判断 使用什么流
        public Stream _get_inner_stream_reader(Lz4ZipEntryInfo file_info)
        {
            if (null != _last_stream)
            {
                _last_stream.Close();
                _last_stream = null;
            }
            _file_stream.Seek(file_info.GetOffset(), SeekOrigin.Begin);

            if (!file_info._compressed)
                return _file_stream;

            if (null == _lz4_stream_reader)
            {
                _lz4_stream_reader = new LZ4.LZ4Stream(
                    _file_stream
                    , System.IO.Compression.CompressionMode.Decompress
                    , LZ4.LZ4StreamFlags.IsolateInnerStream);
            }
            else
                _lz4_stream_reader.ResetRead();
            return _lz4_stream_reader;
        }

        //单独的创建 zip 的类
        internal static class GZipFileCreater
        {

            public class GZipFileInfoWrite
            {
                public Lz4ZipEntryInfo _info;
                public uint _self_offset;
                public uint _self_size;

                public GZipFileInfoWrite(Lz4ZipEntryInfo file)
                {
                    _info = file;
                }
            }


            public static void CreateZipFile(string root_dir, FileInfo[] files, string dest_gz_file, bool compressed = false)
            {
                Lz4ZipFile.ZipFileInput input = new Lz4ZipFile.ZipFileInput();
                input._files = files;
                input._root_dir = root_dir;
                List<Lz4ZipFile.ZipFileInput> input_files = new List<Lz4ZipFile.ZipFileInput>();
                input_files.Add(input);
                CreateZipFile(input_files, dest_gz_file, compressed);
            }

            //主要入口
            public static void CreateZipFile(
                List<Lz4ZipFile.ZipFileInput> input_files,
                string dest_gz_file,
                bool compressed = false)
            {
                Dictionary<string, FileInfo> file_map;
                List<GZipFileInfoWrite> zip_file_info_list;

                //1. 创建文件列表
                _create_file_dict(input_files, compressed, out file_map, out zip_file_info_list);


                //2. 创建stream，如果原来文件存在，删除
                if (System.IO.File.Exists(dest_gz_file))
                {
                    System.IO.File.Delete(dest_gz_file);
                }
                Stream fout = File.OpenWrite(dest_gz_file);

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
                    _write_file_list(fout, zip_file_info_list);
                }

                //5. 写入文件
                //如果压缩率低于 该值，就改用非压缩的方法
                float compress_ratio = 0.9f;
                byte[] temp_buffer = new byte[4096];
                for (int i = 0; i < zip_file_info_list.Count; i++)
                {
                    GZipFileInfoWrite zip_file_info = zip_file_info_list[i];
                    FileInfo file_info = file_map[zip_file_info._info._file_name];
                    _write_single_file(fout, zip_file_info, file_info, temp_buffer, compress_ratio);
                }
                fout.Close();
            }


            public static void _create_file_dict(
                List<Lz4ZipFile.ZipFileInput> file_list
                , bool default_compress
                , out Dictionary<string, FileInfo> out_file_dict
                , out List<GZipFileInfoWrite> out_file_list)
            {
                out_file_dict = new Dictionary<string, FileInfo>();
                out_file_list = new List<GZipFileInfoWrite>();
                foreach (var input in file_list)
                {
                    DirectoryInfo di = new DirectoryInfo(input._root_dir);
                    string full_root_path = di.FullName;

                    full_root_path = full_root_path.Replace('\\', '/');
                    if (!full_root_path.EndsWith("/"))
                        full_root_path = full_root_path + "/";

                    for (int i = 0; i < input._files.Length; i++)
                    {
                        FileInfo file_info = input._files[i];
                        string file_full_name = file_info.FullName;
                        file_full_name = file_full_name.Replace('\\', '/');

                        if (!file_full_name.StartsWith(full_root_path))
                        {
                            continue;
                        }
                        string file_name = file_full_name.Substring(full_root_path.Length);

                        //先把数据填进去
                        Lz4ZipEntryInfo gzip_file_info = new Lz4ZipEntryInfo(
                            file_name
                            , 0
                            , (uint)file_info.Length
                            , (uint)file_info.Length
                            , default_compress);

                        out_file_dict.Add(file_name, file_info);
                        out_file_list.Add(new GZipFileInfoWrite(gzip_file_info));
                    }
                }
            }


            public static void _write_file_list(Stream fout, List<GZipFileInfoWrite> zip_file_info_list)
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

                    GZipFileInfoWrite fi = zip_file_info_list[i];
                    //写文件名
                    {
                        byte[] name_bytes = System.Text.Encoding.UTF8.GetBytes(fi._info.GetFileName());
                        byte[] temp_bytes = BitConverter.GetBytes((ushort)name_bytes.Length);
                        fout.Write(temp_bytes, 0, temp_bytes.Length);
                        fout.Write(name_bytes, 0, name_bytes.Length);
                    }

                    {
                        byte[] temp_bytes = BitConverter.GetBytes(fi._info.GetFileOrigSize());
                        fout.Write(temp_bytes, 0, temp_bytes.Length);

                        temp_bytes = BitConverter.GetBytes(fi._info.GetOffset());
                        fout.Write(temp_bytes, 0, temp_bytes.Length);

                        temp_bytes = BitConverter.GetBytes(fi._info.GetFileCompresedSize());
                        fout.Write(temp_bytes, 0, temp_bytes.Length);

                        if (fi._info._compressed)
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


            public static void _write_single_file(
                Stream fout
                , GZipFileInfoWrite zip_file_info
                , FileInfo file_info
                , byte[] temp_buffer
                , float compress_ratio)
            {
                //1. 先记录当前的offset
                zip_file_info._info._offset = (uint)fout.Position;

                //2. 开始写文件        
                if (zip_file_info._info._compressed)
                {
                    long file_size_write = _write_single_file_compressed(new EmptyStreamWriter(), file_info, temp_buffer);

                    //如果压缩率太低了，就改用非压缩模式
                    if (file_size_write >= (zip_file_info._info._orig_len * compress_ratio))
                    {
                        zip_file_info._info._compressed = false;
                    }
                }

                if (zip_file_info._info._compressed)
                {
                    long file_size_write = _write_single_file_compressed(fout, file_info, temp_buffer);
                    zip_file_info._info._compressed_len = (uint)file_size_write;
                }
                else
                {
                    long file_size_write = _write_single_file_no_compressed(fout, file_info, temp_buffer);
                    zip_file_info._info._compressed_len = (uint)file_size_write;
                }


                //3. 回写 文件的偏移和 压缩后的size
                long offset_now = fout.Position;
                long offset = zip_file_info._self_size + zip_file_info._self_offset - 9;
                fout.Seek(offset, SeekOrigin.Begin);

                byte[] temp_bytes = BitConverter.GetBytes(zip_file_info._info.GetOffset());
                fout.Write(temp_bytes, 0, temp_bytes.Length);

                temp_bytes = BitConverter.GetBytes(zip_file_info._info.GetFileCompresedSize());
                fout.Write(temp_bytes, 0, temp_bytes.Length);

                if (zip_file_info._info._compressed)
                    fout.WriteByte(1);
                else
                    fout.WriteByte(0);
                fout.Flush();

                fout.Seek(offset_now, SeekOrigin.Begin);
            }


            public static long _write_single_file_compressed(
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

            public static long _write_single_file_no_compressed(
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
        internal class EmptyStreamWriter : Stream
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


        //一个套接stream
        internal class GzipFileReadStream : Stream
        {
            public Stream _orig_stream;
            public uint _file_len;
            public long _pos;
            public GzipFileReadStream(Stream orig, uint file_len)
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
