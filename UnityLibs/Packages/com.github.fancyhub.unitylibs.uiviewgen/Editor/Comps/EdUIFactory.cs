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
    public class EdUIFactory
    {
        public static List<System.Type> CompTypeList;
        public EdUIViewData _data;
        public EdUIFactory(EdUIViewData data)
        {
            _data = data;
            CompTypeList = data.Config._CompTypeList;
        }

        #region CreateView
        public EdUIView CreateView(EdUIViewConf conf)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(conf.PrefabPath);

            var type = PrefabUtility.GetPrefabAssetType(prefab);
            switch (type)
            {
                case PrefabAssetType.Regular:
                    {
                        EdUIView ret = new EdUIView();
                        ret.Conf = conf;
                        ret.Prefab = prefab;
                        ret.IsVariant = false;
                        ret.ParentClass = _data.Config.BaseClassName;
                        return ret;
                    }

                case PrefabAssetType.Variant:
                    {
                        GameObject orig_prefab = EdUIViewGenPrefabUtil.GetOrigPrefabWithVariant(prefab);
                        string orig_prefab_path = AssetDatabase.GetAssetPath(orig_prefab);
                        if (_data.Config.IsPrefabPathValid(orig_prefab_path))
                        {
                            var parent_conf = _data.AddDependPath_Variant(orig_prefab_path);
                            EdUIView ret = new EdUIView();
                            ret.Conf = conf;
                            ret.Prefab = prefab;
                            ret.ParentClass = parent_conf.ClassName;
                            ret.IsVariant = true;
                            return ret;
                        }
                        else
                        {
                            Log.E("Prefab 路径不合法\n Prefab : [{0}] \n Variant Prefab : [{1}]\n", orig_prefab_path, conf.PrefabPath);
                            EdUIView ret = new EdUIView();
                            ret.Conf = conf;
                            ret.Prefab = prefab;
                            ret.ParentClass = _data.Config.BaseClassName;
                            ret.IsVariant = false;
                            return ret;
                        }
                    }

                default:
                    Debug.LogErrorFormat(prefab, " 未知的类型 {0}", type);
                    return null;
            }
        }
        #endregion

        #region Create Field
        public List<EdUIField> CreateFields(GameObject prefab)
        {
            List<EdUIField> ret = new List<EdUIField>();
            Transform root_tran = prefab.transform;
            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            foreach (Transform tran in transforms)
            {
                EEDUIObjType obj_type = EdUIViewGenPrefabUtil.GetTargetType(tran, root_tran);
                switch (obj_type)
                {
                    case EEDUIObjType.prefab_self:
                        {
                            EdUIField field = _CreateField_Component(root_tran, tran);
                            if (null != field)
                                ret.Add(field);
                        }
                        break;

                    case EEDUIObjType.prefab_variant:
                        {
                            //EdSmUiCompField field = _create_field_variant(root_tran, a);
                            //comp_prefab._fields.Add(field);
                        }
                        break;

                    case EEDUIObjType.prefab_inner_root:
                        {
                            EdUIField field = _CreateField_Prefab(root_tran, tran, _data);
                            if (null != field)
                                ret.Add(field);
                        }
                        break;

                    case EEDUIObjType.prefab_inner_obj:
                        {
                            //不处理,该节点由 子prefab 来处理
                        }
                        break;

                    default:
                        Debug.LogError("Error:" + obj_type);
                        break;
                }
            }
            return ret;
        }

        private static EdUIField _CreateField_Prefab(Transform root, Transform target, EdUIViewData view_data)
        {
            if (!target.name.StartsWith("_"))
                return null;

            string inner_prefab_path = EdUIViewGenPrefabUtil.GetInnerPrefabAssetPath(target.gameObject);

            if (view_data.Config.IsPrefabPathValid(inner_prefab_path))
            {
                string go_path = target.ExtGetHierarchyPath(root);
                var dep_conf = view_data.AddDependPath(inner_prefab_path);

                EdUIField field = new EdUIField();
                field.Path = go_path;
                field.FieldType = EdUIFieldType.CreateSubView(dep_conf.ClassName);
                field.Fieldname = _GenFiledName(target.name);

                return field;
            }
            else //如果子 prefab 在别的目录,就当作普通的GameObject 来处理
            {
                Log.E("Prefab 里面的对象 {0} 对应的路径不合法 {1}", target.name, inner_prefab_path);
                string target_name = target.name;
                //如果不是根节点，必须要以 下划线开头才能导出 _
                if (root != target && !target_name.StartsWith("_"))
                    return null;

                Component component = _GetField_Component(target, root);
                if (null == component)
                    return null;

                string go_path = target.ExtGetHierarchyPath(root);
                string field_name = _GenFiledName(target_name);

                EdUIField field = new EdUIField();
                field.Path = go_path;
                field.Fieldname = field_name;
                field.FieldType = EdUIFieldType.CreateComponent(component.GetType());
                return field;
            }
        }


        private static EdUIField _CreateField_Component(Transform root, Transform target)
        {
            string target_name = target.name;
            //如果不是根节点，必须要以 下划线开头才能导出 _
            if (root != target && !target_name.StartsWith("_"))
                return null;

            Component component = _GetField_Component(target, root);
            if (null == component)
                return null;

            string go_path = target.ExtGetHierarchyPath(root);
            string field_name = _GenFiledName(target_name);

            EdUIField field = new EdUIField();
            field.Path = go_path;
            field.Fieldname = field_name;
            field.FieldType = EdUIFieldType.CreateComponent(component.GetType());

            return field;
        }

        private static Component _GetField_Component(Transform target, Transform root)
        {
            foreach (System.Type t in CompTypeList)
            {
                Component obj = target.GetComponent(t);
                if (null != obj)
                    return obj;
            }

            //如果不是根节点，但是 _开头，就把transform导出
            if (target != root)
            {
                return target.GetComponent<Transform>();
            }

            return null;
        }

        private static string _GenFiledName(string name)
        {
            if (name.StartsWith("_"))
                return name;
            return "_" + name;
        }
        #endregion

        #region Create List Field
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
        #endregion

    }

}
