/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/07/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace FH.DebugUI
{
    public class DebugUIIManager
    {
        private DebugUIItem _DataRoot;
        private bool _Dirty = false;
        private DebugUIDocView _DocView;
        private DebugUICommandExecutor _CmdExecutor;

        public DebugUIIManager()
        {
            _DataRoot = new DebugUIItem(null);
            _CmdExecutor = new DebugUICommandExecutor();
            _Dirty = false;
        }

        public void AddItem(string path, IDebugUIItemView view)
        {
            if (_DataRoot.Add(path, view))
                _Dirty = true;
        }

        public void RegCommand(string cmd, MethodInfo action)
        {
            _CmdExecutor.Reg(cmd, action);
        }

        public void Exec(string script, List<(string name, string value)> args = null)
        {
            _CmdExecutor.Exec(script, args);
        }

        public DebugUIDocView ShowInUIDocument(UIDocument doc)
        {
            if (_DocView == null)
                _DocView = new DebugUIDocView();
            if (_Dirty)
            {
                _DataRoot.Sort();
                _DocView.SetTreeViewData(_DataRoot);
                _Dirty = false;
            }

            doc.rootVisualElement.Clear();
            doc.rootVisualElement.Add(_DocView);           
            return _DocView;
        }
    }
}
