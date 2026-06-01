/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/5/31
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using UnityEditor;
using UnityEngine;

namespace FH
{
    public sealed class DebugConnectionWindow : EditorWindow
    {
        private DebugConnectionConnectionPanel _ConnectionPanel;

        [MenuItem("Tools/FancyHub/Debug Connection")]
        public static void Open()
        {
            DebugConnectionWindow window = GetWindow<DebugConnectionWindow>();
            window.titleContent = new GUIContent("Debug Connection");
            window.minSize = new Vector2(420, 260);
            window.Show();
        }

        public static void DrawOnToolBar(Action ownerRepaint = null)
        {
            DrawButton(EditorStyles.toolbarDropDown, ownerRepaint);
        }

        public static void DrawButton(GUIStyle style = null, Action ownerRepaint = null)
        {
            if (style == null)
                style = EditorStyles.toolbarDropDown;

            string buttonText = DebugConnectionConnectionPanel.GetStateText();
            GUIContent content = new GUIContent(buttonText);
            Rect connectionRect = GUILayoutUtility.GetRect(content, style);

            if (GUI.Button(connectionRect, buttonText, style))
                PopupWindow.Show(connectionRect, new PopupContent(ownerRepaint));
        }

        public static void DrawButton(Rect position, UnityEditor.SerializedProperty property, GUIContent label, GUIStyle style = null)
        {
            if (style == null)
                style = EditorStyles.toolbarDropDown;
            string buttonText = DebugConnectionConnectionPanel.GetStateText();
            GUIContent content = new GUIContent(buttonText);

            if (GUI.Button(position, buttonText, style))
                PopupWindow.Show(position, new PopupContent(null));
        }

        private void OnEnable()
        {
            _ConnectionPanel = new DebugConnectionConnectionPanel(Repaint);
        }

        private void OnDisable()
        {
            _ConnectionPanel?.Dispose();
            _ConnectionPanel = null;
        }

        private void OnGUI()
        {
            _ConnectionPanel?.DrawWindowGUI();
        }

        private sealed class PopupContent : PopupWindowContent
        {
            private readonly Action _OwnerRepaint;
            private DebugConnectionConnectionPanel _ConnectionPanel;

            public PopupContent(Action ownerRepaint)
            {
                _OwnerRepaint = ownerRepaint;
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(360, 300);
            }

            public override void OnOpen()
            {
                _ConnectionPanel = new DebugConnectionConnectionPanel(RepaintAll);
            }

            public override void OnClose()
            {
                _ConnectionPanel?.Dispose();
                _ConnectionPanel = null;
            }

            public override void OnGUI(Rect rect)
            {
                _ConnectionPanel?.DrawPopupGUI();
            }

            private void RepaintAll()
            {
                if (editorWindow != null)
                    editorWindow.Repaint();
                _OwnerRepaint?.Invoke();
            }
        }
    }


    [UnityEditor.CustomPropertyDrawer(typeof(DebugConnectionButton))]
    public class DebugConnectionButtonEditor : UnityEditor.PropertyDrawer
    {
        public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        {
            DebugConnectionWindow.DrawButton(position, property, label);
        }
    }
}
