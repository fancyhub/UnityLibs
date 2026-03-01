/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FH
{
    public sealed class CsvTableHeader
    {
        private Str[] _Names;
        private Dictionary<string, int> _NameIndexMap;

        internal CsvTableHeader(Str[] data)
        {
            int count = data == null ? 0 : data.Length;
            _Names = data;
            _NameIndexMap = new Dictionary<string, int>(count);

            for (int i = 0; i < count; i++)
            {
                _NameIndexMap[_Names[i].ToString()] = i;
            }
        }

        public int Count { get { return _Names == null ? 0 : _Names.Length; } }

        public int GetIndex(string name)
        {
            if (name == null)
                return -1;
            if (_NameIndexMap == null)
                return -1;
            if (_NameIndexMap.TryGetValue(name, out var index))
                return index;
            return -1;
        }
    }

    public sealed class CsvTableRow
    {
        private CsvTableHeader _Header;
        private Str[] _Data;

        internal CsvTableRow(Str[] data)
        {
            _Data = data;
        }

        public int Count => _Data == null ? 0 : _Data.Length;

        internal void SetHeader(CsvTableHeader header)
        {
            _Header = header;
        }

        public Str this[int index]
        {
            get
            {
                if (index < 0 || _Data == null || index >= _Data.Length)
                    return null;
                return _Data[index];
            }
        }

        public Str this[string name]
        {
            get
            {
                if (_Header == null)
                    return null;
                int index = _Header.GetIndex(name);
                return this[index];
            }
        }
    }

    public sealed partial class CsvTableReader
    {
        public CsvTableHeader Header;
        public List<CsvTableRow> HeadRows;
        public List<CsvTableRow> DataRows;

        public static CsvTableReader ReadAll(CsvReader reader, int headerNameRowIndex, int dataRowIndex)
        {
            int row_index = 0;
            List<Str> row_temp = new List<Str>();
            if (!reader.ReadRow(row_temp))
                return null;
            int col_count = row_temp.Count;

            CsvTableHeader header = null;

            List<CsvTableRow> headerRows = new List<CsvTableRow>();
            List<CsvTableRow> dataRows = new List<CsvTableRow>();
            Str[] row_data_array = row_temp.ToArray();
            if (row_index < dataRowIndex)
                headerRows.Add(new CsvTableRow(row_data_array));
            else
                dataRows.Add(new CsvTableRow(row_data_array));

            if (row_index == headerNameRowIndex)
                header = new CsvTableHeader(row_data_array);


            for (; ; )
            {
                row_index++;
                row_temp.Clear();
                if (!reader.ReadRow(row_temp))
                    break;

                if (row_temp.Count != col_count)
                    continue;

                row_data_array = row_temp.ToArray();
                if (row_index < dataRowIndex)
                    headerRows.Add(new CsvTableRow(row_data_array));
                else
                    dataRows.Add(new CsvTableRow(row_data_array));

                if (row_index == headerNameRowIndex)
                    header = new CsvTableHeader(row_data_array);
            }

            if (header != null)
            {
                foreach (var p in headerRows)
                    p.SetHeader(header);
                foreach (var p in dataRows)
                    p.SetHeader(header);
            }

            return new CsvTableReader()
            {
                Header = header,
                DataRows = dataRows,
                HeadRows = headerRows,
            };
        }
    }
}
