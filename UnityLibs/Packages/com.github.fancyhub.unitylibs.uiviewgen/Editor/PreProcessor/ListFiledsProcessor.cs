/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;

namespace FH.UI.ViewGenerate.Ed
{ 
    public class ListFiledsProcessor : IViewGeneratePreprocessor
    {
        private class ListFieldElemDesc
        {
            public EdUIField _field;
            public string _name;
            public int _index;
        }

        public void Process(EdUIViewGenContext context)
        {
            //1. 先创建
            foreach (var p in context.ViewList)
            {
                p.ListFields = _CreateListField(p);
            }

            //2. 把field list link起来
            // 因为可能 父类里面已经声明了该对象，需要link起来
            // 比如 UIViewA 还有 List<Lable> _lbl_list;
            // UIViewB: UIViewA, B里面也有一个 _lbl,需要添加父结构里面的 _lbl_list 里面
            foreach (EdUIView view in context.ViewList)
            {
                _PostProcessListFields(view, context.DBDesc);
            }
        }         

        private void _PostProcessListFields(EdUIView view, EdUIViewDescDB db)
        {
            if (view._ProcessedParentListFields)
                return;

            if (view.ParentView != null)
            {
                _PostProcessListFields(view.ParentView, db);
            }

            for (int i = view.ListFields.Count - 1; i >= 0; i--)
            {
                var field = view.ListFields[i];
                EdUIViewListField parent_list_field = view._FindListFieldInParent(field._field_name);
                if (!field.PostProcess(db, parent_list_field))
                {
                    view.ListFields.RemoveAt(i);
                }
            }
            view._ProcessedParentListFields = true;
        }


        private List<EdUIViewListField> _CreateListField(EdUIView ui_view)
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