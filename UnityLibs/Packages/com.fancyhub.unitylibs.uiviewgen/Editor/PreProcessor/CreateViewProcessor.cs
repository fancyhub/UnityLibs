/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace FH.UI.ViewGenerate.Ed
{
    public class CreateViewProcessor : IViewGeneratePreprocessor
    {
        public void Process(EdUIViewGenContext context)
        {
            List<EdUIView> view_list = new List<EdUIView>();
            for (; ; )
            {
                EdUIViewDesc next_conf = context.GetNextPrefabConf();
                if (null == next_conf)
                    break;

                EdUIView view = _CreateView(context, next_conf);
                view.Fields = _CreateFields(context, view);
                view_list.Add(view);
            }

            context.ViewList = view_list;
        }


        public static EdUIView _CreateView(EdUIViewGenContext context, EdUIViewDesc desc)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(desc.PrefabPath);

            Debug.Assert(prefab != null, "Load Prefab failed "+desc.PrefabPath);
            var type = PrefabUtility.GetPrefabAssetType(prefab);
            switch (type)
            {
                case PrefabAssetType.Regular:
                    {
                        EdUIView ret = new EdUIView();
                        ret.Desc = desc;
                        ret.Prefab = prefab;
                        ret.ParentDesc = null;
                        return ret;
                    }

                case PrefabAssetType.Variant:
                    {
                        GameObject parent_prefab = EdUIViewGenPrefabUtil.GetOrigPrefabWithVariant(prefab);
                        string parent_prefab_path = AssetDatabase.GetAssetPath(parent_prefab);
                        if (context.Config.IsPrefabPathValid(parent_prefab_path))
                        {
                            var parent_desc = context.AddDependPath_Variant(parent_prefab_path);
                            EdUIView ret = new EdUIView();
                            ret.Desc = desc;
                            ret.Prefab = prefab;
                            ret.ParentDesc = parent_desc;

                            if (desc.ParentPrefabPath != parent_prefab_path)
                            {
                                desc.SetParentPrefabPath(parent_prefab_path);
                            }
                            return ret;
                        }
                        else
                        {
                            UnityEngine.Debug.LogErrorFormat("Prefab 路径不合法\n Prefab : [{0}] \n Variant Prefab : [{1}]\n", parent_prefab_path, desc.PrefabPath);
                            EdUIView ret = new EdUIView();
                            ret.Desc = desc;
                            ret.Prefab = prefab;
                            ret.ParentDesc = null;
                            return ret;
                        }
                    }

                default:
                    Debug.LogErrorFormat(prefab, " 未知的类型 {0}", type);
                    return null;
            }
        }


        private static List<EdUIField> _CreateFields(EdUIViewGenContext context, EdUIView view)
        {
            List<EdUIField> field_list = new List<EdUIField>();
            Transform root_tran = view.Prefab.transform;
            Transform[] transforms = view.Prefab.GetComponentsInChildren<Transform>(true);
            foreach (Transform tran in transforms)
            {
                EEDUIObjType obj_type = EdUIViewGenPrefabUtil.GetTargetType(tran, root_tran);
                switch (obj_type)
                {
                    case EEDUIObjType.Prefab_Self:
                        {
                            EdUIField field = _CreateField_Component(context, root_tran, tran);
                            if (null != field)
                                field_list.Add(field);
                        }
                        break;

                    case EEDUIObjType.Prefab_Variant:
                        {
                            //EdSmUiCompField field = _create_field_variant(root_tran, a);
                            //comp_prefab._fields.Add(field);
                        }
                        break;

                    case EEDUIObjType.Prefab_Inner_Root:
                        {
                            EdUIField field = _CreateField_Prefab(context, root_tran, tran);
                            if (null != field)
                                field_list.Add(field);
                        }
                        break;

                    case EEDUIObjType.Prefab_Inner_Object:
                        {
                            //不处理,该节点由 子 prefab 来处理
                        }
                        break;

                    default:
                        Debug.LogError("Error:" + obj_type);
                        break;
                }
            }

            return field_list;
        }


        private static EdUIField _CreateField_Prefab(EdUIViewGenContext context, Transform root, Transform target)
        {
            if (!target.name.StartsWith("_"))
                return null;

            string inner_prefab_path = EdUIViewGenPrefabUtil.GetInnerPrefabAssetPath(target.gameObject);

            if (context.Config.IsPrefabPathValid(inner_prefab_path))
            {
                string go_path = _GetHierarchyPath(target, root);
                var dep_conf = context.AddDependPath(inner_prefab_path);

                EdUIField field = new EdUIField();
                field.Path = go_path;
                field.FieldType = EdUIFieldType.CreateSubView(dep_conf);
                field.Fieldname = _GenFiledName(target.name);

                return field;
            }
            else //如果子 prefab 在别的目录,就当作普通的GameObject 来处理
            {
                UnityEngine.Debug.LogErrorFormat("Prefab 里面的对象 {0} 对应的路径不合法 {1}", target.name, inner_prefab_path);
                string target_name = target.name;
                //如果不是根节点，必须要以 下划线开头才能导出 _
                if (root != target && !target_name.StartsWith("_"))
                    return null;

                Component component = context.Config.GetComponent(target, root);
                if (null == component)
                    return null;

                string go_path = _GetHierarchyPath(target, root);
                string field_name = _GenFiledName(target_name);

                EdUIField field = new EdUIField();
                field.Path = go_path;
                field.Fieldname = field_name;
                field.FieldType = EdUIFieldType.CreateComponent(component.GetType());
                return field;
            }
        }


        private static EdUIField _CreateField_Component(EdUIViewGenContext context, Transform root, Transform target)
        {
            string target_name = target.name;
            //如果不是根节点，必须要以 下划线开头才能导出 _
            if (root != target && !target_name.StartsWith("_"))
                return null;

            Component component = context.Config.GetComponent(target, root);
            if (null == component)
                return null;

            string go_path = _GetHierarchyPath(target, root);
            string field_name = _GenFiledName(target_name);

            EdUIField field = new EdUIField();
            field.Path = go_path;
            field.Fieldname = field_name;
            field.FieldType = EdUIFieldType.CreateComponent(component.GetType());

            return field;
        }

        private static string _GenFiledName(string name)
        {
            if (name.StartsWith("_"))
                return name;
            return "_" + name;
        }

        #region Hierachy Path
        private static StringBuilder _string_builder = new StringBuilder();
        /// <summary>
        /// 不包括root
        /// </summary>
        private static string _GetHierarchyPath(Transform self, Transform root)
        {
            if (null == self)
                return string.Empty;

            _string_builder.Length = 0;
            _GetHierarchyPath(self, self, root, _string_builder);
            return _string_builder.ToString();
        }

        private static void _GetHierarchyPath(Transform target, Transform obj, Transform root, StringBuilder sb)
        {
            if (obj == root)
            {
                //不包括root节点
                //if (null != obj)
                //{
                // sb.Append(obj.name);
                //}
                return;
            }

            if (null == obj)
            {
                Debug.LogErrorFormat("Root {0} 不是 obj {1} 的 根节点", root, target);
                return;
            }

            _GetHierarchyPath(target, obj.parent, root, sb);

            if (sb.Length > 0)
            {
                sb.Append('/');
            }
            sb.Append(obj.name);
        }
        #endregion
    }
}