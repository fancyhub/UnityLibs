//自动生成的
using System;
using System.Collections;
using System.Collections.Generic;
using LocStr = FH.LocKeyStr;using LocId = FH.LocKeyId;

namespace FH{

    public partial class Table
    {
        protected static List<System.Object> _tempItemsForCsv = new List<System.Object>(104);
        public abstract bool LoadFromCsv(ITableReader reader);
    }


    public static class TableLoaderCsvUtil
	{
        private static bool _Init=false;
		public static void Init()
		{		
            if(_Init) return;
            _Init=true;

			EnumConverterMgr.RegFunc((v) => (EItemType)v, (v) => (int)v);
			EnumConverterMgr.RegFunc((v) => (EItemSubType)v, (v) => (int)v);
			EnumConverterMgr.RegFunc((v) => (EItemQuality)v, (v) => (int)v);
			EnumConverterMgr.RegFunc((v) => (EItemFlag)v, (v) => (int)v);
		}

        #region Base Reader
        public static void Read(ITableDataReader reader, ref bool v)
        {
            v = reader.ReadBool();
        }
        public static void Read(ITableDataReader reader, ref int v)
        {
            v = reader.ReadInt32();
        }
        public static void Read(ITableDataReader reader, ref uint v)
        {
            v = reader.ReadUInt32();
        }
        public static void Read(ITableDataReader reader, ref long v)
        {
            v = reader.ReadInt64();
        }
        public static void Read(ITableDataReader reader, ref ulong v)
        {
            v = reader.ReadUInt64();
        }
        public static void Read(ITableDataReader reader, ref float v)
        {
            v = reader.ReadF32();
        }
        public static void Read(ITableDataReader reader, ref double v)
        {
            v = reader.ReadF64();
        }
        public static void Read(ITableDataReader reader, ref string v)
        {
            v = reader.ReadString();
        }
        public static void Read(ITableDataReader reader, ref LocStr v)
        {
            v = new LocStr(reader.ReadString());
        }
        public static void Read(ITableDataReader reader, ref LocId v)
        {
            v =new LocId(reader.ReadInt32());
        }
        public static void Read<T>(ITableDataReader reader, ref T v) where T : Enum
        {
            if (!EnumConverterMgr.Convert(reader.ReadInt32(), ref v))
            {
                Log.E("没有找到类型 {0} 的转换", typeof(T));
            }
        }
        #endregion
		#region Tuple Reader

        public static void ReadTuple(ITableTupleReader tupleReader, ref (int,bool) v)
        {
            if(tupleReader==null)
                return;

			Read(tupleReader,ref v.Item1);
			Read(tupleReader,ref v.Item2);
		}

        public static void ReadTuple(ITableTupleReader tupleReader, ref PairItemIntBool v, out (int,bool) v2)
        {
            v2=default;
            if(tupleReader==null)
            {
                TableAlias.Create(ref v, false,v2);                
                return;
            }
			Read(tupleReader,ref v2.Item1);
			Read(tupleReader,ref v2.Item2);

             TableAlias.Create(ref v,false,v2);
        }

        public static void ReadTuple(ITableTupleReader tupleReader, ref PairItemIntInt64 v, out (int,int) v2)
        {
            v2=default;
            if(tupleReader==null)
            {
                TableAlias.Create(ref v, false,v2);                
                return;
            }
			Read(tupleReader,ref v2.Item1);
			Read(tupleReader,ref v2.Item2);

             TableAlias.Create(ref v,false,v2);
        }

        public static void ReadTuple(ITableTupleReader tupleReader, ref (int,long) v)
        {
            if(tupleReader==null)
                return;

			Read(tupleReader,ref v.Item1);
			Read(tupleReader,ref v.Item2);
		}

        public static void ReadTuple(ITableTupleReader tupleReader, ref (float,float,float) v)
        {
            if(tupleReader==null)
                return;

			Read(tupleReader,ref v.Item1);
			Read(tupleReader,ref v.Item2);
			Read(tupleReader,ref v.Item3);
		}
		#endregion

		#region List Reader

        public static void ReadList(ITableRowReader rowReader, ref (int,long)[]v)
        {
            var listReader = rowReader.BeginList();
            int count = listReader != null ? listReader.GetCount() : 0;
            if (count == 0)
                v = Array.Empty<(int,long)>();
            else
            {
                v = new (int,long)[count];
                for (int i = 0; i < count; i++)
                {
                    (int,long) item = default;
                    ReadTuple(listReader.BeginTuple(), ref item);                    
                    v[i] = item;
                }
            }
        }

