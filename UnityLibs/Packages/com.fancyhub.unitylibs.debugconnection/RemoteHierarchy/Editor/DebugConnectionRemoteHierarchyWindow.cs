/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace FH
{
    public sealed class DebugConnectionRemoteHierarchyWindow : EditorWindow
    {
        private const int RootId = 0;

        private readonly Dictionary<int, NodeState> _Nodes = new Dictionary<int, NodeState>();
        private readonly List<int> _RootIds = new List<int>();

        private Vector2 _TreeScroll;
        private Vector2 _InspectorScroll;
        private int _SelectedId;
        private DebugConnectionRemoteHierarchyComponentsResponse _SelectedComponents;
        private string _SearchText = string.Empty;
        private string _LastInfo = string.Empty;
        private bool _IncludePublicFields = true;
        private bool _IncludeSerializableFields = true;
        private bool _IncludePublicProperties;

        [MenuItem("Tools/FancyHub/Debug Connection/Remote Hierarchy")]
        public static void ShowWindow()
        {
            GetWindow<DebugConnectionRemoteHierarchyWindow>("Remote Hierarchy");
        }

        private void OnEnable()
        {
            DebugConnectionClient.Register(DebugConnectionRemoteHierarchyProtocol.ChildrenResponse, OnChildrenResponse);
            DebugConnectionClient.Register(DebugConnectionRemoteHierarchyProtocol.ComponentsResponse, OnComponentsResponse);
            DebugConnectionClient.Connected += OnConnectionChanged;
            DebugConnectionClient.Disconnected += OnConnectionChanged;
        }

        private void OnDisable()
        {
            DebugConnectionClient.Unregister(DebugConnectionRemoteHierarchyProtocol.ChildrenResponse, OnChildrenResponse);
            DebugConnectionClient.Unregister(DebugConnectionRemoteHierarchyProtocol.ComponentsResponse, OnComponentsResponse);
            DebugConnectionClient.Connected -= OnConnectionChanged;
            DebugConnectionClient.Disconnected -= OnConnectionChanged;
        }

        private void OnGUI()
        {
            DrawToolbar();

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawTreePanel();
                DrawInspectorPanel();
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                using (new EditorGUI.DisabledScope(!DebugConnectionClient.IsConnected))
                {
                    if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                        RefreshRoots();
                }

                GUILayout.Space(8);
                DebugConnectionWindow.DrawOnToolBar();                
                GUILayout.Space(8);
                GUILayout.Label("Search", GUILayout.Width(44));
                _SearchText = GUILayout.TextField(_SearchText, EditorStyles.toolbarSearchField, GUILayout.MinWidth(120));

                GUILayout.FlexibleSpace();
                if (!string.IsNullOrEmpty(_LastInfo))
                    GUILayout.Label(_LastInfo, EditorStyles.miniLabel);
            }
        }

        private void DrawTreePanel()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(Mathf.Max(280, position.width * 0.45f))))
            {
                EditorGUILayout.LabelField("Hierarchy", EditorStyles.boldLabel);

                if (!DebugConnectionClient.IsConnected)
                {
                    EditorGUILayout.HelpBox("No debug connection.", MessageType.Info);
                    return;
                }

                _TreeScroll = EditorGUILayout.BeginScrollView(_TreeScroll);
                if (_RootIds.Count == 0)
                {
                    EditorGUILayout.HelpBox("Click Refresh to load remote roots.", MessageType.Info);
                }
                else
                {
                    for (int i = 0; i < _RootIds.Count; i++)
                        DrawNode(_RootIds[i], 0);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawNode(int instanceId, int depth)
        {
            if (!_Nodes.TryGetValue(instanceId, out NodeState state) || state.Node == null)
                return;

            if (!MatchesSearch(state))
            {
                bool childMatches = false;
                for (int i = 0; i < state.Children.Count; i++)
                {
                    if (_Nodes.TryGetValue(state.Children[i], out NodeState child) && MatchesSearch(child))
                    {
                        childMatches = true;
                        break;
                    }
                }

                if (!childMatches && !string.IsNullOrEmpty(_SearchText))
                    return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(depth * 16);

                if (state.Node.ChildCount > 0)
                {
                    Rect foldoutRect = GUILayoutUtility.GetRect(14, EditorGUIUtility.singleLineHeight, GUILayout.Width(14));
                    bool expanded = EditorGUI.Foldout(foldoutRect, state.Expanded, string.Empty);
                    if (expanded != state.Expanded)
                    {
                        state.Expanded = expanded;
                        if (expanded && !state.ChildrenLoaded)
                            RequestChildren(instanceId);
                    }
                }
                else
                {
                    GUILayout.Space(14);
                }

                GUIStyle style = state.Node.IsScene || instanceId == _SelectedId ? EditorStyles.boldLabel : EditorStyles.label;
                string prefix = state.Node.ActiveInHierarchy ? string.Empty : "[inactive] ";
                string label = state.Node.IsScene ? state.Node.Name : prefix + state.Node.Name;
                if (GUILayout.Button(label, style))
                    SelectNode(instanceId);
            }

            if (!state.Expanded)
                return;

            if (!state.ChildrenLoaded)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Space((depth + 1) * 16 + 14);
                    EditorGUILayout.LabelField("Loading...", EditorStyles.miniLabel);
                }
                return;
            }

            for (int i = 0; i < state.Children.Count; i++)
                DrawNode(state.Children[i], depth + 1);
        }

        private void DrawInspectorPanel()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Inspector", EditorStyles.boldLabel);

                if (_SelectedId == 0)
                {
                    EditorGUILayout.HelpBox("Select a remote GameObject.", MessageType.Info);
                    return;
                }

                DrawInspectorOptions();

                NodeState state;
                if (_Nodes.TryGetValue(_SelectedId, out state) && state.Node != null)
                {
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.LabelField("Name", state.Node.Name);
                        EditorGUILayout.LabelField("Path", state.Node.Path);
                        EditorGUILayout.LabelField("Scene", state.Node.SceneName);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("Active", GUILayout.Width(145));
                            using (new EditorGUI.DisabledScope(state.Node.IsScene || !DebugConnectionClient.IsConnected))
                            {
                                bool active = EditorGUILayout.Toggle(state.Node.ActiveSelf);
                                if (active != state.Node.ActiveSelf)
                                {
                                    state.Node.ActiveSelf = active;
                                    RequestSetActive(state.Node.InstanceId, active);
                                }
                            }
                            GUILayout.Label("In Hierarchy: " + state.Node.ActiveInHierarchy, EditorStyles.miniLabel);
                        }
                        EditorGUILayout.LabelField("Children", state.Node.ChildCount.ToString());
                    }
                }

                _InspectorScroll = EditorGUILayout.BeginScrollView(_InspectorScroll);
                if (_SelectedComponents == null || _SelectedComponents.InstanceId != _SelectedId)
                {
                    EditorGUILayout.HelpBox("Loading components...", MessageType.Info);
                }
                else if (_SelectedComponents.Components == null || _SelectedComponents.Components.Length == 0)
                {
                    EditorGUILayout.HelpBox("No components.", MessageType.Info);
                }
                else
                {
                    for (int i = 0; i < _SelectedComponents.Components.Length; i++)
                        DrawComponent(_SelectedComponents.Components[i]);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void DrawInspectorOptions()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginChangeCheck();
                _IncludeSerializableFields = GUILayout.Toggle(_IncludeSerializableFields, "Serialized", EditorStyles.toolbarButton, GUILayout.Width(76));
                _IncludePublicFields = GUILayout.Toggle(_IncludePublicFields, "Public Fields", EditorStyles.toolbarButton, GUILayout.Width(92));
                _IncludePublicProperties = GUILayout.Toggle(_IncludePublicProperties, "Properties", EditorStyles.toolbarButton, GUILayout.Width(82));
                bool changed = EditorGUI.EndChangeCheck();

                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(!DebugConnectionClient.IsConnected || _SelectedId == 0))
                {
                    if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)) || changed)
                    {
                        _SelectedComponents = null;
                        RequestComponents(_SelectedId);
                    }
                }
            }
        }

        private void DrawComponent(DebugConnectionRemoteHierarchyComponent component)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(component.TypeName, EditorStyles.boldLabel);
                    if (component.HasEnabled)
                    {
                        using (new EditorGUI.DisabledScope(!DebugConnectionClient.IsConnected))
                        {
                            bool enabled = EditorGUILayout.Toggle(component.Enabled, GUILayout.Width(18));
                            if (enabled != component.Enabled)
                            {
                                component.Enabled = enabled;
                                RequestSetComponentEnabled(_SelectedId, component.ComponentIndex, enabled);
                            }
                        }
                    }
                }

                if (component.Properties == null)
                    return;

                for (int i = 0; i < component.Properties.Length; i++)
                {
                    DebugConnectionRemoteHierarchyProperty property = component.Properties[i];
                    EditorGUILayout.LabelField(property.Name, property.Value);
                }
            }
        }

        private bool MatchesSearch(NodeState state)
        {
            if (string.IsNullOrEmpty(_SearchText))
                return true;

            return state.Node != null
                && !string.IsNullOrEmpty(state.Node.Name)
                && state.Node.Name.IndexOf(_SearchText, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void RefreshRoots()
        {
            _RootIds.Clear();
            _Nodes.Clear();
            _SelectedId = 0;
            _SelectedComponents = null;
            RequestChildren(RootId);
        }

        private void SelectNode(int instanceId)
        {
            _SelectedId = instanceId;
            _SelectedComponents = null;
            RequestComponents(instanceId);
            Repaint();
        }

        private void RequestChildren(int instanceId)
        {
            Send(DebugConnectionRemoteHierarchyProtocol.RequestChildren, new DebugConnectionRemoteHierarchyRequest
            {
                InstanceId = instanceId,
            });
            _LastInfo = "Requested children";
        }

        private void RequestComponents(int instanceId)
        {
            Send(DebugConnectionRemoteHierarchyProtocol.RequestComponents, new DebugConnectionRemoteHierarchyRequest
            {
                InstanceId = instanceId,
                IncludePublicFields = _IncludePublicFields,
                IncludeSerializableFields = _IncludeSerializableFields,
                IncludePublicProperties = _IncludePublicProperties,
            });
            _LastInfo = "Requested components";
        }

        private void RequestSetActive(int instanceId, bool active)
        {
            Send(DebugConnectionRemoteHierarchyProtocol.SetGameObjectActive, new DebugConnectionRemoteHierarchySetActiveRequest
            {
                InstanceId = instanceId,
                Active = active,
            });
            _LastInfo = "Set active";
        }

        private void RequestSetComponentEnabled(int instanceId, int componentIndex, bool enabled)
        {
            Send(DebugConnectionRemoteHierarchyProtocol.SetComponentEnabled, new DebugConnectionRemoteHierarchySetComponentEnabledRequest
            {
                InstanceId = instanceId,
                ComponentIndex = componentIndex,
                Enabled = enabled,
                IncludePublicFields = _IncludePublicFields,
                IncludeSerializableFields = _IncludeSerializableFields,
                IncludePublicProperties = _IncludePublicProperties,
            });
            _LastInfo = "Set component enabled";
        }

        private static void Send(Guid messageId, object payload)
        {
            string json = JsonUtility.ToJson(payload);
            DebugConnectionClient.Send(messageId, Encoding.UTF8.GetBytes(json));
        }

        private void OnChildrenResponse(DebugConnectionMessageEventArgs args)
        {
            DebugConnectionRemoteHierarchyChildrenResponse response = FromJson<DebugConnectionRemoteHierarchyChildrenResponse>(args.Data);
            if (response == null)
                return;

            if (response.ParentId == RootId)
                _RootIds.Clear();

            NodeState parent = null;
            if (response.ParentId != RootId)
            {
                if (!_Nodes.TryGetValue(response.ParentId, out parent))
                    return;

                parent.Children.Clear();
                parent.ChildrenLoaded = true;
            }

            DebugConnectionRemoteHierarchyNode[] nodes = response.Nodes ?? Array.Empty<DebugConnectionRemoteHierarchyNode>();
            for (int i = 0; i < nodes.Length; i++)
            {
                DebugConnectionRemoteHierarchyNode node = nodes[i];
                NodeState state;
                if (!_Nodes.TryGetValue(node.InstanceId, out state))
                {
                    state = new NodeState();
                    _Nodes[node.InstanceId] = state;
                }

                state.Node = node;
                if (response.ParentId == RootId)
                    _RootIds.Add(node.InstanceId);
                else
                    parent.Children.Add(node.InstanceId);
            }

            _LastInfo = "Loaded " + nodes.Length + " node(s)";
            Repaint();
        }

        private void OnComponentsResponse(DebugConnectionMessageEventArgs args)
        {
            DebugConnectionRemoteHierarchyComponentsResponse response = FromJson<DebugConnectionRemoteHierarchyComponentsResponse>(args.Data);
            if (response == null)
                return;

            _SelectedComponents = response;
            _LastInfo = "Loaded components";
            Repaint();
        }

        private void OnConnectionChanged()
        {
            Repaint();
        }

        private static T FromJson<T>(byte[] data) where T : class
        {
            if (data == null || data.Length == 0)
                return null;

            try
            {
                return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(data));
            }
            catch
            {
                return null;
            }
        }

        private sealed class NodeState
        {
            public DebugConnectionRemoteHierarchyNode Node;
            public bool Expanded;
            public bool ChildrenLoaded;
            public readonly List<int> Children = new List<int>();
        }
    }
}
