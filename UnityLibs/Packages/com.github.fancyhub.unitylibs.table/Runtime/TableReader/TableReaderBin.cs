using System;
using System.Collections.Generic;
using System.IO;

namespace FH
{
    public class TableReaderBinBase
    {
        public MemoryStream _ms;
        public BinaryReader _reader;
        public List<string> _str_list;

        public void Reset(byte[] buff)
        {
            _ms = null;
            _reader = null;
            if (buff == null)
                return;

            _ms = new MemoryStream(buff);
            _reader = new BinaryReader(_ms);

            int sign = _reader.ReadInt32();
            //Read Str
            {
                //int body_len = _reader.Read7BitEncodedInt();
                //int count = _reader.Read7BitEncodedInt();

                int body_len = _reader.ReadInt32();
                int count = _reader.ReadInt32();
                _str_list = new List<string>(count);
                for (int i = 0; i < count; i++)
                {
                    _str_list.Add(_reader.ReadString());
                }
            }
        }

        public void SeekBegin(int offset)
        {
            _ms.Seek(offset, SeekOrigin.Begin);
        }

        public string ReadRefString()
        {
            //int idx = _reader.Read7BitEncodedInt();
            int idx = _reader.ReadInt32();
            return _str_list[idx];
        }

        public int ReadCount()
        {
            //return _reader.Read7BitEncodedInt();
            return _reader.ReadInt32();
        }

        public bool ReadBool()
        {
            return _reader.ReadByte() == 1;
        }

        public float ReadF32()
        {
            return _reader.ReadSingle();
        }

        public double ReadF64()
        {
            return _reader.ReadDouble();
        }

        public int ReadInt32()
        {
            return _reader.ReadInt32();
            //return _reader.Read7BitEncodedInt();
        }
        public uint ReadUInt32()
        {
            //return (uint)_reader.Read7BitEncodedInt();
            return (uint) _reader.ReadUInt32();
        }

        public long ReadInt64()
        {
            //return _reader.Read7BitEncodedInt64();
            return _reader.ReadInt64();
        }

        public ulong ReadUInt64()
        {
            //return (ulong)_reader.Read7BitEncodedInt64();
            return (ulong) _reader.ReadUInt64();
        }

        public Str ReadString()
        {
            return ReadRefString();
        }

        public (int offset, int len) ReadSheetOffset()
        {
            var offset = _reader.ReadInt32();
            var len = ReadCount();
            return (offset, len);
        }
    }

    public class TableReaderBin : ITableReader
    {
        public TableReaderBinBase _base_reader = new TableReaderBinBase();
        private TableRowReaderBin _row_reader = new TableRowReaderBin();
        public Dictionary<string, (int offset, int len)> _table_dict;
        public List<Str> _str_list_header = new List<Str>();

        public int _row_count;
        public int _row_index;

        public string CurLang;

        public ETableReaderType ReaderType => ETableReaderType.Bin;
        public void Reset(byte[] buff)
        {
            _base_reader.Reset(buff);
            _table_dict?.Clear();
            if (buff == null)
                return;

            //table index
            int body_len = _base_reader.ReadCount();
            int count = _base_reader.ReadCount();
            _table_dict = new Dictionary<string, (int offset, int len)>(count);
            for (int i = 0; i < count; i++)
            {
                string sheet_name = _base_reader.ReadRefString();
                var sheetOffset = _base_reader.ReadSheetOffset();
                _table_dict.Add(sheet_name, sheetOffset);
            }
        }

        public bool Start(string sheet_name)
        {
            if (!_table_dict.TryGetValue(sheet_name, out var t))
                return false;

            _base_reader.SeekBegin(t.offset);
            _row_count = 0;
            _row_index = 0;
            return true;
        }

        public List<Str> ReadHeader()
        {
            _str_list_header.Clear();
            int count = _base_reader.ReadCount();
            for (int i = 0; i < count; i++)
            {
                _str_list_header.Add(_base_reader.ReadRefString());
            }
            _row_count = _base_reader.ReadCount();
            _row_index = 0;
            return _str_list_header;
        }

