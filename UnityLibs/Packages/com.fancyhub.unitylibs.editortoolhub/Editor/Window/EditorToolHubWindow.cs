/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   :
 * Desc    :
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FH.EditorToolHub
{
    public sealed class EditorToolHubWindow : EditorWindow
    {
        private const string CNoneModuleId = "<none>";

        private EditorToolHubLayoutAsset _layoutAsset;
        private EditorToolHubLayoutData _layout;
        private VisualElement _workspace;
        private ObjectField _layoutAssetField;
        private readonly List<IEditorToolModule> _activeModules = new List<IEditorToolModule>();

        [MenuItem("Tools/FancyHub/Editor Tool Hub")]
        public static void Open()
        {
            EditorToolHubWindow window = GetWindow<EditorToolHubWindow>();
            window.titleContent = new GUIContent("Tool Hub");
            window.minSize = new Vector2(720, 420);
            window.Show();
        }

        private void OnDisable()
        {
            ReleaseActiveModules();
        }

        public void CreateGUI()
        {
            _layoutAsset = EditorToolHubLayoutStore.LoadActiveAsset();
            _layout = EditorToolHubLayoutStore.Load(_layoutAsset);

            rootVisualElement.Clear();
            rootVisualElement.style.flexDirection = FlexDirection.Column;

            BuildToolbar();
            BuildWorkspaceRoot();
            RefreshWorkspace();
        }

        private void BuildToolbar()
        {
            Toolbar toolbar = new Toolbar();

            Label title = new Label("Editor Tool Hub");
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginLeft = 6;
            title.style.marginRight = 12;
            toolbar.Add(title);

            ToolbarToggle editToggle = new ToolbarToggle { text = "Edit Layout", value = _layout.editMode };
            editToggle.RegisterValueChangedCallback(evt =>
            {
                _layout.editMode = evt.newValue;
                SaveLayout();
                RefreshWorkspace();
            });
            toolbar.Add(editToggle);

            ToolbarSpacer spacer = new ToolbarSpacer();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            _layoutAssetField = new ObjectField();
            _layoutAssetField.objectType = typeof(EditorToolHubLayoutAsset);
            _layoutAssetField.allowSceneObjects = false;
            _layoutAssetField.value = _layoutAsset;
            _layoutAssetField.style.width = 230;
            _layoutAssetField.RegisterValueChangedCallback(evt => SetLayoutAsset(evt.newValue as EditorToolHubLayoutAsset));
            toolbar.Add(_layoutAssetField);

            toolbar.Add(new ToolbarButton(SaveLayoutAsset) { text = "Save Asset" });
            toolbar.Add(new ToolbarButton(SaveLayoutAsAsset) { text = "Save As" });
            toolbar.Add(new ToolbarButton(() => SetLayoutAsset(null)) { text = "Use Transient" });

            ToolbarButton reloadButton = new ToolbarButton(() =>
            {
                EditorToolModuleRegistry.Rebuild();
                RefreshWorkspace();
            })
            { text = "Reload Modules" };
            toolbar.Add(reloadButton);

            ToolbarButton resetButton = new ToolbarButton(() =>
            {
                _layout = EditorToolHubLayoutStore.CreateDefault();
                SaveLayout();
                RefreshWorkspace();
            })
            { text = "Reset Layout" };
            toolbar.Add(resetButton);

            rootVisualElement.Add(toolbar);
        }

        private void BuildWorkspaceRoot()
        {
            _workspace = new VisualElement();
            _workspace.style.flexGrow = 1;
            _workspace.style.paddingLeft = 4;
            _workspace.style.paddingRight = 4;
            _workspace.style.paddingTop = 4;
            _workspace.style.paddingBottom = 4;
            rootVisualElement.Add(_workspace);
        }

        private void RefreshWorkspace()
        {
            if (_layout == null)
                _layout = EditorToolHubLayoutStore.Load(_layoutAsset);

            _layout.EnsureValid();
            ReleaseActiveModules();
            _workspace.Clear();
            _workspace.Add(CreateNodeView(_layout.root));
        }

        private VisualElement CreateNodeView(EditorToolHubLayoutNode node)
        {
            node.EnsureValid();

            if (node.IsSplit)
                return CreateSplitView(node);

            return CreatePanelView(node);
        }

        private VisualElement CreateSplitView(EditorToolHubLayoutNode node)
        {
            VisualElement split = new VisualElement();
            split.style.flexGrow = 1;
            split.style.flexBasis = 0;
            split.style.flexDirection = node.splitDirection == EditorToolHubSplitDirection.Vertical
                ? FlexDirection.Row
                : FlexDirection.Column;

            VisualElement first = CreateNodeView(node.first);
            VisualElement second = CreateNodeView(node.second);
            VisualElement splitter = CreateSplitter(node, split, first, second);

            ApplySplitFraction(node, first, second);

            split.Add(first);
            split.Add(splitter);
            split.Add(second);
            return split;
        }

        private VisualElement CreatePanelView(EditorToolHubLayoutNode node)
        {
            VisualElement panel = new VisualElement();
            panel.style.flexGrow = 1;
            panel.style.flexBasis = 0;
            panel.style.marginLeft = 3;
            panel.style.marginRight = 3;
            panel.style.marginTop = 3;
            panel.style.marginBottom = 3;
            panel.style.borderTopWidth = 1;
            panel.style.borderBottomWidth = 1;
            panel.style.borderLeftWidth = 1;
            panel.style.borderRightWidth = 1;
            panel.style.borderTopColor = new Color(0.22f, 0.22f, 0.22f);
            panel.style.borderBottomColor = new Color(0.22f, 0.22f, 0.22f);
            panel.style.borderLeftColor = new Color(0.22f, 0.22f, 0.22f);
            panel.style.borderRightColor = new Color(0.22f, 0.22f, 0.22f);
            panel.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f);

            panel.Add(CreatePanelHeader(node));

            VisualElement body = new VisualElement();
            body.style.flexGrow = 1;
            body.style.flexBasis = 0;
            body.style.paddingLeft = 6;
            body.style.paddingRight = 6;
            body.style.paddingTop = 6;
            body.style.paddingBottom = 6;
            panel.Add(body);

            AddModuleGUI(body, node);
            return panel;
        }

        private VisualElement CreatePanelHeader(EditorToolHubLayoutNode node)
        {
            VisualElement header = new VisualElement();
            header.style.flexDirection = _layout.editMode ? FlexDirection.Column : FlexDirection.Row;
            header.style.alignItems = _layout.editMode ? Align.Stretch : Align.Center;
            header.style.minHeight = _layout.editMode ? 54 : 28;
            header.style.paddingLeft = 4;
            header.style.paddingRight = 4;
            header.style.paddingTop = 3;
            header.style.paddingBottom = 3;
            header.style.backgroundColor = new Color(0.20f, 0.20f, 0.20f);

            if (_layout.editMode)
            {
                VisualElement moduleRow = new VisualElement();
                moduleRow.style.flexDirection = FlexDirection.Row;
                moduleRow.style.alignItems = Align.Center;
                moduleRow.style.height = 23;
                header.Add(moduleRow);

                PopupField<string> modulePopup = CreateModulePopup(node);
                modulePopup.style.flexGrow = 1;
                modulePopup.style.flexShrink = 1;
                modulePopup.style.minWidth = 80;
                moduleRow.Add(modulePopup);

                VisualElement actionRow = new VisualElement();
                actionRow.style.flexDirection = FlexDirection.Row;
                actionRow.style.alignItems = Align.Center;
                actionRow.style.flexWrap = Wrap.Wrap;
                actionRow.style.marginTop = 3;
                header.Add(actionRow);

                actionRow.Add(CreateSmallButton("Split V", () => SplitPanel(node, EditorToolHubSplitDirection.Vertical), 64));
                actionRow.Add(CreateSmallButton("Split H", () => SplitPanel(node, EditorToolHubSplitDirection.Horizontal), 64));
                actionRow.Add(CreateSmallButton("Remove", () => RemovePanel(node), 62));
            }
            else
            {
                Label title = new Label(GetModuleTitle(node.moduleId));
                title.style.flexGrow = 1;
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                title.style.marginLeft = 4;
                header.Add(title);
            }

            return header;
        }

        private PopupField<string> CreateModulePopup(EditorToolHubLayoutNode node)
        {
            List<string> moduleIds = GetModuleIdChoices();
            string current = string.IsNullOrEmpty(node.moduleId) ? CNoneModuleId : node.moduleId;
            if (!moduleIds.Contains(current))
                moduleIds.Add(current);

            PopupField<string> popup = new PopupField<string>(moduleIds, current, GetModuleTitle, GetModuleTitle);
            popup.RegisterValueChangedCallback(evt =>
            {
                node.moduleId = evt.newValue == CNoneModuleId ? string.Empty : evt.newValue;
                SaveLayout();
                RefreshWorkspace();
            });

            return popup;
        }

        private VisualElement CreateSplitter(
            EditorToolHubLayoutNode node,
            VisualElement split,
            VisualElement first,
            VisualElement second)
        {
            VisualElement splitter = new VisualElement();
            bool vertical = node.splitDirection == EditorToolHubSplitDirection.Vertical;
            splitter.style.backgroundColor = new Color(0.10f, 0.10f, 0.10f);

            if (vertical)
            {
                splitter.style.width = 5;
                splitter.style.marginLeft = 1;
                splitter.style.marginRight = 1;
            }
            else
            {
                splitter.style.height = 5;
                splitter.style.marginTop = 1;
                splitter.style.marginBottom = 1;
            }

            splitter.RegisterCallback<MouseDownEvent>(evt =>
            {
                if (evt.button != 0)
                    return;

                splitter.CaptureMouse();
                evt.StopPropagation();
            });

            splitter.RegisterCallback<MouseMoveEvent>(evt =>
            {
                if (!splitter.HasMouseCapture())
                    return;

                Vector2 local = split.WorldToLocal(evt.mousePosition);
                float total = vertical ? split.contentRect.width : split.contentRect.height;
                if (total <= 0)
                    return;

                float value = vertical ? local.x / total : local.y / total;
                node.splitFraction = Mathf.Clamp(value, 0.1f, 0.9f);
                ApplySplitFraction(node, first, second);
                SaveLayout();
                evt.StopPropagation();
            });

            splitter.RegisterCallback<MouseUpEvent>(evt =>
            {
                if (!splitter.HasMouseCapture())
                    return;

                splitter.ReleaseMouse();
                SaveLayout();
                evt.StopPropagation();
            });

            return splitter;
        }

        private static void ApplySplitFraction(EditorToolHubLayoutNode node, VisualElement first, VisualElement second)
        {
            first.style.flexGrow = Mathf.Max(0.01f, node.splitFraction);
            first.style.flexBasis = 0;
            second.style.flexGrow = Mathf.Max(0.01f, 1f - node.splitFraction);
            second.style.flexBasis = 0;
        }

        private void AddModuleGUI(VisualElement body, EditorToolHubLayoutNode node)
        {
            if (string.IsNullOrEmpty(node.moduleId))
            {
                if (_layout.editMode)
                    body.Add(new HelpBox("Select a module for this panel.", HelpBoxMessageType.Info));
                return;
            }

            EditorToolModuleDescriptor descriptor = EditorToolModuleRegistry.Find(node.moduleId);
            if (descriptor == null)
            {
                body.Add(new HelpBox("Missing module: " + node.moduleId, HelpBoxMessageType.Warning));
                return;
            }

            try
            {
                IEditorToolModule module = descriptor.CreateInstance();
                _activeModules.Add(module);

                EditorToolModuleContext context = new EditorToolModuleContext(this, node.nodeId, SaveLayout, () => _layout.editMode);
                VisualElement content = module.CreateGUI(context);
                if (content == null)
                    content = new HelpBox("Module returned no GUI.", HelpBoxMessageType.Info);

                content.style.flexGrow = 1;
                body.Add(content);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                body.Add(new HelpBox("Failed to create module: " + descriptor.Title, HelpBoxMessageType.Error));
            }
        }

        private void SplitPanel(EditorToolHubLayoutNode node, EditorToolHubSplitDirection direction)
        {
            EditorToolHubLayoutNode existingPanel = node.Clone();
            existingPanel.kind = EditorToolHubLayoutNodeKind.Panel;
            existingPanel.first = null;
            existingPanel.second = null;
            existingPanel.EnsureValid();

            node.nodeId = Guid.NewGuid().ToString("N");
            node.kind = EditorToolHubLayoutNodeKind.Split;
            node.splitDirection = direction;
            node.splitFraction = 0.5f;
            node.moduleId = string.Empty;
            node.first = existingPanel;
            node.second = EditorToolHubLayoutNode.CreatePanel(string.Empty);
            node.EnsureValid();

            SaveLayout();
            RefreshWorkspace();
        }

        private void RemovePanel(EditorToolHubLayoutNode node)
        {
            if (_layout.root == node)
            {
                node.moduleId = string.Empty;
                SaveLayout();
                RefreshWorkspace();
                return;
            }

            if (RemoveNode(_layout.root, node))
            {
                _layout.root.EnsureValid();
                SaveLayout();
                RefreshWorkspace();
            }
        }

        private static bool RemoveNode(EditorToolHubLayoutNode current, EditorToolHubLayoutNode target)
        {
            if (current == null || !current.IsSplit)
                return false;

            if (current.first == target && current.second != null)
            {
                current.CopyFrom(current.second);
                return true;
            }

            if (current.second == target && current.first != null)
            {
                current.CopyFrom(current.first);
                return true;
            }

            return RemoveNode(current.first, target) || RemoveNode(current.second, target);
        }

        private static Button CreateSmallButton(string text, Action action, int width)
        {
            Button button = new Button(action) { text = text };
            button.style.width = width;
            button.style.height = 20;
            button.style.marginLeft = 2;
            button.style.marginRight = 0;
            button.style.paddingLeft = 0;
            button.style.paddingRight = 0;
            return button;
        }

        private static List<string> GetModuleIdChoices()
        {
            List<string> moduleIds = new List<string>();
            moduleIds.Add(CNoneModuleId);
            moduleIds.AddRange(EditorToolModuleRegistry.Modules.Select(module => module.Id));
            return moduleIds;
        }

        private static string GetModuleTitle(string moduleId)
        {
            if (string.IsNullOrEmpty(moduleId) || moduleId == CNoneModuleId)
                return "None";

            EditorToolModuleDescriptor descriptor = EditorToolModuleRegistry.Find(moduleId);
            if (descriptor == null)
                return moduleId;

            return descriptor.IsDynamic ? descriptor.Title + " (Window)" : descriptor.Title;
        }

        private void SaveLayout()
        {
            _layout = EditorToolHubLayoutStore.Save(_layout, _layoutAsset);
        }

        private void SaveLayoutAsset()
        {
            SaveLayout();

            if (_layoutAsset == null)
            {
                SaveLayoutAsAsset();
                return;
            }

            EditorToolHubLayoutStore.SaveAssetToDisk(_layoutAsset);
        }

        private void SaveLayoutAsAsset()
        {
            EditorToolHubLayoutAsset asset = EditorToolHubLayoutStore.SaveAsAsset(_layout);
            if (asset == null)
                return;

            SetLayoutAsset(asset);
        }

        private void SetLayoutAsset(EditorToolHubLayoutAsset asset)
        {
            if (_layoutAsset == asset)
                return;

            _layoutAsset = asset;
            EditorToolHubLayoutStore.SaveActiveAsset(_layoutAsset);
            _layout = EditorToolHubLayoutStore.Load(_layoutAsset);

            if (_layoutAssetField != null)
                _layoutAssetField.SetValueWithoutNotify(_layoutAsset);

            RefreshWorkspace();
        }

        private void ReleaseActiveModules()
        {
            for (int i = 0; i < _activeModules.Count; i++)
            {
                IEditorToolModuleLifecycle lifecycle = _activeModules[i] as IEditorToolModuleLifecycle;
                if (lifecycle == null)
                    continue;

                try
                {
                    lifecycle.OnDisable();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            _activeModules.Clear();
        }
    }
}
