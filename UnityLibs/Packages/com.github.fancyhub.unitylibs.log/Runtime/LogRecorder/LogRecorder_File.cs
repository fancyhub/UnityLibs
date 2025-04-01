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
        private const string C_FILE_NAME = "applog.txt";
        private const string C_OLD_FILE_NAME = "applog_{0:yyyy_MM_dd_HH_mm_ss_fff}.txt";
        private const string C_FILE_PATTERN = "applog_*.txt";
        private const int C_OLD_FILE_MAX_COUNT = 20;

        private string _file_path;
        private string _dir_path;
        private StreamWriter _stream_writer;
#if UNITY_EDITOR
        private bool _succ;
#endif

        public LogRecorder_File(string log_dir_path)
        {
            _dir_path = log_dir_path;
        }

        public void Start()
        {
            //1.
            _file_path = System.IO.Path.Combine(_dir_path, C_FILE_NAME);

            //2. 移动旧的Log文件
            if (System.IO.File.Exists(_file_path))
            {
                DateTime modify_time = System.IO.File.GetLastWriteTime(_file_path);
                string dest_log_path = System.IO.Path.Combine(_dir_path, string.Format(C_OLD_FILE_NAME, modify_time));
                System.IO.File.Move(_file_path, dest_log_path);
            }

            //3. Create New Log File
            if (!System.IO.Directory.Exists(_dir_path))
            {
                Directory.CreateDirectory(_dir_path);
                //UnityEngine.Debug.LogError("Log Dir not exist " + System.IO.Path.GetFullPath(dir));
                //return;
            }

            //4. 删除旧的Log文件,控制总数量
            if (C_OLD_FILE_MAX_COUNT > 0)
            {
                try
                {
                    string[] files = System.IO.Directory.GetFiles(_dir_path, C_FILE_PATTERN, SearchOption.TopDirectoryOnly);
                    if (files.Length > C_OLD_FILE_MAX_COUNT)
                    {
                        System.Array.Sort(files);
                        int count_to_del = files.Length - C_OLD_FILE_MAX_COUNT;
                        for (int i = 0; i < count_to_del; i++)
                        {
                            System.IO.File.Delete(files[i]);
                        }
                    }
                }
                catch { }
            }

            var fileStream = new FileStream(_file_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _stream_writer = new StreamWriter(fileStream);

#if UNITY_EDITOR
            _succ = true;
            _stream_writer.Close();
            _stream_writer = null;
#endif
        }

        public static string GetLogFileDirPath()
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
            return dir;
        }

        public void Record(List<string> messages)
        {
#if UNITY_EDITOR
            if (_succ)
            {
                System.IO.File.AppendAllLines(_file_path, messages);                
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
