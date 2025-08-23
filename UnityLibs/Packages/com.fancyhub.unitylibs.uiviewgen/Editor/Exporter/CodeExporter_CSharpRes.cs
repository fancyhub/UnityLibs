using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FH.UI.ViewGenerate.Ed
{
    public sealed class CodeExporter_CSharpRes : ICodeExporter
    {
        private const string C_CLASS_BEIGN =
            @"
    //PrefabPath:""{prefab_path}"", ParentPrefabPath:""{parent_prefab_path}"", CsClassName:""{class_name}"", ParentCsClassName:""{parent_class_name}""
    public partial class {class_name} : {parent_class_name}
    {
        public {new_flag} const string CPath = ""{path}"";
";

        public const string C_CODE_INIT_BEGIN =
            @"
        #region AutoGen 1
        public override string GetPath() { return CPath; }

        protected override void _AutoInit()
        {
            base._AutoInit();
            var refs = _FindViewReference(""{prefab_name}"");
            if (refs == null)
                return;
";



        private const string C_CODE_INIT_END =
            @"
        }";

        private const string C_CODE_DESTROY_BEGIN =
            @"
        protected override void _AutoDestroy()
        {
            base._AutoDestroy();
";

        private const string C_CODE_DESTROY_END = @"
        }
";

        private const string C_CLASS_END = @"
        #endregion
    }
";

        private UIViewGeneratorConfig.CSharpConfig _Config;
        public CodeExporter_CSharpRes(UIViewGeneratorConfig.CSharpConfig config)
        {
            _Config = config;
        }

        public void Export(EdUIViewGenContext context)
        {
            foreach (var view in context.ViewList)
            {
                //1. 生成 xxx.res.cs
                string file_path = _Config.GenFilePath_Res(view.Desc.PrefabPath);
                VersionControlUtil.Checkout(file_path);
                using StreamWriter sw = new StreamWriter(file_path);
                using var file_scope = CSFileScope.Create(sw, _Config);

                EdStrFormatter formater = _GenStrFormatter(view);
                sw.WriteLine(formater.Format(C_CLASS_BEIGN));

                //1. 变量声明
                {
                    //public Transform _btn;
                    FieldExporter.Export_Declaration(_Config, sw, view.Fields);

                    //public List<Transform> _btn_list;
                    ListFieldExporter.Export_Declaration(_Config, sw, view.ListFields);
                }

                //2. 变量初始化
                {
                    sw.WriteLine(formater.Format(C_CODE_INIT_BEGIN));

                    FieldExporter.Export_Init(_Config, sw, view.Fields);
                    ListFieldExporter.Export_Init(sw, view.ListFields);
                    sw.WriteLine(C_CODE_INIT_END);
                }


                //3. 变量 Destroy
                {
                    sw.WriteLine(C_CODE_DESTROY_BEGIN);
                    FieldExporter.Export_Destroy(sw, view.Fields);
                    ListFieldExporter.Export_Destroy(sw, view.ListFields);
                    sw.WriteLine(C_CODE_DESTROY_END);
                }

                //4. 输出结尾
                sw.WriteLine(C_CLASS_END);
            }
        }

        private EdStrFormatter _GenStrFormatter(EdUIView view)
        {
            EdStrFormatter formater = new EdStrFormatter();
            formater.Add("class_name", _Config.GenClassName(view.Desc.PrefabName));
            formater.Add("prefab_name", view.Desc.PrefabName);

            if (view.ParentDesc == null)
            {
                formater.Add("new_flag", "");
                formater.Add("parent_class_name", _Config.BaseClassName);
            }
            else
            {
                formater.Add("new_flag", " new ");
                formater.Add("parent_class_name", _Config.GenClassName(view.ParentDesc.PrefabName));
            }
            formater.Add("prefab_path", view.Desc.PrefabPath);
            formater.Add("parent_prefab_path", view.Desc.ParentPrefabPath);


            switch (_Config.PathMode)
            {
                case UIViewGeneratorConfig.EPathMode.AssetPath:
                    formater.Add("path", view.Desc.PrefabPath);
                    break;
                case UIViewGeneratorConfig.EPathMode.ResourcePath:
                    formater.Add("path", _GetResourcePath(_Config.ResourcePath, view.Desc.PrefabPath));
                    break;
                case UIViewGeneratorConfig.EPathMode.PrefabName:
                    formater.Add("path", view.Desc.PrefabName);
                    break;
            }
            return formater;
        }

        private static string _GetResourcePath(string resource_folder_name, string asset_path)
        {
            int start_pos = asset_path.LastIndexOf(resource_folder_name);
            if (start_pos < 0)
            {
                UnityEngine.Debug.LogError($"Path: {asset_path} Doesn't Contain \"{resource_folder_name}\"");
                return "";
            }

            start_pos += resource_folder_name.Length;
            int end_pos = asset_path.Length - ".prefab".Length;
            return asset_path.Substring(start_pos, end_pos - start_pos);
        }

        private static class ListFieldExporter
        {
            /// <summary>
            /// 输出 成员变量的获取， 输出的情况
            /// 没有parent 才能声明成员变量
            /// </summary>
            public static void Export_Declaration(UIViewGeneratorConfig.CSharpConfig config, StreamWriter sw, List<EdUIViewListField> fields)
            {
                foreach (var field in fields)
                {
                    if (!_ShouldExport(field))
                        continue;

                    if (null != field.Parent)
                    {
                        //在parent 里面声明该对象
                        continue;
                    }

                    sw.WriteLine("\t\tpublic List<{0}> {1}_list = new List<{0}>();", _GetFieldTypeName(config, field.FieldType), field.FieldName);
                }
            }

            public static void Export_Init(StreamWriter sw, List<EdUIViewListField> fields)
            {
                foreach (var list_field in fields)
                {
                    if (!_ShouldExport(list_field))
                        return;

                    foreach (var field in list_field.FieldList)
                    {
                        sw.WriteLine("\t\t\t{0}_list.Add({1});", list_field.FieldName, field.Field.Fieldname);
                    }
                }
            }

            public static void Export_Destroy(StreamWriter sw, List<EdUIViewListField> fields)
            {
                foreach (var field in fields)
                {
                    if (!_ShouldExport(field))
                        return;

                    sw.WriteLine("\t\t\t{0}_list.Clear();", field.FieldName);
                }
            }


            //只有 field_list.Count ==1 并且没有parent，没有child，那就完全不输出了
            private static bool _ShouldExport(EdUIViewListField field)
            {
                if (field.FieldList.Count > 1)
                    return true;

                if (null != field.Parent)
                    return true;
                if (field.HasChild)
                    return true;
                return false;
            }


            private static string _GetFieldTypeName(UIViewGeneratorConfig.CSharpConfig config, EdUIFieldType field_type)
            {
                if (field_type.Type == EdUIFieldType.EType.Component)
                    return field_type.CompType.FullName;

                if (field_type.ViewType == null)
                    return config.BaseClassName;
                return config.GenClassName(field_type.ViewType.PrefabName);
            }
        }

        private static class FieldExporter
        {
            /// <summary>
            /// 输出 成员变量
            /// </summary>
            public static void Export_Declaration(UIViewGeneratorConfig.CSharpConfig config, StreamWriter sw, List<EdUIField> fields)
            {
                foreach (var field in fields)
                {
                    sw.WriteLine("\t\tpublic {0} {1};", _GetFieldTypeName(config, field.FieldType), field.Fieldname);
                }
            }

            private static string _GetFieldTypeName(UIViewGeneratorConfig.CSharpConfig config, EdUIFieldType field_type)
            {
                if (field_type.Type == EdUIFieldType.EType.Component)
                    return field_type.CompType.FullName;

                return config.GenClassName(field_type.ViewType.PrefabName);
            }

            /// <summary>
            /// 输出 成员变量的获取
            /// </summary>
            public static void Export_Init(UIViewGeneratorConfig.CSharpConfig config, StreamWriter sw, List<EdUIField> fields)
            {
                foreach (var field in fields)
                {
                    switch (field.FieldType.Type)
                    {
                        case EdUIFieldType.EType.Component:
                            sw.WriteLine("\t\t\t{0} = refs.GetComp<{1}>(\"{0}\");", field.Fieldname, _GetFieldTypeName(config, field.FieldType));
                            break;
                        case EdUIFieldType.EType.SubView:
                            sw.WriteLine("\t\t\t{0} = _CreateSub<{1}>(refs.GetObj(\"{0}\"));", field.Fieldname, _GetFieldTypeName(config, field.FieldType));
                            break;
                    }
                }
            }

            public static void Export_Destroy(StreamWriter sw, List<EdUIField> fields)
            {
                foreach (var field in fields)
                {
                    switch (field.FieldType.Type)
                    {
                        case EdUIFieldType.EType.Component:
                            break;
                        case EdUIFieldType.EType.SubView:
                            sw.WriteLine("\t\t\t{0}.Destroy();", field.Fieldname);
                            break; ;
                    }
                }
            }
        }
    }
}
