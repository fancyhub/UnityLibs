/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   :
 * Desc    :
*************************************************************************************/

using System;
using UnityEditor;
using UnityEngine;

namespace FH.EditorToolHub
{
    public sealed class EditorToolModuleContext
    {
        private readonly Action _saveLayout;
        private readonly Func<bool> _isEditMode;

        internal EditorToolModuleContext(EditorWindow window, string panelId, Action saveLayout, Func<bool> isEditMode)
        {
            Window = window;
            PanelId = panelId;
            _saveLayout = saveLayout;
            _isEditMode = isEditMode;
        }

        public EditorWindow Window { get; private set; }
        public string PanelId { get; private set; }
        public bool IsEditMode { get { return _isEditMode != null && _isEditMode(); } }
        public UnityEngine.Object[] Selection { get { return UnityEditor.Selection.objects; } }

        public void Repaint()
        {
            if (Window != null)
                Window.Repaint();
        }

        public void SaveLayout()
        {
            if (_saveLayout != null)
                _saveLayout();
        }
    }
}
