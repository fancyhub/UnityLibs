/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace FH
{
    public sealed class CsvWriter : IDisposable
    {
        private static System.Text.Encoding UtfEncodingWithBom = new System.Text.UTF8Encoding(true);
        private static System.Text.Encoding UtfEncodingNoBom = new System.Text.UTF8Encoding(false);

        private TextWriter _Writer;
        private bool _EmptyRow = true;
        private bool _LeaveOpen = false;
        public CsvWriter(TextWriter writer, bool leave_open = false)
        {
            _Writer = writer;
            _LeaveOpen = leave_open;
        }

        public CsvWriter(string file_path, bool withBom)
        {
            if (withBom)
                _Writer = new StreamWriter(file_path, false, UtfEncodingWithBom);
            else
                _Writer = new StreamWriter(file_path, false, UtfEncodingNoBom);
            _LeaveOpen = false;
        }

        public void WriteWord(string word)
        {
            if (!_EmptyRow)
                _Writer.Write(',');
            _Writer.Write(FormatCsvStr(word));
            _EmptyRow = false;
        }

        public void WriteWords(params string[] words)
        {
            foreach (var p in words)
                WriteWord(p);
        }

        public void WriteWords<T>(IList<T> words, bool end_new_row)
        {
            foreach (var p in words)
                WriteWord(p.ToString());
            if (end_new_row)
                NextRow();
        }

        public void WriteWords(IEnumerable e, bool end_new_row)
        {
            foreach (var p in e)
            {
                WriteWord(p.ToString());
            }
            if (end_new_row)
                NextRow();
        }

        public void WriteWords<T>(IEnumerable<T> e, bool end_new_row)
        {
            foreach (var p in e)
            {
                WriteWord(p.ToString());
            }
            if (end_new_row)
                NextRow();
        }

        public void NextRow()
        {
            //不能是空行
            if (_EmptyRow)
                return;
            _Writer.WriteLine();
            _EmptyRow = true;
        }

        public void Close()
        {
            if (_LeaveOpen)
            {
                _Writer = null;
                return;
            }

            var t = _Writer;
            _Writer = null;
            t?.Close();
        }

        public static string FormatCsvStr(string s)
        {
            if (s == null)
                return string.Empty;

            bool contain_qutos = s.Contains('\"');
            if (!contain_qutos && !s.Contains('\n') && !s.Contains('\r') && !s.Contains(','))
                return s;

            if (contain_qutos)
                s = s.Replace("\"", "\"\"");

            return string.Concat("\"", s, "\"");
        }

        public void Dispose()
        {
            Close();
        }
    }
}
