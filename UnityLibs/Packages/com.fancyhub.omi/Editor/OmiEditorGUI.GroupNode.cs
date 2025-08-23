/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace FH.Omi.Editor
{
    public static partial class OmiEditorGUI
    {
        public static void DrawGroup(GroupNode node)
        {
            if (node == null)
                return;

            GroupAttribute attr = node.GroupAttr;
            if (attr == null)
            {
                _DrawMemberList(node.Members);
                _DrawNodeList(node.Children);
                return;
            }

            switch (attr.Style)
            {
                case EGroupStyle.Box:
                    {
                        using var box_handler = new GroupBoxHandler(node);
                        using var dir_handler = new GroupDirHandler(node);
                        _DrawGroupInnerContent(node);
                    }
                    break;

                case EGroupStyle.Folder:
                    {

                    }
                    break;

                case EGroupStyle.None:
                    {
                        using var dir_handler = new GroupDirHandler(node);
                        _DrawGroupInnerContent(node);
                    }
                    break;
            }
        }


        private static void _DrawGroupInnerContent(GroupNode node)
        {
            if (node == null)
                return;

            GroupAttribute attr = node.GroupAttr;
            if (attr == null || !attr.TabChild)
            {
                _DrawMemberList(node.Members);
                _DrawNodeList(node.Children);
                return;
            }
            var tabs = node.Tabs;
            if (tabs == null || tabs.Length == 0)
            {
                _DrawMemberList(node.Members);
                _DrawNodeList(node.Children);
                return;
            }

            _DrawMemberList(node.Members);
            int tab_index = node.GetUserData<int>("tab_index");
            int new_index = GUILayout.Toolbar(tab_index, node.Tabs);
            if (new_index != tab_index)
                node.SetUserData("tab_index", new_index);
            DrawGroup(node.Children[new_index]);
        }

        private static void _DrawNodeList(List<GroupNode> node_list)
        {
            if (node_list == null)
                return;

            foreach (var child in node_list)
                DrawGroup(child);
        }

        public struct GroupBoxHandler : IGUIHandler
        {
            private GroupAttribute _Attr;
            public GroupBoxHandler(GroupNode node)
            {
                _Attr = null;
                if (node == null)
                    return;
                _Attr = node.GroupAttr;
                GUILayout.BeginVertical(node.Name, "window");
            }

            public void Dispose()
            {
                if (_Attr == null)
                    return;
                _Attr = null;
                UnityEditor.EditorGUILayout.EndVertical();
                UnityEditor.EditorGUILayout.Space();
            }
        }

        public struct GroupDirHandler : IGUIHandler
        {
            private GroupAttribute _Attr;
            public GroupDirHandler(GroupNode node)
            {
                this._Attr = null;
                if (node == null)
                    return;
                this._Attr = node.GroupAttr;

                switch (_Attr.Dir)
                {
                    case EGroupDir.Horizontal:
                        UnityEditor.EditorGUILayout.BeginHorizontal();
                        break;
                    case EGroupDir.Vertical:
                        UnityEditor.EditorGUILayout.BeginVertical();
                        break;
                }
            }

            public void Dispose()
            {
                if (_Attr == null)
                    return;
                var dir = _Attr.Dir;
                _Attr = null;

                switch (dir)
                {
                    case EGroupDir.Horizontal:
                        UnityEditor.EditorGUILayout.EndHorizontal();
                        break;
                    case EGroupDir.Vertical:
                        UnityEditor.EditorGUILayout.EndVertical();
                        break;
                }
            }
        }
    }

    public interface IGUIHandler : IDisposable
    {
    }
}