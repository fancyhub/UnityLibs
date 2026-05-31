using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FH
{
    public sealed class ResServiceDebugViewer : EditorWindow
    {
        private const float CIdWidth = 56;
        private const float CTypeWidth = 110;
        private const float CCountWidth = 64;
        private const float CStatusWidth = 110;
        private const float CBundleWidth = 180;
        private const float CPathTypeWidth = 150;

        private EdResServiceSnapshot _SnapshotService;
        private Vector2 _ScrollPos;
        private string _Filter = string.Empty;
        private bool _ShowResItems = true;
        private bool _ShowBundleItems = true;
        private bool _ShowBundleManifest;
        private string _RequestError = string.Empty;

        [MenuItem("Tools/Res Service Debug Viewer")]
        public static void Open()
        {
            ResServiceDebugViewer window = GetWindow<ResServiceDebugViewer>();
            window.titleContent = new GUIContent("Res Service Debug");
            window.minSize = new Vector2(760, 420);
            window.Show();
        }

        private void OnEnable()
        {
            _SnapshotService = new EdResServiceSnapshot();
            _SnapshotService.SnapshotReceived += OnSnapshotReceived;
            DebugConnectionEditorClient.Connected += OnConnectionChanged;
            DebugConnectionEditorClient.Disconnected += OnConnectionChanged;
            DebugConnectionEditorClient.Error += OnConnectionError;
            DebugConnectionEditorClient.TargetInfoChanged += OnTargetInfoChanged;
        }

        private void OnDisable()
        {
            DebugConnectionEditorClient.Connected -= OnConnectionChanged;
            DebugConnectionEditorClient.Disconnected -= OnConnectionChanged;
            DebugConnectionEditorClient.Error -= OnConnectionError;
            DebugConnectionEditorClient.TargetInfoChanged -= OnTargetInfoChanged;

            if (_SnapshotService == null)
                return;

            _SnapshotService.SnapshotReceived -= OnSnapshotReceived;
            _SnapshotService.Dispose();
            _SnapshotService = null;
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(6);
            DrawTarget();
            EditorGUILayout.Space(6);
            DrawSnapshot();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                using (new EditorGUI.DisabledScope(!DebugConnectionEditorClient.IsConnected))
                {
                    if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
                        RequestSnapshot();
                }


                DebugConnectionPopupContent.DrawGUI(Repaint);
               

                GUILayout.Space(8);
                _Filter = GUILayout.TextField(_Filter, GUI.skin.FindStyle("ToolbarSeachTextField") ?? EditorStyles.toolbarTextField, GUILayout.MinWidth(180));
                if (GUILayout.Button("x", EditorStyles.toolbarButton, GUILayout.Width(24)))
                    _Filter = string.Empty;

                GUILayout.FlexibleSpace();
                _ShowResItems = GUILayout.Toggle(_ShowResItems, "Res", EditorStyles.toolbarButton, GUILayout.Width(56));
                _ShowBundleItems = GUILayout.Toggle(_ShowBundleItems, "Bundles", EditorStyles.toolbarButton, GUILayout.Width(76));
                _ShowBundleManifest = GUILayout.Toggle(_ShowBundleManifest, "Manifest", EditorStyles.toolbarButton, GUILayout.Width(84));
            }
        }

        private void DrawTarget()
        {
            DebugConnectionTargetInfo target = DebugConnectionEditorClient.GetTargetInfo();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (!target.IsConnected)
                {
                    EditorGUILayout.LabelField("Connection", DebugConnectionEditorClient.IsRunning ? "Connecting" : "Disconnected");
                    DrawError(DebugConnectionEditorClient.LastError);
                    return;
                }

                EditorGUILayout.LabelField("Connection", string.IsNullOrEmpty(target.DisplayName) ? target.RemoteEndPoint : target.DisplayName);
                EditorGUILayout.LabelField("Host", target.Host + ":" + target.Port);
                EditorGUILayout.LabelField("Remote", target.RemoteEndPoint);
            }
        }

        private void DrawSnapshot()
        {
            SnapShot snapshot = _SnapshotService == null ? null : _SnapshotService.LatestSnapshot;
            if (snapshot == null)
            {
                DrawError(_RequestError);
                DrawError(_SnapshotService == null ? string.Empty : _SnapshotService.LastError);
                EditorGUILayout.HelpBox("No snapshot.", MessageType.Info);
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Frame", snapshot.FrameCount.ToString());
                EditorGUILayout.LabelField("Updated UTC", _SnapshotService.LatestSnapshotUtc.ToString("yyyy-MM-dd HH:mm:ss"));
                EditorGUILayout.LabelField("Res Items", GetCount(snapshot.ResItems).ToString());
                EditorGUILayout.LabelField("Bundle Items", GetCount(snapshot.BundleItems).ToString());
                EditorGUILayout.LabelField("Manifest Bundles", GetManifestCount(snapshot.BundleManifest).ToString());
            }

            DrawError(_RequestError);
            DrawError(_SnapshotService.LastError);
            DrawError(snapshot.Error);

            _ScrollPos = EditorGUILayout.BeginScrollView(_ScrollPos);
            if (_ShowResItems)
                DrawResItems(snapshot);
            if (_ShowBundleItems)
                DrawBundleItems(snapshot);
            if (_ShowBundleManifest)
                DrawBundleManifest(snapshot.BundleManifest);
            EditorGUILayout.EndScrollView();
        }

        private void DrawResItems(SnapShot snapshot)
        {
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Res Items", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Id", EditorStyles.toolbarButton, GUILayout.Width(CIdWidth));
                GUILayout.Label("Type", EditorStyles.toolbarButton, GUILayout.Width(CTypeWidth));
                GUILayout.Label("Users", EditorStyles.toolbarButton, GUILayout.Width(CCountWidth));
                GUILayout.Label("Status", EditorStyles.toolbarButton, GUILayout.Width(CStatusWidth));
                //GUILayout.Label("Path Type", EditorStyles.toolbarButton, GUILayout.Width(CPathTypeWidth));
                GUILayout.Label("Bundle", EditorStyles.toolbarButton, GUILayout.Width(CBundleWidth));
                GUILayout.Label("Path", EditorStyles.toolbarButton);
            }

            if (snapshot.ResItems == null || snapshot.ResItems.Count == 0)
            {
                EditorGUILayout.HelpBox("Empty.", MessageType.Info);
                return;
            }

            int shownCount = 0;
            for (int i = 0; i < snapshot.ResItems.Count; i++)
            {
                ResSnapShotItem item = snapshot.ResItems[i];
                if (!MatchResItem(item))
                    continue;

                shownCount++;
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(item.Id.ToString(), GUILayout.Width(CIdWidth));
                    GUILayout.Label(item.ResType.ToString(), GUILayout.Width(CTypeWidth));
                    GUILayout.Label(item.UserCount.ToString(), GUILayout.Width(CCountWidth));
                    GUILayout.Label(item.InstStatus.ToString(), GUILayout.Width(CStatusWidth));
                    //GUILayout.Label(item.PathTypeMask.ToString(), GUILayout.Width(CPathTypeWidth));
                    GUILayout.Label(item.BundleName ?? string.Empty, GUILayout.Width(CBundleWidth));
                    GUILayout.Label(item.Path ?? string.Empty);
                }
            }

            DrawFilteredEmpty(shownCount);
        }  

        private void DrawBundleItems(SnapShot snapshot)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Bundle Items", EditorStyles.boldLabel);
            DrawBundleHeader("Bundle", "Load", "File");

            if (snapshot.BundleItems == null || snapshot.BundleItems.Count == 0)
            {
                EditorGUILayout.HelpBox("Empty.", MessageType.Info);
                return;
            }

            int shownCount = 0;
            for (int i = 0; i < snapshot.BundleItems.Count; i++)
            {
                BundleSnapshotItem item = snapshot.BundleItems[i];
                if (!MatchBundleItem(item))
                    continue;

                shownCount++;
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(item.BundleName ?? string.Empty, GUILayout.Width(CBundleWidth));
                    GUILayout.Label(item.BundleStatus.ToString(), GUILayout.Width(CTypeWidth));
                    GUILayout.Label(item.FileStatus.ToString(), GUILayout.Width(CTypeWidth));
                }
            }

            DrawFilteredEmpty(shownCount);
        }

        private void DrawBundleManifest(BundleManifest manifest)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Bundle Manifest", EditorStyles.boldLabel);
            DrawBundleHeader("Bundle", "Deps", "Assets");

            if (manifest == null || manifest.BundleList == null || manifest.BundleList.Length == 0)
            {
                EditorGUILayout.HelpBox("Empty.", MessageType.Info);
                return;
            }

            int shownCount = 0;
            for (int i = 0; i < manifest.BundleList.Length; i++)
            {
                BundleManifest.Item item = manifest.BundleList[i];
                if (!MatchManifestItem(item))
                    continue;

                shownCount++;
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label(item.Name ?? string.Empty, GUILayout.Width(CBundleWidth));
                    GUILayout.Label(GetArrayCount(item.Deps).ToString(), GUILayout.Width(CTypeWidth));
                    GUILayout.Label(GetArrayCount(item.Assets).ToString(), GUILayout.Width(CTypeWidth));
                    GUILayout.Label(GetAssetPreview(item.Assets));
                }
            }

            DrawFilteredEmpty(shownCount);
        }

        private void DrawBundleHeader(string name0, string name1, string name2)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(name0, EditorStyles.toolbarButton, GUILayout.Width(CBundleWidth));
                GUILayout.Label(name1, EditorStyles.toolbarButton, GUILayout.Width(CTypeWidth));
                GUILayout.Label(name2, EditorStyles.toolbarButton, GUILayout.Width(CTypeWidth));
                GUILayout.Label(string.Empty, EditorStyles.toolbarButton);
            }
        }

        private void RequestSnapshot()
        {
            _RequestError = string.Empty;
            if (_SnapshotService != null && _SnapshotService.RequestSnapShot(_ShowBundleManifest))
                return;

            _RequestError = _SnapshotService == null ? "Snapshot service is not ready." : _SnapshotService.LastError;
        }

        private void DrawError(string error)
        {
            if (!string.IsNullOrEmpty(error))
                EditorGUILayout.HelpBox(error, MessageType.Warning);
        }

        private void DrawFilteredEmpty(int shownCount)
        {
            if (shownCount == 0 && !string.IsNullOrEmpty(_Filter))
                EditorGUILayout.HelpBox("No matched items.", MessageType.Info);
        }

        private bool MatchResItem(ResSnapShotItem item)
        {
            if (string.IsNullOrEmpty(_Filter))
                return true;

            return Contains(item.Id.ToString())
                || Contains(item.ResType.ToString())
                || Contains(item.Path)
                || Contains(item.BundleName)
                || Contains(item.InstStatus.ToString())
                || Contains(item.PathTypeMask.ToString());
        }

        private bool MatchBundleItem(BundleSnapshotItem item)
        {
            if (string.IsNullOrEmpty(_Filter))
                return true;

            return Contains(item.BundleName)
                || Contains(item.BundleStatus.ToString())
                || Contains(item.FileStatus.ToString());
        }

        private bool MatchManifestItem(BundleManifest.Item item)
        {
            if (string.IsNullOrEmpty(_Filter))
                return true;
            if (item == null)
                return false;
            if (Contains(item.Name))
                return true;

            string[] assets = item.GetAssets();
            for (int i = 0; i < assets.Length; i++)
            {
                if (Contains(assets[i]))
                    return true;
            }

            return false;
        }

        private bool Contains(string value)
        {
            return !string.IsNullOrEmpty(value)
                && value.IndexOf(_Filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void OnSnapshotReceived(SnapShot snapshot)
        {
            _RequestError = string.Empty;
            Repaint();
        }

        private void OnConnectionChanged()
        {
            Repaint();
        }

        private void OnConnectionError(string message)
        {
            Repaint();
        }

        private void OnTargetInfoChanged(DebugConnectionTargetInfo info)
        {
            Repaint();
        }
 

        private static int GetCount<T>(System.Collections.Generic.List<T> list)
        {
            return list == null ? 0 : list.Count;
        }

        private static int GetManifestCount(BundleManifest manifest)
        {
            return manifest == null || manifest.BundleList == null ? 0 : manifest.BundleList.Length;
        }

        private static int GetArrayCount<T>(T[] array)
        {
            return array == null ? 0 : array.Length;
        }

        private static string GetAssetPreview(string[] assets)
        {
            if (assets == null || assets.Length == 0)
                return string.Empty;

            const int maxCount = 3;
            int count = Math.Min(assets.Length, maxCount);
            string value = string.Join(", ", assets, 0, count);
            if (assets.Length > maxCount)
                value += " ...";
            return value;
        }

        
    }
}
