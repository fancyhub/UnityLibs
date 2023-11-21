/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FH.UI.View.Gen.ED
{
    public class EdUIView
    {
        private bool _ProcessedParentListFields = false;

        public EdUIView ParentView; //继承的类
        public EdUIViewConf Conf;
        public GameObject Prefab;
        public string ParentClass;
        public bool IsVariant = false;

        public List<EdUIField> Fields = new List<EdUIField>();
        public List<EdUIViewListField> ListFields = new List<EdUIViewListField>();

        public void PostProcessListFields(EdUIViewConfDb db)
        {
            if (_ProcessedParentListFields)
                return;

            ParentView?.PostProcessListFields(db);

            for (int i = ListFields.Count - 1; i >= 0; i--)
            {
                var field = ListFields[i];
                EdUIViewListField parent_list_field = _FindListFieldInParent(field._field_name);
                if (!field.PostProcess(db, parent_list_field))
                {
                    ListFields.RemoveAt(i);
                }
            }
            _ProcessedParentListFields = true;
        }


        private EdUIViewListField _FindListFieldInParent(string list_field_name)
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