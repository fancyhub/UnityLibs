/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/11/12
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using FH.Ed;

namespace FH.AssetBundleBuilder.Ed
{
    public class AssetGraphViewWindow : UnityEditor.EditorWindow
    {
        private const string CGraphPathKey = "FH.AssetBundleBuilder.AssetGraphViewWindow.GraphPath";

        private enum ViewType
        {
            AssetBundles,
            LoadSimulation,
            ReverseDependencies,
        }

        private AssetGraph _graph;
        private string _graphPath;
        private string _loadError;
        private ViewType _view;
        private AssetGraphBundleView _bundleView;
        private AssetGraphLoadSimulationView _loadView;
        private AssetGraphReverseDependencyView _reverseView;

        [MenuItem("Tools/FancyHub/Res/AssetBundle/Asset Graph Viewer")]
        public static void Open()
        {
            AssetGraphViewWindow window = GetWindow<AssetGraphViewWindow>();
            window.titleContent = new GUIContent("Asset Graph");
            window.minSize = new Vector2(700, 400);
            window.Show();
        }

        private void OnEnable()
        {
            _bundleView = new AssetGraphBundleView();
            _loadView = new AssetGraphLoadSimulationView(Repaint);
            _reverseView = new AssetGraphReverseDependencyView(Repaint);
            _graphPath = EditorPrefs.GetString(CGraphPathKey, string.Empty);
            if (!string.IsNullOrEmpty(_graphPath) && File.Exists(_graphPath))
                LoadGraph(_graphPath);
        }

        private void OnGUI()
        {
            DrawToolbar();
            if (_graph == null)
            {
                EditorGUILayout.HelpBox(string.IsNullOrEmpty(_loadError)
                    ? "Please select a graph.json file."
                    : _loadError, MessageType.Info);
                return;
            }

            switch (_view)
            {
                case ViewType.AssetBundles:
                    _bundleView.OnGUI(_graph, position.height);
                    break;
                case ViewType.LoadSimulation:
                    _loadView.OnGUI(_graph, position.height);
                    break;
                case ViewType.ReverseDependencies:
                    _reverseView.OnGUI(_graph, position.height);
                    break;
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Open graph.json", EditorStyles.toolbarButton, GUILayout.Width(105)))
                {
                    string path = EditorUtility.OpenFilePanel("Select graph.json", Application.dataPath, "json");
                    if (!string.IsNullOrEmpty(path))
                        LoadGraph(path);
                }

                if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(65)) && !string.IsNullOrEmpty(_graphPath))
                    LoadGraph(_graphPath);

                GUILayout.Label(string.IsNullOrEmpty(_graphPath) ? "Not loaded" : _graphPath, EditorStyles.toolbarTextField);
            }

            GUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(8);
                string[] tabs = { "Assets & Bundles", "Load Simulation", "Reverse Dependencies" };
                int selectedTab = GUILayout.Toolbar((int)_view, tabs, EditorStyles.toolbarButton,
                    GUILayout.Height(28), GUILayout.ExpandWidth(true));
                if (selectedTab != (int)_view)
                {
                    _view = (ViewType)selectedTab;
                    GUI.FocusControl(null);
                }
                GUILayout.Space(8);
            }
            GUILayout.Space(6);
        }

        private void LoadGraph(string path)
        {
            try
            {
                AssetGraph graph = AssetGraph.LoadFromFile(path);
                if (graph == null || graph.Assets == null || graph.Bundles == null)
                    throw new InvalidDataException("graph.json is empty or has an invalid format.");
                _graphPath = path;
                _graph = graph;
                _loadError = null;
                _bundleView.SetGraph(_graph);
                _loadView.SetGraph(_graph);
                _reverseView.SetGraph(_graph);
                EditorPrefs.SetString(CGraphPathKey, path);
            }
            catch (Exception e)
            {
                _graph = null;
                _loadError = "Failed to load graph.json: " + e.Message;
                Debug.LogException(e);
            }
            Repaint();
        }
    }

    internal sealed class AssetGraphBundleView
    {
        private readonly AssetBundleTableView _tableView = new AssetBundleTableView();
        private string _assetFilter = string.Empty;
        private string _bundleFilter = string.Empty;
        private string _tagFilter = string.Empty;

        public void SetGraph(AssetGraph graph)
        {
            _tableView.SetData(graph);
        }

        public void OnGUI(AssetGraph graph, float windowHeight)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _assetFilter = EditorGUILayout.TextField("Asset", _assetFilter);
                _bundleFilter = EditorGUILayout.TextField("Bundle", _bundleFilter);
                _tagFilter = EditorGUILayout.TextField("Tag", _tagFilter);
                if (GUILayout.Button("Export CSV", GUILayout.Width(90)))
                    ExportCsv();
            }
            _tableView.AssetFilter = _assetFilter;
            _tableView.BundleFilter = _bundleFilter;
            _tableView.TagFilter = _tagFilter;
            _tableView.Reload();
            Rect rect = GUILayoutUtility.GetRect(0, Mathf.Max(280, windowHeight - 105), GUILayout.ExpandWidth(true));
            _tableView.OnGUI(rect);
        }

        private void ExportCsv()
        {
            string path = EditorUtility.SaveFilePanel("Export Assets and Bundles", Application.dataPath, "asset-bundles", "csv");
            if (string.IsNullOrEmpty(path))
                return;
            List<string[]> rows = new List<string[]> { new[] { "Asset Path", "Asset Bundles", "Bundle Tags" } };
            rows.AddRange(_tableView.GetFilteredRows(_assetFilter, _bundleFilter, _tagFilter).Select(r => new[] { r[0], r[1], r[2] }));
            AssetGraphCsv.Write(path, rows);
        }
    }

    internal sealed class AssetGraphLoadSimulationView
    {
        private readonly Action _repaint;
        private readonly HashSet<string> _roots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly AssetPathListView _candidatesView = new AssetPathListView("Exported Assets");
        private readonly AssetPathListView _rootsView = new AssetPathListView("Load Roots");
        private readonly AssetPathListView _dependenciesView = new AssetPathListView("All Dependencies (Including Self)");
        private AssetGraph _graph;

        public AssetGraphLoadSimulationView(Action repaint)
        {
            _repaint = repaint;
            _candidatesView.DoubleClickedCallback = AddRoot;
            _rootsView.DoubleClickedCallback = RemoveRoot;
        }

        public void SetGraph(AssetGraph graph)
        {
            _graph = graph;
            _roots.Clear();
            _candidatesView.Items = graph.Assets.Where(a => a.Export).Select(a => a.Path).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            _rootsView.Items = new List<string>();
            _dependenciesView.Items = new List<string>();
        }

        public void OnGUI(AssetGraph graph, float windowHeight)
        {
            EditorGUILayout.HelpBox("Double-click an asset on the left to add it to the load roots. The right list shows all dependencies, including the roots.", MessageType.None);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Export CSV", GUILayout.Width(90)))
                    ExportCsv();
            }
            Rect rect = GUILayoutUtility.GetRect(0, Mathf.Max(280, windowHeight - 140), GUILayout.ExpandWidth(true));
            Rect[] columns = SplitColumns(rect);
            _dependenciesView.Items = GetDependencies().ToList();
            _candidatesView.OnGUI(columns[0]);
            _rootsView.OnGUI(columns[1]);
            _dependenciesView.OnGUI(columns[2]);
        }

        private void AddRoot(string path)
        {
            _roots.Add(path);
            _rootsView.Items = _roots.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            _repaint();
        }

        private void RemoveRoot(string path)
        {
            _roots.Remove(path);
            _rootsView.Items = _roots.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            _repaint();
        }

        private IEnumerable<string> GetDependencies()
        {
            HashSet<int> result = new HashSet<int>();
            foreach (string path in _roots)
            {
                AssetGraph.Asset asset = _graph.Assets.FirstOrDefault(a => string.Equals(a.Path, path, StringComparison.OrdinalIgnoreCase));
                AddDependencies(asset, result);
            }
            return result.Select(i => _graph.Assets[i].Path).OrderBy(p => p, StringComparer.OrdinalIgnoreCase);
        }

        private void AddDependencies(AssetGraph.Asset asset, HashSet<int> result)
        {
            if (asset == null || !result.Add(asset.Index))
                return;
            if (asset.Deps == null)
                return;
            foreach (int index in asset.Deps)
            {
                if (index >= 0 && index < _graph.Assets.Count)
                    AddDependencies(_graph.Assets[index], result);
            }
        }

        private void ExportCsv()
        {
            string path = EditorUtility.SaveFilePanel("Export Load Simulation", Application.dataPath, "load-simulation", "csv");
            if (string.IsNullOrEmpty(path))
                return;
            List<string> candidates = _graph.Assets.Where(a => a.Export).Select(a => a.Path).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            List<string> roots = _roots.OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            List<string> deps = GetDependencies().ToList();
            List<string[]> rows = new List<string[]> { new[] { "Exported Assets", "Load Roots", "All Dependencies (Including Self)" } };
            int count = Mathf.Max(candidates.Count, Mathf.Max(roots.Count, deps.Count));
            for (int i = 0; i < count; i++)
                rows.Add(new[] { ValueAt(candidates, i), ValueAt(roots, i), ValueAt(deps, i) });
            AssetGraphCsv.Write(path, rows);
        }

        private static string ValueAt(List<string> values, int index)
        {
            return index < values.Count ? values[index] : string.Empty;
        }

        private static Rect[] SplitColumns(Rect rect)
        {
            float gap = 4f;
            float width = rect.width / 3f;
            return new[]
            {
                new Rect(rect.x, rect.y, width - gap, rect.height),
                new Rect(rect.x + width, rect.y, width - gap, rect.height),
                new Rect(rect.x + width * 2f, rect.y, width - gap, rect.height),
            };
        }
    }

    internal sealed class AssetGraphReverseDependencyView
    {
        private readonly Action _repaint;
        private readonly AssetPathListView _assetsView = new AssetPathListView("All Assets");
        private readonly AssetPathListView _directView = new AssetPathListView("Direct Dependents");
        private readonly AssetPathListView _recursiveView = new AssetPathListView("Recursive Dependents");
        private AssetGraph _graph;
        private string _selectedPath;

        public AssetGraphReverseDependencyView(Action repaint)
        {
            _repaint = repaint;
            _assetsView.SelectionChangedCallback = path => { _selectedPath = path; _repaint(); };
        }

        public void SetGraph(AssetGraph graph)
        {
            _graph = graph;
            _selectedPath = null;
            _assetsView.Items = graph.Assets.Select(a => a.Path).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            _directView.Items = new List<string>();
            _recursiveView.Items = new List<string>();
        }

        public void OnGUI(AssetGraph graph, float windowHeight)
        {
            EditorGUILayout.HelpBox("Select an asset on the left. The middle list shows its direct dependents; the right list shows all recursive dependents.", MessageType.None);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Export CSV", GUILayout.Width(90)))
                    ExportCsv();
            }
            Rect rect = GUILayoutUtility.GetRect(0, Mathf.Max(280, windowHeight - 140), GUILayout.ExpandWidth(true));
            Rect[] columns = SplitColumns(rect);
            _directView.Items = GetReverseDependencies(false).ToList();
            _recursiveView.Items = GetReverseDependencies(true).ToList();
            _assetsView.OnGUI(columns[0]);
            _directView.OnGUI(columns[1]);
            _recursiveView.OnGUI(columns[2]);
        }

        private IEnumerable<string> GetReverseDependencies(bool recursive)
        {
            AssetGraph.Asset target = _graph.Assets.FirstOrDefault(a => string.Equals(a.Path, _selectedPath, StringComparison.OrdinalIgnoreCase));
            if (target == null)
                return new string[0];
            HashSet<int> result = new HashSet<int>();
            Queue<int> queue = new Queue<int>();
            queue.Enqueue(target.Index);
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                foreach (AssetGraph.Asset candidate in _graph.Assets)
                {
                    if (candidate.Deps == null || !candidate.Deps.Contains(current) || !result.Add(candidate.Index))
                        continue;
                    if (recursive)
                        queue.Enqueue(candidate.Index);
                }
            }
            return result.Select(i => _graph.Assets[i].Path).OrderBy(p => p, StringComparer.OrdinalIgnoreCase);
        }

        private void ExportCsv()
        {
            string path = EditorUtility.SaveFilePanel("Export Reverse Dependencies", Application.dataPath, "reverse-dependencies", "csv");
            if (string.IsNullOrEmpty(path))
                return;
            List<string> assets = _graph.Assets.Select(a => a.Path).OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            List<string> direct = GetReverseDependencies(false).ToList();
            List<string> recursive = GetReverseDependencies(true).ToList();
            List<string[]> rows = new List<string[]> { new[] { "All Assets", "Direct Dependents", "Recursive Dependents" } };
            int count = Mathf.Max(assets.Count, Mathf.Max(direct.Count, recursive.Count));
            for (int i = 0; i < count; i++)
                rows.Add(new[] { ValueAt(assets, i), ValueAt(direct, i), ValueAt(recursive, i) });
            AssetGraphCsv.Write(path, rows);
        }

        private static string ValueAt(List<string> values, int index)
        {
            return index < values.Count ? values[index] : string.Empty;
        }

        private static Rect[] SplitColumns(Rect rect)
        {
            float gap = 4f;
            float width = rect.width / 3f;
            return new[]
            {
                new Rect(rect.x, rect.y, width - gap, rect.height),
                new Rect(rect.x + width, rect.y, width - gap, rect.height),
                new Rect(rect.x + width * 2f, rect.y, width - gap, rect.height),
            };
        }
    }

    internal sealed class AssetBundleTableView : TreeView
    {
        private sealed class Row : TreeViewItem
        {
            public string Path;
            public string Bundles;
            public string Tags;
        }

        private readonly List<Row> _rows = new List<Row>();
        private AssetGraph _graph;
        public string AssetFilter;
        public string BundleFilter;
        public string TagFilter;

        public AssetBundleTableView() : base(new TreeViewState(), CreateHeader())
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.sortingChanged += header => Reload();
        }

        public override void OnGUI(Rect rect)
        {
            // Recalculate column widths from the actual TreeView width.
            float availableWidth = Mathf.Max(240f, rect.width - 2f);
            MultiColumnHeaderState.Column[] columns = multiColumnHeader.state.columns;
            columns[0].width = availableWidth * 0.52f;
            columns[1].width = availableWidth * 0.24f;
            columns[2].width = availableWidth - columns[0].width - columns[1].width;
            base.OnGUI(rect);
        }

        public void SetData(AssetGraph graph)
        {
            _graph = graph;
            _rows.Clear();
            foreach (AssetGraph.Asset asset in graph.Assets)
            {
                List<AssetGraph.Bundle> bundles = graph.Bundles.Where(b => b.Assets != null && b.Assets.Contains(asset.Index)).ToList();
                _rows.Add(new Row { id = _rows.Count, depth = 0, Path = asset.Path,
                    Bundles = string.Join(", ", bundles.Select(b => b.Name).ToArray()),
                    Tags = string.Join(", ", bundles.SelectMany(b => b.Tags ?? new string[0]).Distinct().ToArray()) });
            }
            Reload();
        }

        public List<string[]> GetFilteredRows(string assetFilter, string bundleFilter, string tagFilter)
        {
            return _rows.Where(r => Contains(r.Path, assetFilter) && Contains(r.Bundles, bundleFilter) && Contains(r.Tags, tagFilter))
                .Select(r => new[] { r.Path, r.Bundles, r.Tags }).ToList();
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1) { children = new List<TreeViewItem>() };
            root.children.AddRange(_rows);
            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return _rows.Where(r => Contains(r.Path, AssetFilter) && Contains(r.Bundles, BundleFilter) && Contains(r.Tags, TagFilter)).Cast<TreeViewItem>().ToList();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            Row row = (Row)args.item;
            DrawCell(args.GetCellRect(0), row.Path);
            DrawCell(args.GetCellRect(1), row.Bundles);
            DrawCell(args.GetCellRect(2), row.Tags);
        }

        private static void DrawCell(Rect rect, string value)
        {
            GUI.Label(rect, value);
            AssetGraphClipboard.HandleContextClick(rect, value);
        }

        private static MultiColumnHeader CreateHeader()
        {
            MultiColumnHeaderState.Column[] columns = new MultiColumnHeaderState.Column[3];
            string[] names = { "Asset Path", "Asset Bundles", "Bundle Tags" };
            for (int i = 0; i < columns.Length; i++)
                columns[i] = new MultiColumnHeaderState.Column { headerContent = new GUIContent(names[i]), autoResize = true, minWidth = 80 };
            return new MultiColumnHeader(new MultiColumnHeaderState(columns));
        }

        private static bool Contains(string value, string filter)
        {
            return string.IsNullOrEmpty(filter) || (!string.IsNullOrEmpty(value) && value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }

    internal sealed class AssetPathListView : TreeView
    {
        private List<string> _items = new List<string>();
        public Action<string> SelectionChangedCallback;
        public Action<string> DoubleClickedCallback;
        public List<string> Items { set { _items = value ?? new List<string>(); Reload(); } }

        public AssetPathListView(string title) : base(new TreeViewState(), CreateHeader(title))
        {
            rowHeight = 20;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
        }

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1) { children = new List<TreeViewItem>() };
            for (int i = 0; i < _items.Count; i++)
                root.children.Add(new TreeViewItem(i, 0, _items[i]));
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            GUI.Label(args.rowRect, args.item.displayName);
            AssetGraphClipboard.HandleContextClick(args.rowRect, args.item.displayName);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds != null && selectedIds.Count > 0 && selectedIds[0] >= 0 && selectedIds[0] < _items.Count)
                SelectionChangedCallback?.Invoke(_items[selectedIds[0]]);
        }

        protected override void DoubleClickedItem(int id)
        {
            if (id >= 0 && id < _items.Count)
                DoubleClickedCallback?.Invoke(_items[id]);
        }

        private static MultiColumnHeader CreateHeader(string title)
        {
            return new MultiColumnHeader(new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column { headerContent = new GUIContent(title), autoResize = true, minWidth = 80 }
            }));
        }
    }

    internal static class AssetGraphCsv
    {
        public static void Write(string path, IEnumerable<string[]> rows)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string[] row in rows)
            {
                builder.AppendLine(string.Join(",", row.Select(Escape).ToArray()));
            }
            File.WriteAllText(path, builder.ToString(), new UTF8Encoding(true));
            EditorUtility.RevealInFinder(path);
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            string escaped = value.Replace("\"", "\"\"");
            return escaped.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0 ? "\"" + escaped + "\"" : escaped;
        }
    }

    internal static class AssetGraphClipboard
    {
        public static void HandleContextClick(Rect rect, string value)
        {
            Event current = Event.current;
            if (current.type != EventType.ContextClick || !rect.Contains(current.mousePosition))
                return;

            GenericMenu menu = new GenericMenu();
            if (string.IsNullOrEmpty(value))
                menu.AddDisabledItem(new GUIContent("Copy"));
            else
                menu.AddItem(new GUIContent("Copy"), false, () => GUIUtility.systemCopyBuffer = value);
            menu.ShowAsContext();
            current.Use();
        }
    }
}
