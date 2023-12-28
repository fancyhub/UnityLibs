using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.Ed
{
    public class EdUILayoutStack
    {
        public Stack<Matrix4x4> _stacks = new Stack<Matrix4x4>();

        public void Push(Matrix4x4 matrix)
        {
            _stacks.Push(GUI.matrix);
            GUI.matrix = matrix;
        }

        public void Pop()
        {
            if (_stacks.Count == 0)
                return;
            GUI.matrix = _stacks.Pop();
        }
    }
}
