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
using UnityEngine;

namespace FH.EditorToolHub
{
    public enum EditorToolHubLayoutNodeKind
    {
        Panel = 0,
        Split = 1,
    }

    public enum EditorToolHubSplitDirection
    {
        Vertical = 0,
        Horizontal = 1,
    }

    [Serializable]
    public sealed class EditorToolHubLayoutData
    {
        public int version = 2;
        public bool editMode = true;
        public EditorToolHubLayoutNode root;

        [SerializeField]
        private List<LegacyEditorToolHubPanelData> panels = new List<LegacyEditorToolHubPanelData>();

        public void EnsureValid()
        {
            if (root == null)
                root = CreateDefaultRoot();

            if (root != null)
                root.EnsureValid();
        }

        public EditorToolHubLayoutData Clone()
        {
            EditorToolHubLayoutData layout = new EditorToolHubLayoutData();
            layout.CopyFrom(this);
            return layout;
        }

        public void CopyFrom(EditorToolHubLayoutData other)
        {
            if (other == null)
            {
                version = 2;
                editMode = true;
                root = CreateDefaultRoot();
                panels = new List<LegacyEditorToolHubPanelData>();
                return;
            }

            version = Math.Max(2, other.version);
            editMode = other.editMode;
            root = other.root != null ? other.root.Clone() : CreateDefaultRoot();
            panels = new List<LegacyEditorToolHubPanelData>();
            EnsureValid();
        }

        internal void MigrateLegacyPanels()
        {
            if (root != null)
                return;

            if (panels == null || panels.Count == 0)
                return;

            List<EditorToolHubLayoutNode> legacyPanels = new List<EditorToolHubLayoutNode>();
            for (int i = 0; i < panels.Count; i++)
            {
                if (string.IsNullOrEmpty(panels[i].moduleId))
                    continue;

                legacyPanels.Add(EditorToolHubLayoutNode.CreatePanel(panels[i].moduleId));
            }

            if (legacyPanels.Count == 0)
                return;

            root = legacyPanels[0];
            for (int i = 1; i < legacyPanels.Count; i++)
                root = EditorToolHubLayoutNode.CreateSplit(EditorToolHubSplitDirection.Vertical, root, legacyPanels[i]);
        }

        internal static EditorToolHubLayoutNode CreateDefaultRoot()
        {
            List<EditorToolModuleDescriptor> defaultModules = EditorToolModuleRegistry.Modules
                .Where(module => module.IsDefault)
                .OrderBy(module => module.Order)
                .ToList();

            if (defaultModules.Count == 0)
                defaultModules = EditorToolModuleRegistry.Modules.Take(2).ToList();

            if (defaultModules.Count == 0)
                return EditorToolHubLayoutNode.CreatePanel(string.Empty);

            EditorToolHubLayoutNode first = EditorToolHubLayoutNode.CreatePanel(defaultModules[0].Id);
            if (defaultModules.Count == 1)
                return first;

            EditorToolHubLayoutNode second = EditorToolHubLayoutNode.CreatePanel(defaultModules[1].Id);
            return EditorToolHubLayoutNode.CreateSplit(EditorToolHubSplitDirection.Vertical, first, second);
        }
    }

    [Serializable]
    public sealed class EditorToolHubLayoutNode
    {
        public string nodeId;
        public EditorToolHubLayoutNodeKind kind;
        public EditorToolHubSplitDirection splitDirection;
        public float splitFraction = 0.5f;
        public string moduleId;
        public EditorToolHubLayoutNode first;
        public EditorToolHubLayoutNode second;

        public bool IsPanel { get { return kind == EditorToolHubLayoutNodeKind.Panel; } }
        public bool IsSplit { get { return kind == EditorToolHubLayoutNodeKind.Split; } }

        public static EditorToolHubLayoutNode CreatePanel(string moduleId)
        {
            return new EditorToolHubLayoutNode
            {
                nodeId = Guid.NewGuid().ToString("N"),
                kind = EditorToolHubLayoutNodeKind.Panel,
                moduleId = moduleId,
                splitFraction = 0.5f,
            };
        }

        public static EditorToolHubLayoutNode CreateSplit(
            EditorToolHubSplitDirection direction,
            EditorToolHubLayoutNode first,
            EditorToolHubLayoutNode second)
        {
            return new EditorToolHubLayoutNode
            {
                nodeId = Guid.NewGuid().ToString("N"),
                kind = EditorToolHubLayoutNodeKind.Split,
                splitDirection = direction,
                splitFraction = 0.5f,
                first = first,
                second = second,
            };
        }

        public void EnsureValid()
        {
            if (string.IsNullOrEmpty(nodeId))
                nodeId = Guid.NewGuid().ToString("N");

            splitFraction = Mathf.Clamp(splitFraction <= 0 ? 0.5f : splitFraction, 0.1f, 0.9f);

            if (kind == EditorToolHubLayoutNodeKind.Split)
            {
                if (first == null)
                    first = CreatePanel(string.Empty);

                if (second == null)
                    second = CreatePanel(string.Empty);

                first.EnsureValid();
                second.EnsureValid();
                moduleId = string.Empty;
            }
            else
            {
                kind = EditorToolHubLayoutNodeKind.Panel;
                first = null;
                second = null;
            }
        }

