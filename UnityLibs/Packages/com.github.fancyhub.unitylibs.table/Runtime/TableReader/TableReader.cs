using System;
using System.Collections.Generic;

namespace FH
{
    public enum ETableReaderType
    {
        None,
        Csv,
        Bin,
    }

    public interface ITableDataReader
    {
        public bool ReadBool();
        public uint ReadUInt32();
        public int ReadInt32();
        public long ReadInt64();
        public ulong ReadUInt64();
        public float ReadF32();
        double ReadF64();
        public Str ReadString();
    }

    public interface ITableTupleReader : ITableDataReader
    {
    }

    public interface ITableRowReader : ITableDataReader
    {
        public ITableListReader BeginList();
        public ITableTupleReader BeginTuple();
    }

    public interface ITableListReader : ITableDataReader
    {
        public int GetCount();
        public ITableTupleReader BeginTuple();
    }

    public interface ITableReader
    {
        public ETableReaderType ReaderType { get; }
        public List<Str> ReadHeader();

        public bool NextRow(out ITableRowReader rowReader);
    }
}
