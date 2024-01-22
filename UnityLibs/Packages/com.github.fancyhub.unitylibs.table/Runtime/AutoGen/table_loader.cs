//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
using LocStr = FH.LocKey;
using Log = FH.TableLog;

namespace FH
{

    public delegate bool CreateTableReader(string sheet_name, string lang_name, out ITableReader reader);
    public delegate Table TableLoader(string lang);
    public struct TableInfo
    {
        public TableLoader Loader;
        public bool MultiLang;
        public TableInfo(TableLoader loader, bool multiLang)
        {
            this.Loader = loader;
            this.MultiLang = multiLang;
        }
    }
    public partial class TableLoaderMgr
    {
        private static List<System.Object> _temp = new List<System.Object>(10000);
        public static List<string> LangList;

        public CreateTableReader CreateTableReader;
        public Dictionary<Type, TableInfo> LoaderDict;
        static TableLoaderMgr()
        {
            LangList = new List<string>(2);

            LangList.Add("en");
            LangList.Add("zh-Hans");
            EnumConverterMgr.RegFunc((v) => (EItemType)v, (v) => (int)v);
            EnumConverterMgr.RegFunc((v) => (EItemSubType)v, (v) => (int)v);
            EnumConverterMgr.RegFunc((v) => (EItemQuality)v, (v) => (int)v);
            EnumConverterMgr.RegFunc((v) => (EItemFlag)v, (v) => (int)v);
        }

        public TableLoaderMgr(CreateTableReader createTableReader)
        {
            CreateTableReader = createTableReader;
            LoaderDict = new Dictionary<Type, TableInfo>(20 + 2);


            LoaderDict.Add(typeof(TItemData), new TableInfo(_LoadItemData, false));
            LoaderDict.Add(typeof(TLoc), new TableInfo(_LoadLoc, true));
        }

