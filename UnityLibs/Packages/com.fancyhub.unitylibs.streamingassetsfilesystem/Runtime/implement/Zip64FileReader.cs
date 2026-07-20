/*************************************************************************************
 * Desc    : Read-only ZIP/ZIP64 reader for Android OBB style archives.
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace FH.StreamingAssetsFileSystem
{
    internal sealed class Zip64FileReader : IDisposable
    {
        private const uint C_SIG_EOCD = 0x06054b50;
        private const uint C_SIG_ZIP64_EOCD = 0x06064b50;
        private const uint C_SIG_ZIP64_LOCATOR = 0x07064b50;
        private const uint C_SIG_CENTRAL_DIRECTORY = 0x02014b50;
        private const uint C_SIG_LOCAL_FILE_HEADER = 0x04034b50;

        private const ushort C_GP_FLAG_UTF8 = 1 << 11;
        private const ushort C_METHOD_STORED = 0;
        private const ushort C_METHOD_DEFLATE = 8;
        private const ushort C_ZIP64_EXTRA_ID = 0x0001;

        private readonly string _ZipFilePath;
        private readonly Dictionary<string, Entry> _EntryMap = new Dictionary<string, Entry>();
        private readonly List<Entry> _Entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => _Entries;

        public Zip64FileReader(string zip_file_path)
        {
            _ZipFilePath = zip_file_path;
            using (FileStream stream = File.Open(zip_file_path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ReadCentralDirectory(stream);
            }
        }

        public void Dispose()
        {
        }

        public Entry GetEntry(string path)
        {
            _EntryMap.TryGetValue(path, out Entry entry);
            return entry;
        }

        public Stream OpenEntry(Entry entry)
        {
            if (entry == null)
                return null;

            FileStream stream = File.Open(_ZipFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                long data_offset = GetEntryDataOffset(stream, entry);
                stream.Position = data_offset;

                if (entry.CompressionMethod == C_METHOD_STORED)
                    return new SlicedFileStream(stream, data_offset, entry.CompressedSize);

                if (entry.CompressionMethod == C_METHOD_DEFLATE)
                {
                    Stream deflate_stream = new DeflateStream(new BoundedFileStream(stream, entry.CompressedSize), CompressionMode.Decompress);
                    return new DeflatedEntryStream(deflate_stream, entry.UncompressedSize);
                }

                throw new NotSupportedException($"Unsupported zip compression method: {entry.CompressionMethod}");
            }
            catch
            {
                stream.Dispose();
                throw;
            }
        }

        private void ReadCentralDirectory(FileStream stream)
        {
            Eocd eocd = FindEocd(stream);
            long central_dir_offset = eocd.CentralDirectoryOffset;
            long entry_count = eocd.EntryCount;

            if (eocd.NeedsZip64)
            {
                Zip64Eocd zip64_eocd = ReadZip64Eocd(stream, eocd.Offset);
                central_dir_offset = zip64_eocd.CentralDirectoryOffset;
                entry_count = zip64_eocd.EntryCount;
            }

            stream.Position = central_dir_offset;
            for (long i = 0; i < entry_count; i++)
            {
                Entry entry = ReadCentralDirectoryEntry(stream);
                _Entries.Add(entry);
                _EntryMap[entry.FullName] = entry;
            }
        }

        private static Eocd FindEocd(FileStream stream)
        {
            long file_length = stream.Length;
            int read_len = (int)Math.Min(file_length, 65535 + 22);
            byte[] buffer = new byte[read_len];
            long read_start = file_length - read_len;
            stream.Position = read_start;
            ReadExactly(stream, buffer, 0, buffer.Length);

            for (int i = buffer.Length - 22; i >= 0; i--)
            {
                if (ReadUInt32(buffer, i) != C_SIG_EOCD)
                    continue;

                ushort comment_len = ReadUInt16(buffer, i + 20);
                if (i + 22 + comment_len != buffer.Length)
                    continue;

                ushort disk_no = ReadUInt16(buffer, i + 4);
                ushort cd_disk_no = ReadUInt16(buffer, i + 6);
                ushort disk_entries = ReadUInt16(buffer, i + 8);
                ushort total_entries = ReadUInt16(buffer, i + 10);
                uint cd_size = ReadUInt32(buffer, i + 12);
                uint cd_offset = ReadUInt32(buffer, i + 16);

                return new Eocd
                {
                    Offset = read_start + i,
                    EntryCount = total_entries,
                    CentralDirectoryOffset = cd_offset,
                    NeedsZip64 = disk_no == ushort.MaxValue
                        || cd_disk_no == ushort.MaxValue
                        || disk_entries == ushort.MaxValue
                        || total_entries == ushort.MaxValue
                        || cd_size == uint.MaxValue
                        || cd_offset == uint.MaxValue,
                };
            }

            throw new InvalidDataException("End of central directory record was not found.");
        }

        private static Zip64Eocd ReadZip64Eocd(FileStream stream, long eocd_offset)
        {
            const int locator_size = 20;
            if (eocd_offset < locator_size)
                throw new InvalidDataException("ZIP64 EOCD locator was not found.");

            stream.Position = eocd_offset - locator_size;
            uint locator_sig = ReadUInt32(stream);
            if (locator_sig != C_SIG_ZIP64_LOCATOR)
                throw new InvalidDataException("ZIP64 EOCD locator was not found.");

            ReadUInt32(stream);
            ulong zip64_eocd_offset = ReadUInt64(stream);
            ReadUInt32(stream);

            stream.Position = checked((long)zip64_eocd_offset);
            uint zip64_sig = ReadUInt32(stream);
            if (zip64_sig != C_SIG_ZIP64_EOCD)
                throw new InvalidDataException("ZIP64 EOCD record was not found.");

            ReadUInt64(stream);
            ReadUInt16(stream);
            ReadUInt16(stream);
            ReadUInt32(stream);
            ReadUInt32(stream);
            ulong disk_entries = ReadUInt64(stream);
            ulong total_entries = ReadUInt64(stream);
            ulong cd_size = ReadUInt64(stream);
            ulong cd_offset = ReadUInt64(stream);

            if (disk_entries != total_entries)
                throw new NotSupportedException("Spanned ZIP archives are not supported.");

            return new Zip64Eocd
            {
                EntryCount = checked((long)total_entries),
                CentralDirectoryOffset = checked((long)cd_offset),
                CentralDirectorySize = checked((long)cd_size),
            };
        }

        private static Entry ReadCentralDirectoryEntry(FileStream stream)
        {
            uint sig = ReadUInt32(stream);
            if (sig != C_SIG_CENTRAL_DIRECTORY)
                throw new InvalidDataException("Invalid central directory file header.");

            ReadUInt16(stream);
            ReadUInt16(stream);
            ushort flags = ReadUInt16(stream);
            ushort method = ReadUInt16(stream);
            ReadUInt16(stream);
            ReadUInt16(stream);
            uint crc32 = ReadUInt32(stream);
            uint compressed_size_32 = ReadUInt32(stream);
            uint uncompressed_size_32 = ReadUInt32(stream);
            ushort file_name_len = ReadUInt16(stream);
            ushort extra_len = ReadUInt16(stream);
            ushort comment_len = ReadUInt16(stream);
            ReadUInt16(stream);
            ReadUInt16(stream);
            ReadUInt32(stream);
            uint local_header_offset_32 = ReadUInt32(stream);

            byte[] name_buffer = new byte[file_name_len];
            ReadExactly(stream, name_buffer, 0, name_buffer.Length);
            byte[] extra_buffer = new byte[extra_len];
            ReadExactly(stream, extra_buffer, 0, extra_buffer.Length);
            if (comment_len > 0)
                stream.Position += comment_len;

            long compressed_size = compressed_size_32;
            long uncompressed_size = uncompressed_size_32;
            long local_header_offset = local_header_offset_32;

            ReadZip64Extra(
                extra_buffer,
                compressed_size_32 == uint.MaxValue,
                uncompressed_size_32 == uint.MaxValue,
                local_header_offset_32 == uint.MaxValue,
                ref compressed_size,
                ref uncompressed_size,
                ref local_header_offset);

            string full_name = Encoding.UTF8.GetString(name_buffer);

            return new Entry
            {
                FullName = full_name,
                Flags = flags,
                CompressionMethod = method,
                Crc32 = crc32,
                CompressedSize = compressed_size,
                UncompressedSize = uncompressed_size,
                LocalHeaderOffset = local_header_offset,
            };
        }

        private static void ReadZip64Extra(
            byte[] extra_buffer,
            bool read_compressed_size,
            bool read_uncompressed_size,
            bool read_local_header_offset,
            ref long compressed_size,
            ref long uncompressed_size,
            ref long local_header_offset)
        {
            int pos = 0;
            while (pos + 4 <= extra_buffer.Length)
            {
                ushort header_id = ReadUInt16(extra_buffer, pos);
                ushort data_size = ReadUInt16(extra_buffer, pos + 2);
                pos += 4;
                if (pos + data_size > extra_buffer.Length)
                    throw new InvalidDataException("Invalid ZIP extra field length.");

                if (header_id == C_ZIP64_EXTRA_ID)
                {
                    int data_pos = pos;
                    if (read_uncompressed_size)
                        uncompressed_size = checked((long)ReadUInt64(extra_buffer, ref data_pos));
                    if (read_compressed_size)
                        compressed_size = checked((long)ReadUInt64(extra_buffer, ref data_pos));
                    if (read_local_header_offset)
                        local_header_offset = checked((long)ReadUInt64(extra_buffer, ref data_pos));
                    return;
                }

                pos += data_size;
            }
        }

        private static long GetEntryDataOffset(FileStream stream, Entry entry)
        {
            stream.Position = entry.LocalHeaderOffset;
            uint sig = ReadUInt32(stream);
            if (sig != C_SIG_LOCAL_FILE_HEADER)
                throw new InvalidDataException("Invalid local file header.");

            stream.Position += 22;
            ushort file_name_len = ReadUInt16(stream);
            ushort extra_len = ReadUInt16(stream);
            return entry.LocalHeaderOffset + 30L + file_name_len + extra_len;
        }

        private static ushort ReadUInt16(Stream stream)
        {
            int b0 = stream.ReadByte();
            int b1 = stream.ReadByte();
            if ((b0 | b1) < 0)
                throw new EndOfStreamException();
            return (ushort)(b0 | (b1 << 8));
        }

        private static uint ReadUInt32(Stream stream)
        {
            int b0 = stream.ReadByte();
            int b1 = stream.ReadByte();
            int b2 = stream.ReadByte();
            int b3 = stream.ReadByte();
            if ((b0 | b1 | b2 | b3) < 0)
                throw new EndOfStreamException();
            return (uint)(b0 | (b1 << 8) | (b2 << 16) | (b3 << 24));
        }

        private static ulong ReadUInt64(Stream stream)
        {
            uint lo = ReadUInt32(stream);
            uint hi = ReadUInt32(stream);
            return lo | ((ulong)hi << 32);
        }

        private static ushort ReadUInt16(byte[] buffer, int offset)
        {
            return (ushort)(buffer[offset] | (buffer[offset + 1] << 8));
        }

        private static uint ReadUInt32(byte[] buffer, int offset)
        {
            return (uint)(buffer[offset]
                | (buffer[offset + 1] << 8)
                | (buffer[offset + 2] << 16)
                | (buffer[offset + 3] << 24));
        }

        private static ulong ReadUInt64(byte[] buffer, ref int offset)
        {
            if (offset + 8 > buffer.Length)
                throw new InvalidDataException("Invalid ZIP64 extra field length.");

            ulong ret = ReadUInt32(buffer, offset) | ((ulong)ReadUInt32(buffer, offset + 4) << 32);
            offset += 8;
            return ret;
        }

        private static void ReadExactly(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int read = stream.Read(buffer, offset, count);
                if (read <= 0)
                    throw new EndOfStreamException();

                offset += read;
                count -= read;
            }
        }

        internal sealed class Entry
        {
            public string FullName;
            public ushort Flags;
            public ushort CompressionMethod;
            public uint Crc32;
            public long CompressedSize;
            public long UncompressedSize;
            public long LocalHeaderOffset;
        }

        private struct Eocd
        {
            public long Offset;
            public long EntryCount;
            public long CentralDirectoryOffset;
            public bool NeedsZip64;
        }

        private struct Zip64Eocd
        {
            public long EntryCount;
            public long CentralDirectoryOffset;
            public long CentralDirectorySize;
        }

        private sealed class SlicedFileStream : Stream
        {
            private readonly FileStream _Stream;
            private readonly long _Start;
            private readonly long _Length;
            private long _Position;

            public SlicedFileStream(FileStream stream, long start, long length)
            {
                _Stream = stream;
                _Start = start;
                _Length = length;
                _Position = 0;
            }

            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;
            public override long Length => _Length;
            public override long Position { get => _Position; set => Seek(value, SeekOrigin.Begin); }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_Position >= _Length)
                    return 0;

                long remain = _Length - _Position;
                if (count > remain)
                    count = (int)Math.Min(int.MaxValue, remain);

                _Stream.Position = _Start + _Position;
                int read = _Stream.Read(buffer, offset, count);
                _Position += read;
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                long new_pos;
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        new_pos = offset;
                        break;
                    case SeekOrigin.Current:
                        new_pos = _Position + offset;
                        break;
                    case SeekOrigin.End:
                        new_pos = _Length + offset;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
                }

                if (new_pos < 0 || new_pos > _Length)
                    throw new IOException("Seek position is outside the ZIP entry.");

                _Position = new_pos;
                return _Position;
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _Stream.Dispose();
                base.Dispose(disposing);
            }
        }

        private sealed class BoundedFileStream : Stream
        {
            private readonly FileStream _Stream;
            private long _Remain;

            public BoundedFileStream(FileStream stream, long length)
            {
                _Stream = stream;
                _Remain = length;
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_Remain <= 0)
                    return 0;

                if (count > _Remain)
                    count = (int)Math.Min(int.MaxValue, _Remain);

                int read = _Stream.Read(buffer, offset, count);
                _Remain -= read;
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _Stream.Dispose();
                base.Dispose(disposing);
            }
        }

        private sealed class DeflatedEntryStream : Stream
        {
            private readonly Stream _Stream;
            private readonly long _Length;
            private long _Position;

            public DeflatedEntryStream(Stream stream, long length)
            {
                _Stream = stream;
                _Length = length;
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => _Length;
            public override long Position { get => _Position; set => throw new NotSupportedException(); }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int read = _Stream.Read(buffer, offset, count);
                _Position += read;
                return read;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _Stream.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
