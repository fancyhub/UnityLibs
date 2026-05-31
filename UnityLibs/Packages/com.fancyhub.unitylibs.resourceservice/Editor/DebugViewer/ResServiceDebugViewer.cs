using UnityEditor;
using UnityEngine;

namespace FH
{
    public sealed partial class ResServiceDebugViewer : EditorWindow
    {
        private EdResServiceSnapshot _SnapshotView;

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
            _SnapshotView = new EdResServiceSnapshot(Repaint);
        }

        private void OnDisable()
        {
            _SnapshotView?.Dispose();
            _SnapshotView = null;
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.Space(6);
            _SnapshotView?.OnGUI();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                DebugConnectionWindow.DrawOnToolBar(Repaint);
                GUILayout.FlexibleSpace();
            }
        }
    }
}
