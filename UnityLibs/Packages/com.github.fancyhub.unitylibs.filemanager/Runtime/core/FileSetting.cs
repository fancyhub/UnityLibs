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
    public enum EFilePlatform
    {
        None,
        Android,
        IOS,
        Win,
        OSX,
    }

    public static class FileSetting
    {
        #region Platform
        private static EFilePlatform _Platform = EFilePlatform.None;
        public static EFilePlatform Platform
        {
            get
            {
                if (_Platform != EFilePlatform.None)
                    return _Platform;

                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        _Platform = EFilePlatform.Android;
                        break;
                    case RuntimePlatform.IPhonePlayer:
                        _Platform = EFilePlatform.IOS;
                        break;

                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.WindowsEditor:
                        _Platform = EFilePlatform.Win;
                        break;
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.OSXEditor:
                        _Platform = EFilePlatform.OSX;
                        break;

                    default:
                        FileLog._.E("未知类型 {0}", Application.platform);
                        _Platform = EFilePlatform.Win;
                        break;
                }

#if UNITY_EDITOR
                if (Application.isEditor)
                {
                    switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
                    {
                        case UnityEditor.BuildTarget.iOS:
                            _Platform = EFilePlatform.IOS;
                            break;
                        case UnityEditor.BuildTarget.Android:
                            _Platform = EFilePlatform.Android;
                            break;
                        case UnityEditor.BuildTarget.StandaloneWindows:
                        case UnityEditor.BuildTarget.StandaloneWindows64:
                            _Platform = EFilePlatform.Win;
                            break;
                        case UnityEditor.BuildTarget.StandaloneOSX:
                            _Platform = EFilePlatform.OSX;
                            break;
                        default:
                            FileLog._.E("未知类型 {0}", UnityEditor.EditorUserBuildSettings.activeBuildTarget);
                            _Platform = EFilePlatform.Win;
                            break;
                    }
                }
#endif
                return _Platform;
            }
        }
        #endregion

                
        private static string _CacheDir;
        public static string CacheDir
        {
            get
            {
                if (_CacheDir != null)
                    return _CacheDir;

                if (Application.isEditor || Platform == EFilePlatform.Win)
                    _CacheDir = System.IO.Path.Combine("Bundle/Cache", Platform.ToString());
                else
                    _CacheDir = System.IO.Path.Combine(Application.persistentDataPath, "Files");

                _CacheDir = _CacheDir.Replace("\\", "/");
                if (!_CacheDir.EndsWith("/"))
                    _CacheDir += "/";

                FileUtil.CreateDir(_CacheDir);
                return _CacheDir;
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
                    _StreamingAssetsDir = System.IO.Path.Combine(Application.streamingAssetsPath, Platform.ToString());
                    _StreamingAssetsDir = _StreamingAssetsDir.Replace("\\", "/");
                    _StreamingAssetsDir += "/";
                }
                return _StreamingAssetsDir;
            }
        }


        public const string ManifestName = "file_manifest.json";
        public const string ManifestUpgradeName = "file_manifest_upgrade.json";
    }
}