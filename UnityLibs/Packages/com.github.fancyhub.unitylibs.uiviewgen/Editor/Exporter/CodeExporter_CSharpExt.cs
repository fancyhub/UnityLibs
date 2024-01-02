using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;

namespace FH.UI.ViewGenerate.Ed
{
    public sealed class CSFileScope : IDisposable
    {
        public string _begin;
        public string _end;
        public StreamWriter _sw;
        public CSFileScope(StreamWriter sw, string begin, string end)
        {
            _sw = sw;
            _begin = begin;
            _end = end;

            sw.WriteLine(_begin);
        }

        public void Dispose()
        {
            _sw.WriteLine(_end);
        }

        public static CSFileScope Create(StreamWriter sw, UIViewGeneratorConfig.CSharpConfig config)
        {
            string start = @"
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace " + config.NameSpace + "\n{";

            return new CSFileScope(sw, start, "}");
        }
    }

    public sealed class CodeExporter_CSharpExt : ICodeExporter
    {
        private const string C_PARTIAL_CODE =
            @"
    public partial class {class_name} // : {parent_class_name} 
    {
        //public override void OnCreate()
        //{
        //    base.OnCreate();
        //}

        //public override void OnDestroy()
        //{
        //    base.OnDestroy();    
        //}
    }
";

        private UIViewGeneratorConfig.CSharpConfig _Config;
        public CodeExporter_CSharpExt(UIViewGeneratorConfig.CSharpConfig config)
        {
            _Config = config;
        }

        public void Export(EdUIViewGenContext context)
        {
            foreach (var view in context.ViewList)
            {
                string file_path = _Config.GenFilePath_Ext(view.Desc.PrefabName);
                if (File.Exists(file_path))
                    return;
                using StreamWriter sw = new StreamWriter(file_path);

                using var file_scope = CSFileScope.Create(sw, _Config);

                //生成默认的 ext.cs 代码
                string code = _GenStrFormatter(view).Format(C_PARTIAL_CODE);

                sw.WriteLine(code);
            }
        }

        private EdStrFormatter _GenStrFormatter(EdUIView view)
        {
            EdStrFormatter formater = new EdStrFormatter();
            formater.Add("class_name", _Config.GenClassName(view.Desc.PrefabName));
            if (view.ParentDesc != null)
                formater.Add("parent_class_name", _Config.GenClassName(view.ParentDesc.PrefabName));
            else
                formater.Add("parent_class_name", _Config.BaseClassName);

            return formater;
        }
    }
}
