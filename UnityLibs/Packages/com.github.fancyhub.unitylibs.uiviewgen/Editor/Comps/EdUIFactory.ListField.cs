/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FH.UI.View.Gen.ED
{
    public partial class EdUIFactory
    {
        public List<EdUIViewListField> CreateListField(EdUIView ui_view)
        {
            List<EdUIViewListField> ret = new List<EdUIViewListField>();
            foreach (EdUIField field in ui_view.Fields)
            {
                //1. 先根据名字来做分组
                var field_desc = _CreateFieldDesc(field);
                if (field_desc == null)
                    continue;

                //2. 添加
                _AddFieldList(ret, field_desc);
            }


            foreach (var e in ret)
            {
                e.Sort();
            }

            return ret;
        }

        private class ListFieldElemDesc
        {
            public EdUIField _field;
            public string _name;
            public int _index;
        }

        private static ListFieldElemDesc _CreateFieldDesc(EdUIField field)
        {
            string field_name = field.Fieldname;

            int last_index = field_name.LastIndexOf('_');
            //如果是第一个，不算，如果没有也不算
            if (last_index <= 0)
                return null;

            string str_num = field_name.Substring(last_index + 1);
            int i = 0;
            if (!int.TryParse(str_num, out i)) //如果不是数字结尾的不算
                return null;

            return new ListFieldElemDesc()
            {
                _field = field,
                _name = field_name.Substring(0, last_index),
                _index = i
            };
        }

        private static void _AddFieldList(List<EdUIViewListField> list_field_list, ListFieldElemDesc field_desc)
        {
            //1. 找到现存的
            foreach (EdUIViewListField list_field in list_field_list)
            {
                if (list_field._field_name == field_desc._name)
                {
                    list_field._field_list.Add(field_desc._field);
                    return;
                }
            }

            //2. 没有找到,就创建一个
            EdUIViewListField new_list_field = new EdUIViewListField(field_desc._name);
            new_list_field._field_list.Add(field_desc._field);
            list_field_list.Add(new_list_field);
        }
    }

}
