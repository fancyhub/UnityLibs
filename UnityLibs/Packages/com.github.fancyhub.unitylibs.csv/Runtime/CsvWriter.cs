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
    public class CsvWriter : IDisposable
    {
        private static System.Text.Encoding UtfEncodingWithBom = new System.Text.UTF8Encoding(true);
        private static System.Text.Encoding UtfEncodingNoBom = new System.Text.UTF8Encoding(false);

        public TextWriter _writer;
        public bool _first = true;
        public CsvWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public CsvWriter(string file_path, bool withBom)
        {
            if (withBom)
                _writer = new StreamWriter(file_path, false, UtfEncodingWithBom);
            else
                _writer = new StreamWriter(file_path, false, UtfEncodingNoBom);
        }

        public void WriteWord(string word)
        {
            if (!_first)
                _writer.Write(',');
            _writer.Write(FormatCsvStr(word));
            _first = false;
        }

        public void WriteWord(params string[] words)
        {
            foreach (var p in words)
                WriteWord(p);
        }

        public void WriteWord<T>(IList<T> words, bool end_line)
        {
            foreach (var p in words)
                WriteWord(p.ToString());
            if (end_line)
                WriteLine();
        }

        public void WriteWord(IEnumerable e, bool end_line)
        {
            foreach (var p in e)
            {
                WriteWord(p.ToString());
            }
            if (end_line)
                WriteLine();
        }

        public void WriteWord<T>(IEnumerable<T> e, bool end_line)
        {
            foreach (var p in e)
            {
                WriteWord(p.ToString());
            }
            if (end_line)
                WriteLine();
        }

        public void WriteLine()
        {
            _writer.WriteLine();
            _first = true;
        }

        public void Close()
        {
            _writer?.Close();
            _writer = null;
        }

        public string FormatCsvStr(string s)
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
