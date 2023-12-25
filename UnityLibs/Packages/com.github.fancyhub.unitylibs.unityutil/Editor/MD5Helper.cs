/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH.Ed
{
    public static class MD5Helper
    {
        private static System.Security.Cryptography.MD5 _Md5 = System.Security.Cryptography.MD5.Create();
        private static System.Text.StringBuilder _StringBuilder = new System.Text.StringBuilder();

        public static string ComputeFile(string file_path)
        {
            using var fs = System.IO.File.OpenRead(file_path);

            byte[] hash = _Md5.ComputeHash(fs);

            _StringBuilder.Clear();
            foreach (var b in hash)
                _StringBuilder.Append(b.ToString("x2"));

            return _StringBuilder.ToString();
        }

        public static string ComputeString(string file_path)
        {
            using var fs = System.IO.File.OpenRead(file_path);


            byte[] hash = _Md5.ComputeHash(fs);

            _StringBuilder.Clear();
            foreach (var b in hash)
                _StringBuilder.Append(b.ToString("x2"));

            return _StringBuilder.ToString();
        }
    }
}
