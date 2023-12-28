using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace FH.UI.Ed
{
    public class EdUITree : EdUIComp
    {
        public TreeView _tree;

        public EdTreeView<T> GetTree<T>()
        {
            return _tree as EdTreeView<T>;
        }

        public EdTreeView<T> CreateTree<T>()
        {
            var ret = new EdTreeView<T>();
            _tree = ret;
            return ret;
        }

        public override void Draw(Vector2 size)
        {
            Rect rect = new Rect(Vector2.zero, size);
            _tree?.OnGUI(rect);
        }
    }
}
