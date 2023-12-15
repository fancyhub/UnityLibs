using System;
using System.Collections.Generic;

namespace FH.AssetBundleBuilder.Ed
{
    public class BundleRuler_Simple : BuilderBundleRuler
    {
        public string BundleName;
        public bool OnlyExportFile = true;
        public List<PatternSearch> PatternList = new List<PatternSearch>();

        public override string GetBundleName(string asset_path, EAssetObjType asset_type, bool need_export)
        {
            if (!need_export && OnlyExportFile)
                return null;

            if (!_IsMatch(asset_path))
                return null;
            return BundleName;
        }

        private bool _IsMatch(string file_path)
        {
            foreach (var a in PatternList)
            {
                if (a.IsMatch(file_path))                
                    return true;                
            }
            return false;
        }
    }

    /// <summary>
    /// 通配符搜索，支持 *
    /// 不支持 ?
    /// </summary>
    [Serializable]
    public class PatternSearch
    {
        private const char C_SEARCH = '*';
        public string Pattern = "*.*";
        public bool IgnoreCase = true;

        public PatternSearch(string search_parttern) : this(search_parttern, true)
        {
        }

        public PatternSearch(string search_parttern, bool ignore_case)
        {
            IgnoreCase = ignore_case;
            Pattern = search_parttern.Trim();
            if (ignore_case)
                Pattern = Pattern.ToLower();
        }

        public bool IsMatch(string target)
        {
            if (Pattern.Length == 0)
                return true;
            return _IsMatch(target, 0, Pattern, 0);
        }

        private bool _IsMatch(string s, int start_s, string p, int start_p)
        {
            bool is_s_empty = (s.Length - start_s) == 0;
            bool is_p_empty = (p.Length - start_p) == 0;
            if (is_p_empty) //如果p结束了，但是 s没有结束，说明没有全部匹配，返回false
                return is_s_empty;


            if (p[start_p] == C_SEARCH)
            {
                //s 不变，p向后移动，尝试匹配
                if (_IsMatch(s, start_s, p, start_p + 1))
                    return true;

                bool first_match = _IsFirstMatch(s, start_s, p, start_p);
                if (!first_match)
                    return false;

                //s向后移动，p不变，尝试匹配
                return _IsMatch(s, start_s + 1, p, start_p);
            }
            else
            {

                bool first_match = _IsFirstMatch(s, start_s, p, start_p);

                if (!first_match)
                    return false;

                return _IsMatch(s, start_s + 1, p, start_p + 1);
            }
        }

        //第一个字母是否相同
        private bool _IsFirstMatch(string s, int offset_s, string p, int offset_p)
        {
            if ((s.Length - offset_s) == 0)
                return false;

            if (p[offset_p] == C_SEARCH)
                return true;

            if (s[offset_s] == p[offset_p])
                return true;

            if (IgnoreCase && System.Char.ToLower(s[offset_s]) == p[offset_p])
                return true;

            return false;
        }
    }
}