        private Table _LoadItemData(string lang)
        {
            string sheet_name = "ItemData";
            lang = null;
            int col_count = 11;

            if (!CreateTableReader(sheet_name, lang, out var reader))
                return null;

            //Check Header
            var header = reader.ReadHeader();
            if (header == null || header.Count != (col_count * 2))
            {
                Log.E("加载错误 {0},表头数量不对", sheet_name);
                return null;
            }
            bool head_rst = true;

            head_rst &= ((header[0] == "Id") && (header[0 + 11] == "int32"));
            head_rst &= ((header[1] == "Name") && (header[1 + 11] == "locid"));
            head_rst &= ((header[2] == "Type") && (header[2 + 11] == "int32"));
            head_rst &= ((header[3] == "SubType") && (header[3 + 11] == "int32"));
            head_rst &= ((header[4] == "Quality") && (header[4 + 11] == "int32"));
            head_rst &= ((header[5] == "PairField") && (header[5 + 11] == "int32_bool"));
            head_rst &= ((header[6] == "PairField2") && (header[6 + 11] == "int32_bool"));
            head_rst &= ((header[7] == "PairField3") && (header[7 + 11] == "int32_int32"));
            head_rst &= ((header[8] == "PairFieldList") && (header[8 + 11] == "list_int32_int64"));
            head_rst &= ((header[9] == "PairFieldList2") && (header[9 + 11] == "list_int32_int64"));
            head_rst &= ((header[10] == "ListField") && (header[10 + 11] == "list_int32"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", sheet_name);
                return null;
            }

            //加载数据
            _temp.Clear();
            for (; ; )
            {
                if (!reader.NextRow(out var rowReader))
                    break;
                var row = new TItemData();
                _Read(rowReader, ref row.Id);
                _Read(rowReader, ref row.Name);
                _Read(rowReader, ref row.Type);
                _Read(rowReader, ref row.SubType);
                _Read(rowReader, ref row.Quality);
                _ReadTuple(rowReader.BeginTuple(), ref row.PairField);
                _ReadTuple(rowReader.BeginTuple(), ref row.PairField2, out (int, bool) __PairField2);
                _ReadTuple(rowReader.BeginTuple(), ref row.PairField3, out (int, int) __PairField3);
                _ReadList(rowReader, ref row.PairFieldList);
                _ReadList(rowReader, ref row.PairFieldList2, out (int, long) __PairFieldList2);
                _ReadList(rowReader, ref row.ListField);

                _temp.Add(row);
            }

            //转换数据
            List<TItemData> list = new List<TItemData>(_temp.Count);
            foreach (var p in _temp)
            {
                list.Add(p as TItemData);
            }


            var dict = new Dictionary<int, TItemData>(list.Count);
            foreach (var p in list)
            {
                if (dict.ContainsKey(p.Id))
                {
                    Log.E("{0} Contain Multi Id: {1}, 如果允许ID重复, 修改表格", typeof(TItemData), p.Id);
                    continue;
                }
                dict.Add(p.Id, p);
            }
            return Table.Create(list, dict);

        }

        private Table _LoadLoc(string lang)
        {
            string sheet_name = "Loc";

            int col_count = 2;

            if (!CreateTableReader(sheet_name, lang, out var reader))
                return null;

            //Check Header
            var header = reader.ReadHeader();
            if (header == null || header.Count != (col_count * 2))
            {
                Log.E("加载错误 {0},表头数量不对", sheet_name);
                return null;
            }
            bool head_rst = true;

            head_rst &= ((header[0] == "Id") && (header[0 + 2] == "int32"));
            head_rst &= ((header[1] == "Val") && (header[1 + 2] == "string"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", sheet_name);
                return null;
            }

            //加载数据
            _temp.Clear();
            for (; ; )
            {
                if (!reader.NextRow(out var rowReader))
                    break;
                var row = new TLoc();
                _Read(rowReader, ref row.Id);
                _Read(rowReader, ref row.Val);

                _temp.Add(row);
            }

            //转换数据
            List<TLoc> list = new List<TLoc>(_temp.Count);
            foreach (var p in _temp)
            {
                list.Add(p as TLoc);
            }


            var dict = new Dictionary<int, TLoc>(list.Count);
            foreach (var p in list)
            {
                if (dict.ContainsKey(p.Id))
                {
                    Log.E("{0} Contain Multi Id: {1}, 如果允许ID重复, 修改表格", typeof(TLoc), p.Id);
                    continue;
                }
                dict.Add(p.Id, p);
            }
            return Table.Create(list, dict);

        }

        #region Base Reader
        private static void _Read(ITableDataReader reader, ref bool v)
        {
            v = reader.ReadBool();
        }
        private static void _Read(ITableDataReader reader, ref int v)
        {
            v = reader.ReadInt32();
        }
        private static void _Read(ITableDataReader reader, ref uint v)
        {
            v = reader.ReadUInt32();
        }
        private static void _Read(ITableDataReader reader, ref long v)
        {
            v = reader.ReadInt64();
        }
        private static void _Read(ITableDataReader reader, ref ulong v)
        {
            v = reader.ReadUInt64();
        }
        private static void _Read(ITableDataReader reader, ref float v)
        {
            v = reader.ReadF32();
        }
        private static void _Read(ITableDataReader reader, ref double v)
        {
            v = reader.ReadF64();
        }
        private static void _Read(ITableDataReader reader, ref string v)
        {
            v = reader.ReadString();
        }
        private static void _Read(ITableDataReader reader, ref LocStr v)
        {
            v = new LocStr(reader.ReadString());
        }
        private static void _Read(ITableDataReader reader, ref LocId v)
        {
            v = new LocId(reader.ReadInt32());
        }
        private static void _Read<T>(ITableDataReader reader, ref T v) where T : Enum
        {
            if (!EnumConverterMgr.Convert(reader.ReadInt32(), ref v))
            {
                Log.E("没有找到类型 {0} 的转换", typeof(T));
            }
        }
        #endregion
        #region Tuple Reader

        private static void _ReadTuple(ITableTupleReader tupleReader, ref (int, bool) v)
        {
            if (tupleReader == null)
                return;

            _Read(tupleReader, ref v.Item1);
            _Read(tupleReader, ref v.Item2);
        }

        private static void _ReadTuple(ITableTupleReader tupleReader, ref PairItemIntBool v, out (int, bool) v2)
        {
            v2 = default;
            if (tupleReader == null)
            {
                TableAlias.Create(ref v, false, v2);
                return;
            }
            _Read(tupleReader, ref v2.Item1);
            _Read(tupleReader, ref v2.Item2);

            TableAlias.Create(ref v, false, v2);
        }

        private static void _ReadTuple(ITableTupleReader tupleReader, ref PairItemIntBool v, out (int, int) v2)
        {
            v2 = default;
            if (tupleReader == null)
            {
                TableAlias.Create(ref v, false, v2);
                return;
            }
            _Read(tupleReader, ref v2.Item1);
            _Read(tupleReader, ref v2.Item2);

            TableAlias.Create(ref v, false, v2);
        }

        private static void _ReadTuple(ITableTupleReader tupleReader, ref (int, long) v)
        {
            if (tupleReader == null)
                return;

            _Read(tupleReader, ref v.Item1);
            _Read(tupleReader, ref v.Item2);
        }

        private static void _ReadTuple(ITableTupleReader tupleReader, ref PairItemIntInt64 v, out (int, long) v2)
        {
            v2 = default;
            if (tupleReader == null)
            {
                TableAlias.Create(ref v, false, v2);
                return;
            }
            _Read(tupleReader, ref v2.Item1);
            _Read(tupleReader, ref v2.Item2);

            TableAlias.Create(ref v, false, v2);
        }
        #endregion

        #region List Reader

        private static void _ReadList(ITableRowReader rowReader, ref (int, long)[] v)
        {
            var listReader = rowReader.BeginList();
            int count = listReader != null ? listReader.GetCount() : 0;
            if (count == 0)
                v = Array.Empty<(int, long)>();
            else
            {
                v = new (int, long)[count];
                for (int i = 0; i < count; i++)
                {
                    (int, long) item = default;
                    _ReadTuple(listReader.BeginTuple(), ref item);
                    v[i] = item;
                }
            }
        }

        private static void _ReadList(ITableRowReader rowReader, ref PairItemIntInt64[] v, out (int, long) v2)
        {
            v2 = default;
            var listReader = rowReader.BeginList();
            int count = listReader != null ? listReader.GetCount() : 0;
            if (count == 0)
                v = Array.Empty<PairItemIntInt64>();
            else
            {
                v = new PairItemIntInt64[count];
                for (int i = 0; i < count; i++)
                {
                    PairItemIntInt64 item = default;
                    _ReadTuple(listReader.BeginTuple(), ref item, out v2);
                    v[i] = item;
                }
            }
        }

        private static void _ReadList(ITableRowReader rowReader, ref int[] v)
        {
            var listReader = rowReader.BeginList();
            int count = listReader != null ? listReader.GetCount() : 0;
            if (count == 0)
                v = Array.Empty<int>();
            else
            {
                v = new int[count];
                for (int i = 0; i < count; i++)
                {
                    int item = default;
                    _Read(listReader, ref item);
                    v[i] = item;
                }
            }
        }
        #endregion
    }
}
