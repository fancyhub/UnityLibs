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
    /// <summary>
    /// 用来自动生成 UI 代码的
    /// </summary>
    public sealed class ViewGenerator
    {
        private ICodeAnalyser _CodeAnylyser;
        private UIViewGeneratorConfig _Config;
        private EdUIViewGenContext _Context;
        private EdUIViewDescDB _DBDesc;
        private List<ICodeExporter> _Exporters;
        private List<IViewGeneratePreprocessor> _Preprocessors;

        public ViewGenerator(UIViewGeneratorConfig config)
        {
            _Config = config;
            _CodeAnylyser = new CodeAnalyser_CSharp();


            List<EdUIViewDesc> all_desc = _CodeAnylyser.ParseAll(config.Csharp.CodeFolder);

            _DBDesc = new EdUIViewDescDB(config, all_desc);
            _Context = new EdUIViewGenContext(_DBDesc);
            _Exporters = new List<ICodeExporter>()
            {
                new CodeExporter_CSharpRes(config.Csharp),
                new CodeExporter_CSharpExt(config.Csharp),
            };

            _Preprocessors = new List<IViewGeneratePreprocessor>()
            {
                new CreateViewProcessor(),
                new CreateViewFieldProcessor(),
                new LinkViewProcessor(),
                new ListFiledsProcessor(),
                new ViewCompReferenceProcessor(),
            };
        }

        public void GenCode(GameObject prefab)
        {
            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogErrorFormat("{0}不是prefab, 选中prefab才允许生成", prefab.name);
                return;
            }
            if (!_Config.IsPrefabPathValid(path))
            {
                Debug.LogErrorFormat("{0}路径不合法, 不允许生成代码", prefab.name);
                return;
            }
            _Context.AddInitPath(path);

            foreach (var p in _Preprocessors)
            {
                p.Process(_Context);
            }

            foreach (var exporter in _Exporters)
            {
                exporter.Export(_Context);
            }
        }

        public void RegenAllCode()
        {
            _DBDesc.RemoveInvalidatePath();
            _Context.AddInitPaths(_DBDesc.GetPathList());

            foreach (var p in _Preprocessors)
            {
                p.Process(_Context);
            }

            foreach (var exporter in _Exporters)
            {
                exporter.Export(_Context);
            }
        }

        public void RemoveInvalidateCodeFiles()
        {
            _DBDesc.RemoveInvalidatePath();

            HashSet<string> validate_file_names = new HashSet<string>();
            foreach (var p in _DBDesc.GetAllDesc())
            {
                validate_file_names.Add(p.GetCsFileNameRes());
                validate_file_names.Add(p.GetCsFileNameRes() + ".meta");

                validate_file_names.Add(p.GetCsFileNameExt());
                validate_file_names.Add(p.GetCsFileNameExt() + ".meta");
            }

            string[] files = System.IO.Directory.GetFiles(_Config.Csharp.CodeFolder);
            foreach (string file_name in files)
            {
                string name = System.IO.Path.GetFileName(file_name);
                if (validate_file_names.Contains(name))
                    continue;
                File.Delete(file_name);
            }
        }       
    }
}