/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/09/05
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;


namespace FH
{
    public class LogRecorder_File : ILogRecorder
    {
        public const string C_FILE_NAME = "applog.txt";
        public const string C_OLD_FILE_NAME = "applog_{0:yyyy_MM_dd_HH_mm_ss_fff}.txt";
        private string _file_path;
        private StreamWriter _stream_writer;
        private bool _succ;

        public LogRecorder_File()
        {
            //1. Dir
            string dir = "Logs";
            switch (UnityEngine.Application.platform)
            {
                default:
                case UnityEngine.RuntimePlatform.WindowsPlayer:
                case UnityEngine.RuntimePlatform.OSXPlayer:
                case UnityEngine.RuntimePlatform.WindowsEditor:
                case UnityEngine.RuntimePlatform.OSXEditor:
                case UnityEngine.RuntimePlatform.LinuxEditor:
                    break;

                case UnityEngine.RuntimePlatform.IPhonePlayer:
                case UnityEngine.RuntimePlatform.Android:
                    dir = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, dir);
                    break;
            }

            //2. Move Old Log File
            _file_path = System.IO.Path.Combine(dir, C_FILE_NAME);
            if (System.IO.File.Exists(_file_path))
            {
                DateTime modify_time = System.IO.File.GetLastWriteTime(_file_path);
                string dest_log_path = System.IO.Path.Combine(dir, string.Format(C_OLD_FILE_NAME, modify_time));
                System.IO.File.Move(_file_path, dest_log_path);
            }

            //3. Create New Log File
            if (!System.IO.Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                //UnityEngine.Debug.LogError("Log Dir not exist " + System.IO.Path.GetFullPath(dir));
                //return;
            }

            var fileStream = new FileStream(_file_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _stream_writer = new StreamWriter(fileStream);
            _succ = true;

#if UNITY_EDITOR
            _stream_writer.Close();
            _stream_writer = null;
#endif
        }

        public void Record(List<string> messages)
        {
#if UNITY_EDITOR
            if (_succ)
            {
                foreach (var p in messages)
                {
                    System.IO.File.AppendAllLines(_file_path, messages);
                }
                return;
            }
#endif
            if (_stream_writer == null)
                return;
            foreach (var p in messages)
                _stream_writer.WriteLine(p);
            _stream_writer.Flush();
        }

        public void Dispose()
        {
            _stream_writer?.Dispose();
            _stream_writer = null;
        }
    }

}
