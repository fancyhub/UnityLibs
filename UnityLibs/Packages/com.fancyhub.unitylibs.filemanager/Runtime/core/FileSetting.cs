/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/

using FH.FileManagement;
using UnityEngine;

namespace FH
{
    public static class FileSetting
    {
        public const string CRelativeFileFullNameExt = "._fullname";
        internal const string CBaseResVersionFileName = "base_res_version";

        private static string _PersistentDir;
        public static string PersistentDir
        {
            get
            {
                if (_PersistentDir != null)
                    return _PersistentDir;

                if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                    _PersistentDir = System.IO.Path.Combine("ProjTemp/Persistent", Application.platform.Ext2DirName());
                else
                    _PersistentDir = Application.persistentDataPath;

                _PersistentDir = System.IO.Path.GetFullPath(_PersistentDir);
                _PersistentDir = _PersistentDir.Replace("\\", "/");
                if (!_PersistentDir.EndsWith("/"))
                    _PersistentDir += "/";
                return _PersistentDir;
            }
        }


        private static string _LocalDir;
        public static string LocalDir
        {
            get
            {
                if (_LocalDir != null)
                    return _LocalDir;
                _LocalDir = PersistentDir + "Files/";
                FileUtil.CreateDir(_LocalDir);
                return _LocalDir;
            }
        }

        private static string _LocalRelativeFileDir;
        public static string LocalRelativeFileDir
        {
            get
            {
                if (_LocalRelativeFileDir != null)
                    return _LocalRelativeFileDir;
                _LocalRelativeFileDir = LocalDir + "Raw/";
                FileUtil.CreateDir(_LocalRelativeFileDir);
                return _LocalRelativeFileDir;
            }
        }


        private static string _DownloadDir;
        public static string DownloadDir
        {
            get
            {
                if (_DownloadDir != null)
                    return _DownloadDir;
                _DownloadDir = PersistentDir + "Download/";
                FileUtil.CreateDir(_DownloadDir);
                return _DownloadDir;
            }
        }

        private static string _StreamingAssetsDir;
        /// <summary>
        /// [Win|Android|IOS|OSX]/
        /// </summary>
        public static string StreamingAssetsDir
        {
            get
            {
                if (_StreamingAssetsDir == null)
                {
                    _StreamingAssetsDir = System.IO.Path.Combine(Application.streamingAssetsPath, "Files", Application.platform.Ext2DirName());
                    _StreamingAssetsDir = _StreamingAssetsDir.Replace("\\", "/");
                    _StreamingAssetsDir += "/";
                }
                return _StreamingAssetsDir;
            }
        }

        private static string _StreamingAssetsRelativeFileDir;
        /// <summary>
        /// [Win|Android|IOS|OSX]/
        /// </summary>
        public static string StreamingAssetsRelativeFileDir
        {
            get
            {
                if (_StreamingAssetsRelativeFileDir == null)
                {
                    _StreamingAssetsRelativeFileDir = StreamingAssetsDir + "Raw/";
                }
                return _StreamingAssetsRelativeFileDir;
            }
        }
    }
}