        public bool NextRow(out ITableRowReader rowReader)
        {
            rowReader = null;
            _row_index++;
            if (_row_index > _row_count)
                return false;
            int row_len = _base_reader.ReadCount();

            _row_reader.Reset(_base_reader);
            rowReader = _row_reader;
            return true;
        }
    }


    public class TableRowReaderBin : ITableRowReader
    {
        public TableReaderBinBase _base_reader;
        public TableListReaderBin _list_reader;
        public TablePairReaderBin _pair_reader;
        public void Reset(TableReaderBinBase reader)
        {
            _base_reader = reader;
        }

        public bool ReadBool() { return _base_reader.ReadBool(); }
        public int ReadInt32() { return _base_reader.ReadInt32(); }
        public uint ReadUInt32() { return _base_reader.ReadUInt32(); }
        public long ReadInt64() { return _base_reader.ReadInt64(); }
        public ulong ReadUInt64() { return _base_reader.ReadUInt64(); }
        public float ReadF32() { return _base_reader.ReadF32(); }
        public double ReadF64() { return _base_reader.ReadF64(); }
        public Str ReadString() { return _base_reader.ReadString(); }

        public ITableListReader BeginList()
        {
            if (_list_reader == null)
            {
                _list_reader = new TableListReaderBin();
            }

            _list_reader.Reset(_base_reader, _base_reader.ReadCount());
            return _list_reader;
        }

        public ITableTupleReader BeginTuple()
        {
            if (_pair_reader == null)
                _pair_reader = new TablePairReaderBin();
            int count = _base_reader.ReadCount();
            _pair_reader.Reset(_base_reader, count);
            if (count == 0)
                return null;
            return _pair_reader;
        }
    }

    public class TableListReaderBin : ITableListReader
    {
        public TablePairReaderBin _pair_reader;
        public TableReaderBinBase _orig_reader;
        public int _count;
        public void Reset(TableReaderBinBase reader, int count)
        {
            _orig_reader = reader;
            _count = count;
        }

        public int GetCount()
        {
            return _count;
        }


        public bool ReadBool() { return _orig_reader.ReadBool(); }
        public int ReadInt32() { return _orig_reader.ReadInt32(); }
        public uint ReadUInt32() { return _orig_reader.ReadUInt32(); }
        public long ReadInt64() { return _orig_reader.ReadInt64(); }
        public ulong ReadUInt64() { return _orig_reader.ReadUInt64(); }
        public float ReadF32() { return _orig_reader.ReadF32(); }
        public double ReadF64() { return _orig_reader.ReadF64(); }
        public Str ReadString() { return _orig_reader.ReadString(); }

        public ITableTupleReader BeginTuple()
        {
            if (_pair_reader == null)
                _pair_reader = new TablePairReaderBin();
            _pair_reader.Reset(_orig_reader, 0);
            return _pair_reader;
        }
    }


    public class TablePairReaderBin : ITableTupleReader
    {
        public TableReaderBinBase _orig_reader;
        public int _count;
        public void Reset(TableReaderBinBase reader, int count)
        {
            _orig_reader = reader;
            _count = count;
        }

        public int GetCount()
        {
            return _count;
        }

        public bool ReadBool() { return _orig_reader.ReadBool(); }
        public int ReadInt32() { return _orig_reader.ReadInt32(); }
        public uint ReadUInt32() { return _orig_reader.ReadUInt32(); }
        public long ReadInt64() { return _orig_reader.ReadInt64(); }
        public ulong ReadUInt64() { return _orig_reader.ReadUInt64(); }
        public float ReadF32() { return _orig_reader.ReadF32(); }
        public double ReadF64() { return _orig_reader.ReadF64(); }
        public Str ReadString() { return _orig_reader.ReadString(); }
    }
}
