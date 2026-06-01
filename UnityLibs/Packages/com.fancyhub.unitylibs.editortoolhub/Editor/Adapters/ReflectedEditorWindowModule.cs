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
using UnityEngine.UIElements;

namespace FH.EditorToolHub
{
    internal sealed class ReflectedEditorWindowModule : IEditorToolModule, IEditorToolModuleLifecycle
    {
        private static readonly Dictionary<string, string> CBuiltinWindowTitles = new Dictionary<string, string>
        {
            { "UnityEditor.ProjectBrowser", "Project" },
            { "UnityEditor.InspectorWindow", "Inspector" },
            { "UnityEditor.SceneHierarchyWindow", "Hierarchy" },
            { "UnityEditor.ConsoleWindow", "Console" },
            { "UnityEditor.SceneView", "Scene" },
            { "UnityEditor.GameView", "Game" },
            { "UnityEditor.AnimationWindow", "Animation" },
            { "UnityEditor.ProfilerWindow", "Profiler" },
        };

        private readonly Type _windowType;
        private readonly string _title;
        private EditorWindow _window;
        private MethodInfo _onDisableMethod;

        public ReflectedEditorWindowModule(Type windowType, string title)
        {
            _windowType = windowType;
            _title = title;
        }

        public VisualElement CreateGUI(EditorToolModuleContext context)
        {
            VisualElement root = new VisualElement();
            root.style.flexGrow = 1;

            if (!CreateHiddenWindow())
            {
                root.Add(new HelpBox("Failed to create EditorWindow: " + _windowType.FullName, HelpBoxMessageType.Error));
                return root;
            }

            MethodInfo createGuiMethod = FindEditorWindowMethod(_windowType, "CreateGUI");
            MethodInfo onGuiMethod = FindEditorWindowMethod(_windowType, "OnGUI");

            if (createGuiMethod != null)
            {
                VisualElement content = CreateUIToolkitContent(createGuiMethod);
                root.Add(content);
                return root;
            }

            if (onGuiMethod != null)
            {
                IMGUIContainer container = null;
                container = new IMGUIContainer(() =>
                {
                    if (_window != null && container != null)
                        _window.position = new Rect(0, 0, container.contentRect.width, container.contentRect.height);

                    InvokeWindowMethod(onGuiMethod);
                });
                container.style.flexGrow = 1;
                root.Add(container);
                return root;
            }

            root.Add(new HelpBox("No CreateGUI or OnGUI method was found.", HelpBoxMessageType.Info));
            return root;
        }

        public void OnDisable()
        {
            if (_window == null)
                return;

            InvokeWindowMethod(_onDisableMethod);
            UnityEngine.Object.DestroyImmediate(_window);
            _window = null;
        }

        public static bool CanAdapt(Type windowType)
        {
            if (windowType == null || windowType.IsAbstract || windowType.IsGenericType)
                return false;

            if (!typeof(EditorWindow).IsAssignableFrom(windowType))
                return false;

            if (windowType == typeof(EditorToolHubWindow))
                return false;

            if (IsUnityEditorWindow(windowType) && !IsAllowedUnityEditorWindow(windowType))
                return false;

            return FindEditorWindowMethod(windowType, "CreateGUI") != null || FindEditorWindowMethod(windowType, "OnGUI") != null;
        }

        public static IEnumerable<Type> GetBuiltinEditorWindowTypes()
        {
            foreach (string fullName in CBuiltinWindowTitles.Keys)
            {
                Type type = FindLoadedType(fullName);
                if (type != null && CanAdapt(type))
                    yield return type;
            }
        }

        public static string GetCategory(Type windowType)
        {
            return IsAllowedUnityEditorWindow(windowType) ? "Unity Windows" : "Reflected Windows";
        }

        public static string GetDisplayTitle(Type windowType)
        {
            string title;
            if (CBuiltinWindowTitles.TryGetValue(windowType.FullName, out title))
                return title;

            string name = windowType.Name;
            if (name.EndsWith("EditorWindow", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - "EditorWindow".Length);
            else if (name.EndsWith("Window", StringComparison.Ordinal))
                name = name.Substring(0, name.Length - "Window".Length);

            return ObjectNames.NicifyVariableName(name);
        }

        private bool CreateHiddenWindow()
        {
            try
            {
                _window = ScriptableObject.CreateInstance(_windowType) as EditorWindow;
                if (_window == null)
                    return false;

                _window.hideFlags = HideFlags.HideAndDontSave;
                _window.titleContent = new GUIContent(_title);
                _onDisableMethod = FindEditorWindowMethod(_windowType, "OnDisable");

                InvokeWindowMethod(FindEditorWindowMethod(_windowType, "OnEnable"));
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        private VisualElement CreateUIToolkitContent(MethodInfo createGuiMethod)
        {
            VisualElement host = new VisualElement();
            host.style.flexGrow = 1;

            try
            {
                InvokeWindowMethod(createGuiMethod);

                VisualElement windowRoot = _window.rootVisualElement;
                while (windowRoot.childCount > 0)
                {
                    VisualElement child = windowRoot[0];
                    child.RemoveFromHierarchy();
                    host.Add(child);
                }

                if (host.childCount == 0)
                    host.Add(new HelpBox("CreateGUI completed but did not add visual content.", HelpBoxMessageType.Info));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                host.Add(new HelpBox("CreateGUI failed: " + ex.Message, HelpBoxMessageType.Error));
            }

            return host;
        }

        private void InvokeWindowMethod(MethodInfo method)
        {
            if (_window == null || method == null)
                return;

            try
            {
                method.Invoke(_window, null);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                    Debug.LogException(ex.InnerException);
                else
                    Debug.LogException(ex);
            }
        }

        private static MethodInfo FindEditorWindowMethod(Type windowType, string methodName)
        {
            for (Type type = windowType; type != null && type != typeof(EditorWindow); type = type.BaseType)
            {
                MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (method != null && method.GetParameters().Length == 0)
                    return method;
            }

            return null;
        }

        private static Type FindLoadedType(string fullName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type type = assemblies[i].GetType(fullName, false);
                if (type != null)
                    return type;
            }

            return null;
        }

        private static bool IsUnityEditorWindow(Type windowType)
        {
            if (windowType == null)
                return false;

            string assemblyName = windowType.Assembly.GetName().Name;
            if (!string.IsNullOrEmpty(assemblyName) && assemblyName.StartsWith("UnityEditor", StringComparison.Ordinal))
                return true;

            return windowType.Namespace != null && windowType.Namespace.StartsWith("UnityEditor", StringComparison.Ordinal);
        }

        private static bool IsAllowedUnityEditorWindow(Type windowType)
        {
            return windowType != null && CBuiltinWindowTitles.ContainsKey(windowType.FullName);
        }
    }
}
