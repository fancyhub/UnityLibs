/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    [CreateAssetMenu(fileName = "LocTextStyle", menuName = "fancyhub/LocTextStyle")]
    public class LocTextStyleAsset : ScriptableObject
    {
        [Serializable]
        public class TextStyle
        {
            public string Lang;
            public AssetPath<Font> Font;
            public FontStyle FontStyle = FontStyle.Normal;
            public int FontSize = 20;
            public float LineSpace = 1;
        }
        public TextStyle[] StyleList;
        public TextStyle Find(string lang)
        {
            if (StyleList == null)
                return null;
            foreach (var p in StyleList)
            {
                if (p.Lang == lang)
                {
                    return p;
                }
            }
            return null;
        }

#if UNITY_EDITOR
        public void EdCreateAll()
        {
            var lang_list = LangSettingAsset.EdGetLangIdList();
            List<TextStyle> temp = new List<TextStyle>(lang_list.Count);


            foreach (var p in lang_list)
            {
                TextStyle item = Find(p.Lang);
                if (item == null)
                {
                    item = new TextStyle()
                    {
                        Lang = p.Lang,
                    };
                }
                temp.Add(item);
            }
            StyleList = temp.ToArray();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
