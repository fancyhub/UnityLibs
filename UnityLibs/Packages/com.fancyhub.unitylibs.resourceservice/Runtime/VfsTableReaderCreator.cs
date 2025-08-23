/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;

namespace FH
{
    public class VfsTableReaderCsvCreator : ITableReaderCreator
    {
        private TableReaderCsv _csv_reader;
        public string _base_dir;
        public VfsTableReaderCsvCreator(string base_dir)
        {
            _base_dir = base_dir.Replace("\\", "/");
            if (!_base_dir.EndsWith("/"))
                _base_dir += "/";
        }

        public bool CreateTableReader(string sheet_name, string lang_name, out ITableReader reader)
        {
            reader = null;
            byte[] buff = _LoadResCsv(sheet_name, lang_name);
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

        private byte[] _LoadResCsv(string sheet_name, string lang_name)
        {
            if (!string.IsNullOrEmpty(lang_name))
                sheet_name = sheet_name + "_" + lang_name;
            string file_path = _base_dir + sheet_name + ".csv";

            return VfsMgr.ReadAllBytes(file_path);
        }
    }

    public class VfsTableReaderBinCreator : ITableReaderCreator
    {
        private TableReaderBin _bin_reader;
        public string _base_dir;
        public VfsTableReaderBinCreator(string base_dir)
        {
            _base_dir = base_dir.Replace("\\", "/");
            if (!_base_dir.EndsWith("/"))
                _base_dir += "/";
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
                    reader = _bin_reader;
                    return true;
                }
                return false;
            }

            byte[] buff = _LoadResBin(lang_name);
            if (buff == null)
                return false;

            if (_bin_reader == null)
                _bin_reader = new TableReaderBin();
            _bin_reader.CurLang = lang_name;
            _bin_reader.Reset(buff);


            if (_bin_reader.Start(sheet_name))
            {
                reader = _bin_reader;
                return true;
            }
            return false;
        }

        public byte[] _LoadResBin(string lang_name)
        {
            string file_path = null;
            if (!string.IsNullOrEmpty(lang_name))
                file_path = _base_dir + $"data_{lang_name}.bin";
            else
                file_path = _base_dir + "data.bin";

            return VfsMgr.ReadAllBytes(file_path);
        }
    }
}
