/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FH.UI.ViewGenerate.Ed
{
    public class EdUIView
    {
        public bool _ProcessedParentListFields = false;

        public string ParentViewName;
        public EdUIView ParentView; //继承的类
        public bool IsVariant = false;

        public EdUIViewDesc Desc;
        public GameObject Prefab;

        public List<EdUIField> Fields = new List<EdUIField>();
        public List<EdUIViewListField> ListFields = new List<EdUIViewListField>();



        public EdUIViewListField _FindListFieldInParent(string list_field_name)
        {
            EdUIView t = ParentView;
            for (; ; )
            {
                if (t == null)
                    return null;

                foreach (var p in t.ListFields)
                {
                    if (p._field_name == list_field_name)
                        return p;
                }

                t = t.ParentView;
            }
        }
    }
}