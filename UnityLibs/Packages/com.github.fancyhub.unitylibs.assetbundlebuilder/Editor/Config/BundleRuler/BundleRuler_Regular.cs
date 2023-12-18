/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace FH.AssetBundleBuilder.Ed
{
    public class BundleRuler_Regular : BuilderBundleRuler, ISerializationCallbackReceiver
    {
        public bool OnlyExportFile = true;
        public string RegularString;
        public string BundleNameFormater;
        [Range(0, 3)]
        public int BundleNameParamCount = 1;

        private Regex _Reg;
        public List<PatternSearch> PatternList = new List<PatternSearch>();

        public override string GetBundleName(string asset_path, EAssetObjType asset_type, bool need_export)
        {
            if (!need_export && OnlyExportFile)
                return null;

            if (_Reg == null)
                return null;

            if (!_IsMatch(asset_path))
                return null;

            Match match_result = _Reg.Match(asset_path);
            GroupCollection groups = match_result.Groups;
            if (groups.Count < (BundleNameParamCount + 1))
            {
                string msg = string.Format("Rule: {0} \n Reg: {1} \nFilePath: {2}\nFormater: {3}\n取词数量: {4}\n"
                    , RulerName
                    , RegularString
                    , asset_path
                    , BundleNameFormater
                    , groups.Count - 1);

                for(int i=0;i<groups.Count; i++)
                {
                    msg += $"\t第{i}: {groups[i].Value}\n";
                }

                BuilderLog.Error(msg);
                throw new Exception(msg);
            }


            switch (BundleNameParamCount)
            {
                default:
                    return null;
                case 0:
                    return BundleNameFormater;

                case 1:
                    return string.Format(BundleNameFormater, groups[1].Value);

                case 2:
                    return string.Format(BundleNameFormater, groups[1].Value, groups[2].Value);

                case 3:
                    return string.Format(BundleNameFormater, groups[1].Value, groups[2].Value, groups[3].Value);
            }
        }

        private bool _IsMatch(string file_path)
        {
            if (PatternList.Count == 0)
                return true;

            foreach (var a in PatternList)
            {
                if (a.IsMatch(file_path))
                    return true;
            }

            return false;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            try
            {
                _Reg = new Regex(RegularString);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.Message);
            }
        }
    }
}
