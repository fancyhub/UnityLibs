/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/21
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    /// <summary>
    /// 编译资源的时候
    /// 第0,1, 2位: 是从Unity获取的, 第2位可以是版本控制的提交号, 比如P4, 外部实现
    /// 第3位: 是时间 20231222103434  年月日时分秒    
    /// </summary>
    public struct VersionInfo
    {
        public static VersionInfo C_DEFAULT = new VersionInfo("0");
        public static VersionInfo C_INVALID = new VersionInfo(null);
        private bool _validate;
        private string _suffix;
        private ulong _v0;
        private ulong _v1;
        private ulong _v2;
        private ulong _v3;
        private ulong _v4;

        private static char[] S_SPLIT = new char[] { '.', '(', ')' };

        public VersionInfo(string name)
        {
            _v0 = 0;
            _v1 = 0;
            _v2 = 0;
            _v3 = 0;
            _v4 = 0;
            _validate = false;
            _suffix = null;

            if (string.IsNullOrEmpty(name))
                return;
            int index = name.IndexOf('_');
            string temp = name;
            if (index > 0)
            {
                _suffix = temp.Substring(name.LastIndexOf('_') + 1).Trim();
                temp = temp.Substring(0, index).Trim();
            }

            string[] tt = temp.Split(S_SPLIT);
            int len = tt.Length;

            if (len <= 0 || !ulong.TryParse(tt[0], out _v0))
                return;

            _validate = true;

            if (len <= 1 || !ulong.TryParse(tt[1], out _v1))
                return;

            if (len <= 2 || !ulong.TryParse(tt[2], out _v2))
                return;

            if (len <= 3 || !ulong.TryParse(tt[3], out _v3))
                return;

            if (len <= 4 || !ulong.TryParse(tt[4], out _v4))
                return;
        }

        public VersionInfo Clone()
        {
            VersionInfo ret = new VersionInfo(null);
            ret._v0 = _v0;
            ret._v1 = _v1;
            ret._v2 = _v2;
            ret._v3 = _v3;
            ret._v4 = _v4;
            ret._suffix = _suffix;
            ret._validate = _validate;
            return ret;
        }

        public static VersionInfo EdCreateResVersionInfo(string suffix = null)
        {
            VersionInfo info = new VersionInfo(UnityEngine.Application.version);
            info._v3 = ulong.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
            info._suffix = suffix;
            info._validate = true;
            return info;
        }

        /// <summary>
        /// 资源的版本号
        /// </summary>
        public ulong Res { get { return _v3; } set { _v3 = value; } }

        /// <summary>
        /// 后缀
        /// </summary>
        public string Suffix { get { return _suffix; } set { _suffix = value; } }

        /// <summary>
        /// 资源文件里面的版本号
        /// {0}.{1}.{2}.{3}_suffix?
        /// </summary>        
        public string ToResVersion(bool with_suffix = false)
        {
            if (with_suffix && !string.IsNullOrEmpty(_suffix))
                return string.Format("{0}.{1}.{2}.{3}._{4}", _v0, _v1, _v2, _v3, _suffix);
            return string.Format("{0}.{1}.{2}.{3}", _v0, _v1, _v2, _v3);
        }

        /// <summary>
        /// {0}.{1}.{2}_suffix?
        /// </summary>    
        public string ToAppVersion(bool with_suffix = false)
        {
            if (with_suffix && !string.IsNullOrEmpty(_suffix))
                return string.Format("{0}.{1}.{2}_{3}", _v0, _v1, _v2, _v3, _suffix);
            return string.Format("{0}.{1}.{2}", _v0, _v1, _v2, _v3);
        }

        /// <summary>
        /// 0.1.2.3 4位
        /// {0}.{1}.{2}
        /// 设置到unity setting上的
        /// </summary>
        public string ToUnityBuildVer()
        {
            return string.Format("{0}.{1}.{2}", _v0, _v1, _v2);
        }

        public bool Validate()
        {
            return _validate;
        }

        public override string ToString()
        {
            return ToResVersion(true);
        }

        public override bool Equals(object obj)
        {
            if (obj is VersionInfo version)
            {
                return version != null &&
                _v0 == version._v0 &&
                _v1 == version._v1 &&
                _v2 == version._v2 &&
                _v3 == version._v3 &&
                _v4 == version._v4;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_v0, _v1, _v2, _v3, _v4, _suffix);
        }

        public static bool operator ==(VersionInfo self, VersionInfo other)
        {
            VersionInfo a = other;
            VersionInfo b = self;
            if (a._v0 != b._v0)
                return false;

            if (a._v1 != b._v1)
                return false;

            if (a._v2 != b._v2)
                return false;

            if (a._v3 != b._v3)
                return false;

            if (a._v4 != b._v4)
                return false;
            //相等
            return true;
        }

        public static bool operator !=(VersionInfo self, VersionInfo other)
        {
            return !(self == other);
        }

        public static bool operator >(VersionInfo self, VersionInfo other)
        {
            VersionInfo a = self;
            VersionInfo b = other;
            if (a._v0 > b._v0)
                return true;
            else if (a._v0 < b._v0)
                return false;

            if (a._v1 > b._v1)
                return true;
            else if (a._v1 < b._v1)
                return false;

            if (a._v2 > b._v2)
                return true;
            else if (a._v2 < b._v2)
                return false;

            if (a._v3 > b._v3)
                return true;
            else if (a._v3 < b._v3)
                return false;

            if (a._v4 > b._v4)
                return true;
            else if (a._v4 < b._v4)
                return false;

            //相等
            return false;
        }

        public static bool operator <(VersionInfo self, VersionInfo other)
        {
            VersionInfo a = other;
            VersionInfo b = self;


            if (a._v0 > b._v0)
                return true;
            else if (a._v0 < b._v0)
                return false;

            if (a._v1 > b._v1)
                return true;
            else if (a._v1 < b._v1)
                return false;

            if (a._v2 > b._v2)
                return true;
            else if (a._v2 < b._v2)
                return false;

            if (a._v3 > b._v3)
                return true;
            else if (a._v3 < b._v3)
                return false;

            if (a._v4 > b._v4)
                return true;
            else if (a._v4 < b._v4)
                return false;

            //相等
            return false;
        }

        public static bool operator >=(VersionInfo self, VersionInfo other)
        {
            if (self < other)
                return false;
            return true;
        }

        public static bool operator <=(VersionInfo self, VersionInfo other)
        {
            if (self > other)
                return false;
            return true;
        }
    }
}