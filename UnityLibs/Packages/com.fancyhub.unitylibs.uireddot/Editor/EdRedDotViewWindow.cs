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
    public class EdRedDotViewWindow : EditorWindow, IEdTreeViewItem<UIRedDotTree.InnerNodeData>
    {
        public EdTreeViewWithToolBar<UIRedDotTree.InnerNodeData> _tree_view;
        public void OnEnable()
        {
            _tree_view = new EdTreeViewWithToolBar<UIRedDotTree.InnerNodeData>();

            ValueTree<UIRedDotTree.InnerNodeData> root = UIRedDotMgr.Tree?._Root;
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
                ValueTree<UIRedDotTree.InnerNodeData> root = UIRedDotMgr.Tree?._Root;
                _tree_view.SetData(root, this);
            }

            Rect rect = new Rect(0, btn_height, position.width, position.height - btn_height);
            _tree_view.OnGUI(rect);
        }

        public Texture2D GetIcon(ValueTree<UIRedDotTree.InnerNodeData> node)
        {
            return AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
        }

        public string GetText(ValueTree<UIRedDotTree.InnerNodeData> node)
        {
            return string.Format($"{node.Key} (Type:{node.Data.NodeType}, Count:{node.Data.Value.Count}, IncreaseFlag:{node.Data.Value.IncrementFlag}, Path:{node.Data.Path})");            
        }
    }
}
