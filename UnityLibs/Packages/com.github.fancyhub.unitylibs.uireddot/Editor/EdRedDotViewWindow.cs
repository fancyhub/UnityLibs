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
    public class EdRedDotViewWindow : EditorWindow, IEdTreeViewData<UIRedDotItemData>
    {
        public EdTreeViewWithToolBar<UIRedDotItemData> _tree_view;
        public void OnEnable()
        {
            _tree_view = new EdTreeViewWithToolBar<UIRedDotItemData>();

            ValueTree<UIRedDotItemData> root = null;
            var ui_rd_data = UIRedDotData.Inst;
            if (ui_rd_data != null)
                root = ui_rd_data._Tree._Root;
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
                ValueTree<UIRedDotItemData> root = null;
                var ui_rd_data = UIRedDotData.Inst;
                if (ui_rd_data != null)
                    root = ui_rd_data._Tree._Root;
                _tree_view.SetData(root, this);
            }

            Rect rect = new Rect(0, btn_height, position.width, position.height - btn_height);
            _tree_view.OnGUI(rect);
        }

        public Texture2D GetIcon(ValueTree<UIRedDotItemData> node)
        {
            return AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));
        }

        public string GetName(ValueTree<UIRedDotItemData> node)
        {
            switch (node.Data.DataType)
            {
                case EUIRedDotDataType.AutoNode:
                    return string.Format("A: {0}({1})", node.Key, node.Data.Value);
                case EUIRedDotDataType.ManualNode:
                    return string.Format("M: {0}({1})", node.Key, node.Data.Value);
                case EUIRedDotDataType.VirtualNode:
                    return string.Format("V: {0}({1})", node.Key, node.Data.Value);
                default:
                    return "";
            }
        }
    }
}
