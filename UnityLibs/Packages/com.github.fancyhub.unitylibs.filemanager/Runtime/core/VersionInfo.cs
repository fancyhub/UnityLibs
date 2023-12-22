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
    /// 格式 1.0.0.0.0_buildxxx
    /// </summary>
    public struct VersionInfo
    {
        public static VersionInfo C_DEFAULT = new VersionInfo("0");
        public static VersionInfo C_INVALID = new VersionInfo(null);
        private bool _validate;
        private string _suffix;
        private ulong _v1;
        private ulong _v2;
        private ulong _v3;
        private ulong _v4;
        private ulong _v5;

        private static char[] S_SPLIT = new char[] { '.', '(', ')' };

        public VersionInfo(string name)
        {
            _v1 = 0;
            _v2 = 0;
            _v3 = 0;
            _v4 = 0;
            _v5 = 0;
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

            if (len <= 0 || !ulong.TryParse(tt[0], out _v1))
                return;

            _validate = true;

            if (len <= 1 || !ulong.TryParse(tt[1], out _v2))
                return;

            if (len <= 2 || !ulong.TryParse(tt[2], out _v3))
                return;

            if (len <= 3 || !ulong.TryParse(tt[3], out _v4))
                return;

            if (len <= 4 || !ulong.TryParse(tt[4], out _v5))
                return;
        }

        public VersionInfo Clone()
        {
            VersionInfo ret = new VersionInfo(null);
            ret._v1 = _v1;
            ret._v2 = _v2;
            ret._v3 = _v3;
            ret._v4 = _v4;
            ret._v5 = _v5;
            ret._suffix = _suffix;
            ret._validate = _validate;
            return ret;
        }

        /// <summary>
        /// 主版本号
        /// </summary>
        public ulong Major { get { return _v1; } set { _v1 = value; } }

        /// <summary>
        /// 次版本号
        /// </summary>
        public ulong Minor { get { return _v2; } set { _v2 = value; } }

        /// <summary>
        /// build版本号
        /// </summary>
        public ulong Build { get { return _v3; } set { _v3 = value; } }

        /// <summary>
        /// build版本号
        /// </summary>
        public ulong Res { get { return _v4; } set { _v4 = value; } }

        /// <summary>
        /// 修订版本号
        /// </summary>
        public ulong Revision { get { return _v5; } set { _v5 = value; } }

        /// <summary>
        /// 后缀
        /// </summary>
        public string Suffix { get { return _suffix; } set { _suffix = value; } }

        /// <summary>
        /// 资源文件里面的版本号
        /// {1}.{2}.{3}.{4}.{5}_suffix?
        /// </summary>        
        public string ToResVersion(bool with_suffix = false)
        {
            if (with_suffix && !string.IsNullOrEmpty(_suffix))
                return string.Format("{0}.{1}.{2}.{3}.{4}_", _v1, _v2, _v3, _v4, _v5, _suffix);
            return string.Format("{0}.{1}.{2}.{3}.{4}", _v1, _v2, _v3, _v4, _v5);
        }

        /// <summary>
        /// {1}.{2}.{3}.{4}_suffix?
        /// </summary>    
        public string ToAppVersion(bool with_suffix = false)
        {
            if (with_suffix && !string.IsNullOrEmpty(_suffix))
                return string.Format("{0}.{1}.{2}.{3}_", _v1, _v2, _v3, _v4, _suffix);
            return string.Format("{0}.{1}.{2}.{3}", _v1, _v2, _v3, _v4);
        }

        /// <summary>
        /// 1.2.3.4 4位
        /// {1}.{2}.{3}.0
        /// 设置到unity setting上的
        /// </summary>
        public string ToUnityBuildVer()
        {
            return string.Format("{0}.{1}.{2}.0", _v1, _v2, _v3);
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
                _v1 == version._v1 &&
                _v2 == version._v2 &&
                _v3 == version._v3 &&
                _v4 == version._v4 &&
                _v5 == version._v5;
            }
            else
                return false;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(_v1, _v2, _v3, _v4, _v5, _suffix);
        }

        public static bool operator ==(VersionInfo self, VersionInfo other)
        {
            VersionInfo a = other;
            VersionInfo b = self;
            if (a._v1 != b._v1)
                return false;

            if (a._v2 != b._v2)
                return false;

            if (a._v3 != b._v3)
                return false;

            if (a._v4 != b._v4)
                return false;

            if (a._v5 != b._v5)
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

            if (a._v5 > b._v5)
                return true;
            else if (a._v5 < b._v5)
                return false;            

            //相等
            return false;
        }

        public static bool operator <(VersionInfo self, VersionInfo other)
        {
            VersionInfo a = other;
            VersionInfo b = self;
           

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

            if (a._v5 > b._v5)
                return true;
            else if (a._v5 < b._v5)
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