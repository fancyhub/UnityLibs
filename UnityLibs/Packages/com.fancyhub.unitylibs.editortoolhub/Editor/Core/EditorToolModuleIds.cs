/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   :
 * Desc    :
*************************************************************************************/

using System;
using UnityEditor;

namespace FH.EditorToolHub
{
    public static class EditorToolModuleIds
    {
        public static string GetEditorWindowModuleId(Type editorWindowType)
        {
            if (editorWindowType == null)
                throw new ArgumentNullException("editorWindowType");

            if (!typeof(EditorWindow).IsAssignableFrom(editorWindowType))
                throw new ArgumentException("Type must derive from EditorWindow.", "editorWindowType");

            return "editor-window:" + editorWindowType.AssemblyQualifiedName;
        }
    }
}
