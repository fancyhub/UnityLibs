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
        private HashSet<string> _ProcessedViews = new HashSet<string>();

        public void Process(EdUIViewGenContext context)
        {
            //1. 先创建
            foreach (var view in context.ViewList)
            {
                List<EdUIViewListField> list_field = new List<EdUIViewListField>();
                foreach (EdUIField field in view.Fields)
                {
                    //1. 先根据名字来做分组
                    if (!_TryParseField(field, out string field_name, out var field_desc))
                        continue;

                    //2. 添加
                    _AddFieldList(list_field, field_name, field_desc);
                }

                foreach (var e in list_field)
                {
                    e.Sort();
                }
                view.ListFields = list_field;
            }

            _ProcessedViews.Clear();
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
            if (_ProcessedViews.Contains(view.Desc.PrefabPath))
                return;

            if (view.ParentView != null)
                _PostProcessListFields(view.ParentView, db);

            for (int i = view.ListFields.Count - 1; i >= 0; i--)
            {
                EdUIViewListField field = view.ListFields[i];
                EdUIViewListField parent_field = _FindFieldInParent(view, field.FieldName);
                if (parent_field == null)
                {
                    _PostProcessSelf(db, field);
                }
                else
                {
                    _MergeToParent(db, field, parent_field);
                }

                if(field.FieldList.Count==0)
                {
                    view.ListFields.RemoveAt(i);
                }                
            }
            _ProcessedViews.Add(view.Desc.PrefabPath);
        }

        private void _PostProcessSelf(EdUIViewDescDB db, EdUIViewListField self)
        {
            self.FieldType = self.FieldList[0].FieldType.Clone();

            switch (self.FieldType.Type)
            {
                case EdUIFieldType.EType.Component://不找父类, 必须相同
                    {
                        for (int i = self.FieldList.Count - 1; i > 0; i--)
                        {
                            if (!self.FieldList[i].FieldType.Equals(self.FieldType))
                            {
                                self.FieldList.RemoveAt(i);
                            }
                        }
                    }
                    break;

                case EdUIFieldType.EType.SubView:
                    {
                        for (int i = self.FieldList.Count - 1; i > 0; i--)
                        {
                            var field_ref = self.FieldList[i];
                            if (field_ref.FieldType.Type != EdUIFieldType.EType.SubView)
                            {
                                self.FieldList.RemoveAt(i);
                                continue;
                            }
                            self.FieldType.ViewType = db.GetCommonBase(self.FieldType.ViewType, field_ref.FieldType.ViewType);
                        }
                    }
                    break;
            }
        }


        public void _MergeToParent(EdUIViewDescDB db_desc, EdUIViewListField self, EdUIViewListField parent_list_field)
        {
            self.FieldType = parent_list_field.FieldType.Clone();

            switch (self.FieldType.Type)
            {
                case EdUIFieldType.EType.Component:
                    {
                        for (int i = self.FieldList.Count - 1; i >= 0; i--)
                        {
                            if (!self.FieldList[i].FieldType.Equals(self.FieldType))
                            {
                                self.FieldList.RemoveAt(i);
                            }
                        }
                    }
                    break;

                case EdUIFieldType.EType.SubView:
                    {
                        for (int i = self.FieldList.Count - 1; i >= 0; i--)
                        {
                            var field_ref = self.FieldList[i];
                            if (field_ref.FieldType.Equals(self.FieldType)) //完全相同
                                continue;

                            if (field_ref.FieldType.Type != EdUIFieldType.EType.SubView)
                            {
                                self.FieldList.RemoveAt(i);
                                continue;
                            }

                            if (!db_desc.IsParentClass(field_ref.FieldType.ViewType, self.FieldType.ViewType))
                            {
                                self.FieldList.RemoveAt(i);
                                continue;
                            }
                        }
                    }
                    break;

                default:
                    //Error
                    self.FieldList.Clear();
                    break;
            }

            //自己会被清除掉
            if (self.FieldList.Count == 0)
            {
                self.FieldType = null;
                return;
            }

            parent_list_field.HasChild = true;
            self.Parent = parent_list_field;
            return;
        }


        public static EdUIViewListField _FindFieldInParent(EdUIView view, string list_field_name)
        {
            EdUIView t = view.ParentView;
            for (; ; )
            {
                if (t == null)
                    return null;

                foreach (var p in t.ListFields)
                {
                    if (p.FieldName == list_field_name)
                        return p;
                }
                t = t.ParentView;
            }
        }


        private static bool _TryParseField(EdUIField field, out string field_name, out EdUIViewListField.FieldRef field_ref)
        {
            field_ref = default;
            field_name = field.Fieldname;
            int last_index = field_name.LastIndexOf('_');
            //如果是第一个，不算，如果没有也不算
            if (last_index <= 0)
                return false;

            string str_num = field_name.Substring(last_index + 1);
            int index = 0;
            if (!int.TryParse(str_num, out index)) //如果不是数字结尾的不算
                return false;

            field_name = field_name.Substring(0, last_index);

            field_ref = new EdUIViewListField.FieldRef(field, index);
            return true;
        }

        private static void _AddFieldList(List<EdUIViewListField> list_field_list, string field_name, EdUIViewListField.FieldRef field_desc)
        {
            //1. 找到现存的
            foreach (EdUIViewListField list_field in list_field_list)
            {
                if (list_field.FieldName == field_name)
                {
                    list_field.FieldList.Add(field_desc);
                    return;
                }
            }

            //2. 没有找到,就创建一个
            EdUIViewListField new_list_field = new EdUIViewListField(field_name);
            new_list_field.FieldList.Add(field_desc);
            list_field_list.Add(new_list_field);
        }
    }
}