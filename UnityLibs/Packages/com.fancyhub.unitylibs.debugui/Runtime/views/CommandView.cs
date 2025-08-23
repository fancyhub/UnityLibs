/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/07/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.UIElements;

namespace FH.DebugUI
{
    public class CommandView : VisualElement, IDebugUIItemView
    {
        private DebugUICommand _Cmd;
        private DebugUIIManager _Mgr;

        private List<DebugUIInputField> _InputElements;
        private List<(string name, string value)> _Args;

        public CommandView(DebugUICommand cmd, DebugUIIManager mgr)
        {
            _Cmd = cmd;
            _Mgr = mgr;

            if (_InputElements == null)
                _InputElements = new List<DebugUIInputField>();

            bool isValid = true;

            if (cmd.Params != null && cmd.Params.Length > 0)
            {
                Label methodInfoLabel = new Label(cmd.Name);
                methodInfoLabel.style.alignSelf = Align.Center;
                Add(methodInfoLabel);

                foreach (var param in cmd.Params)
                {
                    var item = DebugUIInputFieldFactory.CreateInputField(param);
                    if (!item.IsInputValid())
                        isValid = false;
                    _InputElements.Add(item);
                    Add(item);
                }
            }


            if (isValid)
            {
                var button = new Button(ExecuteSelectedCommand);
                button.text = cmd.Name;
                if (_InputElements.Count > 0)
                {
                    button.style.marginTop = 10;
                    button.style.alignSelf = Align.Center;
                }
                Add(button);
            }
        }

        public void OnDebugUIItemEnable(VisualElement content)
        {
            if (_InputElements.Count > 0)
                this.style.width = content.resolvedStyle.width - 30;
            content.Add(this);
        }

        public void ExecuteSelectedCommand()
        {
            if (_InputElements.Count > 0)
            {
                if (_Args == null)
                    _Args = new();

                _Args.Clear();
                foreach (var input in _InputElements)
                {
                    _Args.Add((input.FieldName, input.GetInputValue().ToString()));
                }
            }

            _Mgr.Exec(_Cmd.Script, _Args);
        }
    }

}
