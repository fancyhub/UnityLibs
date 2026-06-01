/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   :
 * Desc    :
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace FH.EditorToolHub
{
    internal sealed class EditorToolModuleDescriptor
    {
        private readonly Func<IEditorToolModule> _factory;

        private EditorToolModuleDescriptor(
            string id,
            string title,
            string category,
            int order,
            bool isDefault,
            Type moduleType,
            Type editorWindowType,
            bool isDynamic,
            Func<IEditorToolModule> factory)
        {
            Id = id;
            Title = title;
            Category = category;
            Order = order;
            IsDefault = isDefault;
            ModuleType = moduleType;
            EditorWindowType = editorWindowType;
            IsDynamic = isDynamic;
            _factory = factory;
        }

        public Type ModuleType { get; private set; }
        public Type EditorWindowType { get; private set; }
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Category { get; private set; }
        public int Order { get; private set; }
        public bool IsDefault { get; private set; }
        public bool IsDynamic { get; private set; }

        public static EditorToolModuleDescriptor FromRegisteredModule(Type moduleType, EditorToolModuleAttribute attribute)
        {
            string id = string.IsNullOrEmpty(attribute.Id) ? moduleType.FullName : attribute.Id;
            string title = string.IsNullOrEmpty(attribute.Title) ? moduleType.Name : attribute.Title;
            string category = string.IsNullOrEmpty(attribute.Category) ? "General" : attribute.Category;

            return new EditorToolModuleDescriptor(
                id,
                title,
                category,
                attribute.Order,
                attribute.IsDefault,
                moduleType,
                attribute.ReplacesEditorWindowType,
                false,
                () => (IEditorToolModule)Activator.CreateInstance(moduleType, true));
        }

        public static EditorToolModuleDescriptor FromEditorWindow(Type editorWindowType)
        {
            string id = EditorToolModuleIds.GetEditorWindowModuleId(editorWindowType);
            string title = ReflectedEditorWindowModule.GetDisplayTitle(editorWindowType);
            string category = ReflectedEditorWindowModule.GetCategory(editorWindowType);

            return new EditorToolModuleDescriptor(
                id,
                title,
                category,
                10000,
                false,
                typeof(ReflectedEditorWindowModule),
                editorWindowType,
                true,
                () => new ReflectedEditorWindowModule(editorWindowType, title));
        }

        public IEditorToolModule CreateInstance()
        {
            return _factory();
        }
    }

    internal static class EditorToolModuleRegistry
    {
        private static List<EditorToolModuleDescriptor> _modules;
        private static Dictionary<string, EditorToolModuleDescriptor> _moduleMap;

        public static IReadOnlyList<EditorToolModuleDescriptor> Modules
        {
            get
            {
                EnsureBuilt();
                return _modules;
            }
        }

        public static EditorToolModuleDescriptor Find(string id)
        {
            EnsureBuilt();

            EditorToolModuleDescriptor descriptor;
            if (!string.IsNullOrEmpty(id) && _moduleMap.TryGetValue(id, out descriptor))
                return descriptor;

            return null;
        }

        public static void Rebuild()
        {
            List<EditorToolModuleDescriptor> modules = new List<EditorToolModuleDescriptor>();
            HashSet<string> ids = new HashSet<string>();
            HashSet<Type> replacedEditorWindowTypes = new HashSet<Type>();

            foreach (Type type in TypeCache.GetTypesDerivedFrom<IEditorToolModule>())
            {
                if (type == null || type.IsAbstract || type.IsGenericType)
                    continue;

                if (type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null) == null)
                    continue;

                EditorToolModuleAttribute attribute = type.GetCustomAttribute<EditorToolModuleAttribute>(false);
                if (attribute == null)
                    continue;

                EditorToolModuleDescriptor descriptor = EditorToolModuleDescriptor.FromRegisteredModule(type, attribute);
                if (string.IsNullOrEmpty(descriptor.Id))
                    continue;

                if (!ids.Add(descriptor.Id))
                {
                    Debug.LogWarning("Duplicate EditorToolHub module id: " + descriptor.Id);
                    continue;
                }

                modules.Add(descriptor);

                if (attribute.ReplacesEditorWindowType != null)
                    replacedEditorWindowTypes.Add(attribute.ReplacesEditorWindowType);
            }

            foreach (Type editorWindowType in TypeCache.GetTypesDerivedFrom<EditorWindow>())
            {
                AddReflectedEditorWindowModule(modules, ids, replacedEditorWindowTypes, editorWindowType);
            }

            foreach (Type editorWindowType in ReflectedEditorWindowModule.GetBuiltinEditorWindowTypes())
            {
                AddReflectedEditorWindowModule(modules, ids, replacedEditorWindowTypes, editorWindowType);
            }

            _modules = modules
                .OrderBy(module => module.Category)
                .ThenBy(module => module.Order)
                .ThenBy(module => module.Title)
                .ToList();
            _moduleMap = _modules.ToDictionary(module => module.Id, module => module);
        }

        private static void EnsureBuilt()
        {
            if (_modules == null || _moduleMap == null)
                Rebuild();
        }

        private static void AddReflectedEditorWindowModule(
            List<EditorToolModuleDescriptor> modules,
            HashSet<string> ids,
            HashSet<Type> replacedEditorWindowTypes,
            Type editorWindowType)
        {
            if (!ReflectedEditorWindowModule.CanAdapt(editorWindowType))
                return;

            if (replacedEditorWindowTypes.Contains(editorWindowType))
                return;

            string id = EditorToolModuleIds.GetEditorWindowModuleId(editorWindowType);
            if (ids.Contains(id))
                return;

            EditorToolModuleDescriptor descriptor = EditorToolModuleDescriptor.FromEditorWindow(editorWindowType);
            if (!ids.Add(descriptor.Id))
                return;

            modules.Add(descriptor);
        }
    }
}
