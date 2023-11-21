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
    //用来format用的，和 string.format 很像，不过 里面的内容不是 {0}, 而是具体的key {abc}
    //_key_value_dict 里面要填写  abc=real_name
    public class EdStrFormatter
    {
        public Dictionary<string, string> _key_value_dict =
            new Dictionary<string, string>();

        public string this[string key]
        {
            set
            {
                _key_value_dict[key] = value;
            }
        }

        public EdStrFormatter Add(string key, string value)
        {
            this[key] = value;
            return this;
        }

        public string Format(string format)
        {
            string ret = format;
            foreach (var a in _key_value_dict)
            {
                ret = ret.Replace("{" + a.Key + "}", a.Value);
            }
            return ret;
        }
    }
}
