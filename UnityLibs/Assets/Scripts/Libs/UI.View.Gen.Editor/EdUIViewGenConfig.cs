
/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/25 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.View.Gen.ED
{
    public static class EdUIViewGenConfig
    {
        public const string C_MENU_Gen_Select = "Assets/Gen UIView Code";
        public const string C_MENU_Gen_ALL = "Tools/UI/Regen All UIView Code";
        public const string C_MENU_Clear_Unused_Class = "Tools/UI/Clear UIView Code";
        public const string C_MENU_Export_Class_Usage = "Tools/UI/Gen Class Usage";

        public const string C_NAME_SPACE = "FH.UI";
        public const string C_CLASS_PREFIX = "UI"; //自动生成的 class 前缀        
        public const string C_CLASS_SUFFIX = "View";

        public const string C_Base_Class = "FH.UI.UIBaseView";
        public const string C_CS_FOLDER = "Assets/Scripts/UI/View";

        public static List<string> S_WHITE_FOLDER_LIST = new List<string>()
        {
            "Assets/Resources/UI/Prefab"
        };

        public static List<string> S_BLACK_FOLDER_LIST = new List<string>()
        {
            "Assets/Res/UI/text_style/"
        };

        public static bool IsPrefabPathValid(string path)
        {
            foreach (var key_word in S_BLACK_FOLDER_LIST)
            {
                if (path.Contains(key_word))
                    return false;
            }

            foreach (var key_word in S_WHITE_FOLDER_LIST)
            {
                if (!path.Contains(key_word))
                    return false;
            }
            return true;
        }
    }
}
