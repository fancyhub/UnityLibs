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

namespace FH.UI.ViewGenerate.Ed
{
    public class EdUIViewListField
    {
        /// <summary>
        /// list 对象的名字
        /// </summary>
        public string _field_name;
        public List<EdUIField> _field_list = new List<EdUIField>();


        /// <summary>
        /// 如果 该 父结构里面也有相同的 field，该对象就不会是空的
        /// </summary>
        public EdUIViewListField _parent;
        public bool _has_child; //是否有子结构       
        public EdUIFieldType _field_type;

        public EdUIViewListField(string name)
        {
            _field_name = name;
        }

        public void Sort()
        {
            _field_list.Sort((a, b) =>
            {
                return a.Fieldname.CompareTo(b.Fieldname);
            });
        }

        public bool PostProcess(EdUIViewDescDB db_desc, EdUIViewListField parent_list_field)
        {
            //1. 如果没有父结构, 直接处理自己
            if (parent_list_field == null)
            {
                _PostProcessSelf(db_desc);
                return true;
            }

            _field_type = parent_list_field._field_type.Clone();

            switch (_field_type.Type)
            {
                case EdUIFieldType.EType.Component:
                    {
                        for (int i = _field_list.Count - 1; i >= 0; i--)
                        {
                            if (!_field_list[i].FieldType.Equals(_field_type))
                            {
                                _field_list.RemoveAt(i);
                            }
                        }
                    }
                    break;

                case EdUIFieldType.EType.SubView:
                    {
                        for (int i = _field_list.Count - 1; i >= 0; i--)
                        {
                            var field = _field_list[i];
                            if (field.FieldType.Equals(_field_type)) //完全相同
                                continue;

                            if (field.FieldType.Type != EdUIFieldType.EType.SubView)
                            {
                                _field_list.RemoveAt(i);
                                continue;
                            }

                            if (!db_desc.IsParentClass(field.FieldType.ViewType, _field_type.ViewType))
                            {
                                _field_list.RemoveAt(i);
                                continue;
                            }
                        }
                    }
                    break;
                default:
                    _field_list.Clear();
                    break;
            }

            //自己会被清除掉
            if (_field_list.Count == 0)
            {
                _field_type = null;
                return false;
            }

            parent_list_field._has_child = true;
            _parent = parent_list_field;
            return true;
        }


        private void _PostProcessSelf(EdUIViewDescDB config)
        {
            _field_type = _field_list[0].FieldType.Clone();

            //不找父类, 必须相同
            if (_field_type.Type == EdUIFieldType.EType.Component)
            {
                for (int i = _field_list.Count - 1; i > 0; i--)
                {
                    if (!_field_list[i].FieldType.Equals(_field_type))
                    {
                        _field_list.RemoveAt(i);
                    }
                }
            }
            else
            {
                for (int i = _field_list.Count - 1; i > 0; i--)
                {
                    var field = _field_list[i];
                    if (field.FieldType.Type != EdUIFieldType.EType.SubView)
                    {
                        _field_list.RemoveAt(i);
                        continue;
                    }
                    _field_type.ViewType = config.GetCommonBase(_field_type.ViewType, field.FieldType.ViewType);
                }
            }
        }
    }
}