        public static void ReadList(ITableRowReader rowReader, ref int[]v)
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
                    Read(listReader, ref item);                    
                    v[i] = item;
                }
            }
        }
		#endregion
		#region String alias
		#endregion
}

    public sealed partial class TableTItemData
    {        
        public override bool LoadFromCsv(ITableReader reader)
        {   
            int col_count = 11;

            //Check Header
            var header = reader.ReadHeader();
            if (header==null || header.Count != (col_count*2))
            {
                Log.E("加载错误 {0},表头数量不对", SheetName);
                return false;
            }
            bool head_rst = true;
            
			head_rst &= ((header[0] == "Id") && (header[0+11] == "int32"));
			head_rst &= ((header[1] == "Name") && (header[1+11] == "locid"));
			head_rst &= ((header[2] == "Type") && (header[2+11] == "int32"));
			head_rst &= ((header[3] == "SubType") && (header[3+11] == "int32"));
			head_rst &= ((header[4] == "Quality") && (header[4+11] == "int32"));
			head_rst &= ((header[5] == "PairField") && (header[5+11] == "int32_bool"));
			head_rst &= ((header[6] == "PairField2") && (header[6+11] == "int32_bool"));
			head_rst &= ((header[7] == "PairField3") && (header[7+11] == "int32_int32"));
			head_rst &= ((header[8] == "PairFieldList") && (header[8+11] == "list_int32_int64"));
			head_rst &= ((header[9] == "PairFieldList2") && (header[9+11] == "list_int32_int64"));
			head_rst &= ((header[10] == "ListField") && (header[10+11] == "list_int32"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", SheetName);
                return false;
            }

            //加载数据
            _tempItemsForCsv.Clear();
            for (; ; )
            {
                if (!reader.NextRow(out var rowReader))
                    break;                
                var row = new TItemData();
				TableLoaderCsvUtil.Read(rowReader, ref row.Id);
				TableLoaderCsvUtil.Read(rowReader, ref row.Name);
				TableLoaderCsvUtil.Read(rowReader, ref row.Type);
				TableLoaderCsvUtil.Read(rowReader, ref row.SubType);
				TableLoaderCsvUtil.Read(rowReader, ref row.Quality);
				TableLoaderCsvUtil.ReadTuple(rowReader.BeginTuple(), ref row.PairField);
				TableLoaderCsvUtil.ReadTuple(rowReader.BeginTuple(), ref row.PairField2, out (int,bool) __PairField2);
				TableLoaderCsvUtil.ReadTuple(rowReader.BeginTuple(), ref row.PairField3, out (int,int) __PairField3);
				TableLoaderCsvUtil.ReadList(rowReader, ref row.PairFieldList);
				TableLoaderCsvUtil.ReadList(rowReader, ref row.PairFieldList2);
				TableLoaderCsvUtil.ReadList(rowReader, ref row.ListField);

                _tempItemsForCsv.Add(row);
            }

            //转换数据
            List.Clear();
            if (List.Capacity < _tempItemsForCsv.Count)
                List.Capacity = _tempItemsForCsv.Count;
            foreach (var p in _tempItemsForCsv)
            {
                List.Add(p as TItemData);
            }          
            _tempItemsForCsv.Clear();
            return true;
            }
		}

    public sealed partial class TableTLoc
    {        
        public override bool LoadFromCsv(ITableReader reader)
        {   
            int col_count = 2;

            //Check Header
            var header = reader.ReadHeader();
            if (header==null || header.Count != (col_count*2))
            {
                Log.E("加载错误 {0},表头数量不对", SheetName);
                return false;
            }
            bool head_rst = true;
            
			head_rst &= ((header[0] == "Id") && (header[0+2] == "int32"));
			head_rst &= ((header[1] == "Val") && (header[1+2] == "string"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", SheetName);
                return false;
            }

            //加载数据
            _tempItemsForCsv.Clear();
            for (; ; )
            {
                if (!reader.NextRow(out var rowReader))
                    break;                
                var row = new TLoc();
				TableLoaderCsvUtil.Read(rowReader, ref row.Id);
				TableLoaderCsvUtil.Read(rowReader, ref row.Val);

                _tempItemsForCsv.Add(row);
            }

            //转换数据
            List.Clear();
            if (List.Capacity < _tempItemsForCsv.Count)
                List.Capacity = _tempItemsForCsv.Count;
            foreach (var p in _tempItemsForCsv)
            {
                List.Add(p as TLoc);
            }          
            _tempItemsForCsv.Clear();
            return true;
            }
		}

    public sealed partial class TableTTestComposeKey
    {        
        public override bool LoadFromCsv(ITableReader reader)
        {   
            int col_count = 5;

            //Check Header
            var header = reader.ReadHeader();
            if (header==null || header.Count != (col_count*2))
            {
                Log.E("加载错误 {0},表头数量不对", SheetName);
                return false;
            }
            bool head_rst = true;
            
			head_rst &= ((header[0] == "Id") && (header[0+5] == "uint32"));
			head_rst &= ((header[1] == "Level") && (header[1+5] == "int32"));
			head_rst &= ((header[2] == "Name") && (header[2+5] == "locid"));
			head_rst &= ((header[3] == "Pos") && (header[3+5] == "float32_float32_float32"));
			head_rst &= ((header[4] == "Flags") && (header[4+5] == "int32"));

            if (!head_rst)
            {
                Log.E("加载错误 {0}, 表头不匹配", SheetName);
                return false;
            }

            //加载数据
            _tempItemsForCsv.Clear();
            for (; ; )
            {
                if (!reader.NextRow(out var rowReader))
                    break;                
                var row = new TTestComposeKey();
				TableLoaderCsvUtil.Read(rowReader, ref row.Id);
				TableLoaderCsvUtil.Read(rowReader, ref row.Level);
				TableLoaderCsvUtil.Read(rowReader, ref row.Name);
				TableLoaderCsvUtil.ReadTuple(rowReader.BeginTuple(), ref row.Pos);
				TableLoaderCsvUtil.Read(rowReader, ref row.Flags);

                _tempItemsForCsv.Add(row);
            }

            //转换数据
            List.Clear();
            if (List.Capacity < _tempItemsForCsv.Count)
                List.Capacity = _tempItemsForCsv.Count;
            foreach (var p in _tempItemsForCsv)
            {
                List.Add(p as TTestComposeKey);
            }          
            _tempItemsForCsv.Clear();
            return true;
            }
		}
}