        public EditorToolHubLayoutNode Clone()
        {
            return new EditorToolHubLayoutNode
            {
                nodeId = nodeId,
                kind = kind,
                splitDirection = splitDirection,
                splitFraction = splitFraction,
                moduleId = moduleId,
                first = first != null ? first.Clone() : null,
                second = second != null ? second.Clone() : null,
            };
        }

        public void CopyFrom(EditorToolHubLayoutNode other)
        {
            if (other == null)
            {
                nodeId = Guid.NewGuid().ToString("N");
                kind = EditorToolHubLayoutNodeKind.Panel;
                moduleId = string.Empty;
                first = null;
                second = null;
                splitFraction = 0.5f;
                return;
            }

            nodeId = other.nodeId;
            kind = other.kind;
            splitDirection = other.splitDirection;
            splitFraction = other.splitFraction;
            moduleId = other.moduleId;
            first = other.first != null ? other.first.Clone() : null;
            second = other.second != null ? other.second.Clone() : null;
            EnsureValid();
        }
    }

    [Serializable]
    internal sealed class LegacyEditorToolHubPanelData
    {
        public string panelId = string.Empty;
        public string moduleId = string.Empty;
    }

    [CreateAssetMenu(fileName = "EditorToolHubLayout", menuName = "FancyHub/Editor Tool Hub/Layout")]
    public sealed class EditorToolHubLayoutAsset : ScriptableObject
    {
        [SerializeField]
        private EditorToolHubLayoutData _layout = new EditorToolHubLayoutData();

        public EditorToolHubLayoutData Layout
        {
            get
            {
                if (_layout == null)
                    _layout = new EditorToolHubLayoutData();

                _layout.MigrateLegacyPanels();
                _layout.EnsureValid();
                return _layout;
            }
        }

        internal void CopyFrom(EditorToolHubLayoutData layout)
        {
            if (_layout == null)
                _layout = new EditorToolHubLayoutData();

            _layout.CopyFrom(layout);
        }
    }

    internal static class EditorToolHubLayoutStore
    {
        private const string CEditorPrefsKey = "FH.EditorToolHub.Layout";
        private const string CLayoutAssetPathKey = "FH.EditorToolHub.LayoutAssetPath";

        public static EditorToolHubLayoutAsset LoadActiveAsset()
        {
            string path = EditorPrefs.GetString(CLayoutAssetPathKey, string.Empty);
            if (string.IsNullOrEmpty(path))
                return null;

            return AssetDatabase.LoadAssetAtPath<EditorToolHubLayoutAsset>(path);
        }

        public static void SaveActiveAsset(EditorToolHubLayoutAsset asset)
        {
            if (asset == null)
            {
                EditorPrefs.DeleteKey(CLayoutAssetPathKey);
                return;
            }

            string path = AssetDatabase.GetAssetPath(asset);
            if (string.IsNullOrEmpty(path))
                EditorPrefs.DeleteKey(CLayoutAssetPathKey);
            else
                EditorPrefs.SetString(CLayoutAssetPathKey, path);
        }

        public static EditorToolHubLayoutData Load(EditorToolHubLayoutAsset asset)
        {
            if (asset != null)
                return asset.Layout;

            return LoadTransient();
        }

        private static EditorToolHubLayoutData LoadTransient()
        {
            EditorToolHubLayoutData layout = null;
            string json = EditorPrefs.GetString(CEditorPrefsKey, string.Empty);

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    layout = JsonUtility.FromJson<EditorToolHubLayoutData>(json);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Failed to load EditorToolHub layout: " + ex.Message);
                }
            }

            if (layout == null)
                layout = CreateDefault();

            layout.MigrateLegacyPanels();
            layout.EnsureValid();
            return layout;
        }

        public static EditorToolHubLayoutData Save(EditorToolHubLayoutData layout, EditorToolHubLayoutAsset asset)
        {
            if (layout == null)
                layout = CreateDefault();

            layout.EnsureValid();

            if (asset != null)
            {
                if (!object.ReferenceEquals(layout, asset.Layout))
                {
                    asset.CopyFrom(layout);
                    layout = asset.Layout;
                }

                EditorUtility.SetDirty(asset);
                SaveActiveAsset(asset);
                return layout;
            }

            EditorPrefs.SetString(CEditorPrefsKey, JsonUtility.ToJson(layout));
            return layout;
        }

        public static EditorToolHubLayoutAsset SaveAsAsset(EditorToolHubLayoutData layout)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Editor Tool Hub Layout",
                "EditorToolHubLayout",
                "asset",
                "Choose where to save the layout asset.");

            if (string.IsNullOrEmpty(path))
                return null;

            EditorToolHubLayoutAsset asset = AssetDatabase.LoadAssetAtPath<EditorToolHubLayoutAsset>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EditorToolHubLayoutAsset>();
                asset.CopyFrom(layout);
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                asset.CopyFrom(layout);
                EditorUtility.SetDirty(asset);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = asset;
            SaveActiveAsset(asset);
            return asset;
        }

        public static void SaveAssetToDisk(EditorToolHubLayoutAsset asset)
        {
            if (asset == null)
                return;

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        public static void Clear()
        {
            EditorPrefs.DeleteKey(CEditorPrefsKey);
            EditorPrefs.DeleteKey(CLayoutAssetPathKey);
        }

        public static EditorToolHubLayoutData CreateDefault()
        {
            EditorToolHubLayoutData layout = new EditorToolHubLayoutData();
            layout.root = EditorToolHubLayoutData.CreateDefaultRoot();
            layout.EnsureValid();
            return layout;
        }
    }
}
