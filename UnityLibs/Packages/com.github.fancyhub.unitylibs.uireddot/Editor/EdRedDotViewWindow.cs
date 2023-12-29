/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/8/18 17:57:02
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;
using UnityEditor;
using FH.UI.Ed;

namespace FH.UI.RedDot.Ed
{
    public class EdRedDotViewWindow : EditorWindow, IEdTreeViewData<UIRedDotNodeData>
    {
        public EdTreeViewWithToolBar<UIRedDotNodeData> _tree_view;
        public void OnEnable()
        {
            _tree_view = new EdTreeViewWithToolBar<UIRedDotNodeData>();

            ValueTree<UIRedDotNodeData> root = UIRedDotMgr.RootNode;
            _tree_view.SetData(root, this);
        }

        [MenuItem("Tools/Red Dot Viewer", false, 25)]
        static void FindRef()
        {
            EdRedDotViewWindow window = GetWindow<EdRedDotViewWindow>();
            window.titleContent = new GUIContent("Red Dot Viewer");
            window.Show();
        }

        public void OnGUI()
        {
            float btn_height = 30;
            if (GUILayout.Button("Refresh"))
            {
                ValueTree<UIRedDotNodeData> root = UIRedDotMgr.RootNode;                
                _tree_view.SetData(root, this);
            }

            Rect rect = new Rect(0, btn_height, position.width, position.height - btn_height);
            _tree_view.OnGUI(rect);
        }

        public Texture2D GetIcon(ValueTree<UIRedDotNodeData> node)
        {
            return AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
        }

        public string GetName(ValueTree<UIRedDotNodeData> node)
        {
            switch (node.Data.NodeType)
            {
                case EUIRedDotNodeType.AutoNode:
                    return string.Format("A: {0}({1})", node.Key, node.Data.Value);
                case EUIRedDotNodeType.ManualNode:
                    return string.Format("M: {0}({1})", node.Key, node.Data.Value);
                case EUIRedDotNodeType.VirtualNode:
                    return string.Format("V: {0}({1})", node.Key, node.Data.Value);
                default:
                    return "";
            }
        }
    }
}
