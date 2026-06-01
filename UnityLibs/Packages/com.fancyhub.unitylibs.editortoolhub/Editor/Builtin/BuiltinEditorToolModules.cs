/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   :
 * Desc    :
*************************************************************************************/

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FH.EditorToolHub
{
    [EditorToolModule("fh.editor-tool-hub.selection", "Selection", Category = "Unity", Order = 0, IsDefault = true)]
    internal sealed class SelectionOverviewModule : IEditorToolModule
    {
        private VisualElement _listRoot;

        public VisualElement CreateGUI(EditorToolModuleContext context)
        {
            VisualElement root = new VisualElement();
            _listRoot = new VisualElement();
            root.Add(_listRoot);

            Refresh();
            root.schedule.Execute(Refresh).Every(500);
            return root;
        }

        private void Refresh()
        {
            if (_listRoot == null)
                return;

            _listRoot.Clear();

            Object[] objects = Selection.objects;
            if (objects == null || objects.Length == 0)
            {
                _listRoot.Add(new Label("No selection"));
                return;
            }

            for (int i = 0; i < objects.Length; i++)
            {
                Object obj = objects[i];
                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 4;

                ObjectField objectField = new ObjectField();
                objectField.objectType = typeof(Object);
                objectField.value = obj;
                objectField.SetEnabled(false);
                objectField.style.flexGrow = 1;
                row.Add(objectField);

                Button pingButton = new Button(() => EditorGUIUtility.PingObject(obj)) { text = "Ping" };
                pingButton.style.width = 54;
                pingButton.style.marginLeft = 4;
                row.Add(pingButton);

                _listRoot.Add(row);
            }
        }
    }

    [EditorToolModule("fh.editor-tool-hub.quick-actions", "Quick Actions", Category = "Unity", Order = 10, IsDefault = true)]
    internal sealed class QuickActionsModule : IEditorToolModule
    {
        public VisualElement CreateGUI(EditorToolModuleContext context)
        {
            VisualElement root = new VisualElement();
            root.style.flexDirection = FlexDirection.Row;
            root.style.flexWrap = Wrap.Wrap;

            root.Add(CreateActionButton("Save Assets", AssetDatabase.SaveAssets));
            root.Add(CreateActionButton("Refresh", AssetDatabase.Refresh));
            root.Add(CreateActionButton("Project Settings", () => SettingsService.OpenProjectSettings(string.Empty)));
            root.Add(CreateActionButton("Preferences", () => SettingsService.OpenUserPreferences(string.Empty)));

            return root;
        }

        private static Button CreateActionButton(string text, System.Action action)
        {
            Button button = new Button(action) { text = text };
            button.style.height = 28;
            button.style.marginRight = 6;
            button.style.marginBottom = 6;
            return button;
        }
    }

    [EditorToolModule("fh.editor-tool-hub.imgui-sandbox", "IMGUI Sandbox", Category = "Samples", Order = 100)]
    internal sealed class IMGUISandboxModule : IMGUIEditorToolModule
    {
        private string _text = "Hello";
        private Object _object;

        protected override void OnGUI(EditorToolModuleContext context)
        {
            _text = EditorGUILayout.TextField("Text", _text);
            _object = EditorGUILayout.ObjectField("Object", _object, typeof(Object), true);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping Object") && _object != null)
                    EditorGUIUtility.PingObject(_object);

                if (GUILayout.Button("Repaint"))
                    context.Repaint();
            }
        }
    }
}
