using System;
using System.Collections.Generic;
using System.IO;

namespace FH
{
    public interface ITableReaderCreator
    {
        public bool CreateTableReader(string sheet_name, string lang_name, out ITableReader reader);
        public void CloseReader();
    }

    public class TableReaderCsvTextCreator : ITableReaderCreator
    {
        private static TableReaderCsv _csv_reader;
        public string _base_dir;
        public TableReaderCsvTextCreator(string base_dir)
        {
            _base_dir = base_dir;
        }

        public bool CreateTableReader(string sheet_name, string lang_name, out ITableReader reader)
        {
            reader = null;
            byte[] buff = _LoadResCsv(_base_dir, sheet_name, lang_name);
            if (buff == null)            
                return false;
            
            if (_csv_reader == null)
                _csv_reader = new TableReaderCsv();
            _csv_reader.Reset(buff);
            reader = _csv_reader;
            return true;
        }
        public void CloseReader()
        {
            _csv_reader?.Reset(null);
        }

        private static byte[] _LoadResCsv(string dir, string sheet_name, string lang_name)
        {
            if (!string.IsNullOrEmpty(lang_name))
                sheet_name = sheet_name + "_" + lang_name;
            string file_path = System.IO.Path.Combine(dir, sheet_name + ".csv");
            string full_path = System.IO.Path.GetFullPath(file_path);

            if (!System.IO.File.Exists(full_path))
            {
                TableLog.E("找不到文件 {0}", full_path);
                return null;
            }

            using (System.IO.FileStream fs = System.IO.File.Open(full_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int numBytesToRead = Convert.ToInt32(fs.Length);
                var ret = new byte[(numBytesToRead)];
                fs.Read(ret, 0, numBytesToRead);

                return ret;
            }
        }
    }

    public class TableReaderCsvBinCreator : ITableReaderCreator
    {
        private TableReaderBin _bin_reader;
        public string _base_dir;
        public TableReaderCsvBinCreator(string base_dir)
        {
            _base_dir = base_dir;
        }

        public void CloseReader()
        {
            _bin_reader?.Reset(null);
        }
        public bool CreateTableReader(string sheet_name, string lang_name, out ITableReader reader)        
        {
            reader = null;
            if (_bin_reader != null && _bin_reader.CurLang == lang_name)
            {
                if (_bin_reader.Start(sheet_name))
                {
                    reader= _bin_reader;
                    return true;
                }
                return false;
            }

            byte[] buff = _LoadResBin(_base_dir, lang_name);
            if (buff == null)
                return false;

            if (_bin_reader == null)
                _bin_reader = new TableReaderBin();
            _bin_reader.CurLang = lang_name;
            _bin_reader.Reset(buff);


            if (_bin_reader.Start(sheet_name))
            {
                reader= _bin_reader;
                return true;
            }
            return false;
        }

        public static byte[] _LoadResBin(string dir, string lang_name)
        {
            string file_path = System.IO.Path.Combine(dir, "data.bin");
            if (!string.IsNullOrEmpty(lang_name))
                file_path = System.IO.Path.Combine(dir, $"data_{lang_name}.bin");


            string full_path = System.IO.Path.GetFullPath(file_path);

            if (!System.IO.File.Exists(full_path))
            {
                TableLog.E("找不到文件 {0}", full_path);
                return null;
            }
            return System.IO.File.ReadAllBytes(full_path);
        }
    }
}
