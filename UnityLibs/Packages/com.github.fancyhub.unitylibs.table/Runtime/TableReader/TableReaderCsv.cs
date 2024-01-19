using System;
using System.Collections.Generic;

namespace FH
{
    public class TableReaderCsv : ITableReader
    {
        private CsvReader _csv_reader = null;
        public List<Str> _list_str = new List<Str>();
        public TableRowReaderCsv _row_reader;

        public ETableReaderType ReaderType => ETableReaderType.Csv;

        public void Reset(byte[] buff)
        {
            if (buff == null)
                _csv_reader = null;
            else
                _csv_reader = new CsvReader(buff);
        }

        public List<Str> ReadHeader()
        {
            if (_csv_reader == null)
                return null;
            _list_str.Clear();
            bool ret = _csv_reader.ReadRow(_list_str);
            ret &= _csv_reader.ReadRow(_list_str);
            return _list_str;
        }

        public bool NextRow(out ITableRowReader rowReader)
        {
            rowReader = null;
            _list_str.Clear();
            if (!_csv_reader.ReadRow(_list_str))
                return false;

            if (_row_reader == null)
                _row_reader = new TableRowReaderCsv();
            _row_reader.Reset(_list_str);
            rowReader = _row_reader;
            return true;
        }
    }

    public class TableRowReaderCsv : ITableRowReader
    {
        public int col_index = 0;
        public TableListReaderCsv _list_reader;
        public TableTupleReaderCsv _tuple_reader;
        public List<Str> _list_str;
        public void Reset(List<Str> list_str)
        {
            _list_str = list_str;
            col_index = 0;
        }

        public float ReadF32() { return _list_str[col_index++].ParseFloat(); }
        public double ReadF64() { return _list_str[col_index++].ParseDouble(); }
        public int ReadInt32() { return _list_str[col_index++].ParseInt32(); }
        public long ReadInt64() { return _list_str[col_index++].ParseInt64(); }
        public Str ReadString() { return _list_str[col_index++]; }
        public uint ReadUInt32() { return _list_str[col_index++].ParseUInt32(); }
        public ulong ReadUInt64() { return _list_str[col_index++].ParseUInt64(); }
        public bool ReadBool() { return !_list_str[col_index++].Equals("0"); }

        public ITableTupleReader BeginTuple()
        {
            if (_tuple_reader == null)
                _tuple_reader = new TableTupleReaderCsv();
            _tuple_reader.Reset(ReadString());
            if (_tuple_reader.GetCount() == 0)
                return null;
            return _tuple_reader;
        }

        public ITableListReader BeginList()
        {
            if (_list_reader == null)
                _list_reader = new TableListReaderCsv();
            _list_reader.Reset(ReadString());
            return _list_reader;
        }
    }

    public class TableTupleReaderCsv : ITableTupleReader
    {
        private const char C_CSV_PAIR_SPLIT = '|';
        public List<Str> _str_list = new List<Str>();
        public int _index = 0;
        public void Reset(Str str)
        {
            if (str.IsEmpty())
            {
                _str_list.Clear();
            }
            else
                str.Split(C_CSV_PAIR_SPLIT, _str_list);
            _index = 0;
        }

        public int GetCount()
        {
            return _str_list.Count;
        }
        public bool ReadBool() { return !_str_list[_index++].Equals("0"); }
        public int ReadInt32() { return _str_list[_index++].ParseInt32(); }
        public uint ReadUInt32() { return _str_list[_index++].ParseUInt32(); }
        public long ReadInt64() { return _str_list[_index++].ParseInt64(); }
        public ulong ReadUInt64() { return _str_list[_index++].ParseUInt64(); }
        public float ReadF32() { return _str_list[_index++].ParseFloat(); }
        public double ReadF64() { return _str_list[_index++].ParseDouble(); }
        public Str ReadString() { return _str_list[_index++]; }
    }

    public class TableListReaderCsv : ITableListReader
    {
        private const char C_CSV_LIST_SPLIT = ';';
        public TableTupleReaderCsv _tuple_reader;
        public List<Str> _str_list = new List<Str>();
        public int _index = 0;
        public void Reset(Str str)
        {
            if (str.IsEmpty())
                _str_list.Clear();
            else
                str.Split(C_CSV_LIST_SPLIT, _str_list);
            _index = 0;
        }

        public int GetCount()
        {
            if (_str_list.Count == 0) return 0;
            return 1;
        }

        public bool ReadBool() { return !_str_list[_index++].Equals("0"); }
        public int ReadInt32() { return _str_list[_index++].ParseInt32(); }
        public uint ReadUInt32() { return _str_list[_index++].ParseUInt32(); }
        public long ReadInt64() { return _str_list[_index++].ParseInt64(); }
        public ulong ReadUInt64() { return _str_list[_index++].ParseUInt64(); }
        public float ReadF32() { return _str_list[_index++].ParseFloat(); }
        public double ReadF64() { return _str_list[_index++].ParseDouble(); }
        public Str ReadString() { return _str_list[_index++]; }

        public ITableTupleReader BeginTuple()
        {
            if (_tuple_reader == null)
                _tuple_reader = new TableTupleReaderCsv();
            _tuple_reader.Reset(ReadString());
            if (_tuple_reader.GetCount() == 0)
                return null;
            return _tuple_reader;
        }
    }
}
