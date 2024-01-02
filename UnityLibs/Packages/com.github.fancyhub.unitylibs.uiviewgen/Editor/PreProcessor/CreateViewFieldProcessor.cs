/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FH.UI.ViewGenerate.Ed
{
    public class CreateViewFieldProcessor : IViewGeneratePreprocessor
    {
        public void Process(EdUIViewGenContext context)
        {
            foreach (var view in context.ViewList)
            {
                view.Fields = CreateFields(context,view.Prefab);
            }
        }

        public static List<EdUIField> CreateFields(EdUIViewGenContext context,GameObject prefab)
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
                            EdUIField field = _CreateField_Component(context,root_tran, tran);
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
                            EdUIField field = _CreateField_Prefab(context,root_tran, tran);
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
                field.FieldType = EdUIFieldType.CreateSubView(dep_conf.ClassName);
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