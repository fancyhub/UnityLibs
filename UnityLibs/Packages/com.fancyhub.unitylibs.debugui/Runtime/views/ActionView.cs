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
    [System.AttributeUsage(System.AttributeTargets.Method)]
    public sealed class ActionViewAttribute : System.Attribute
    {
        public readonly string Path;
        public readonly string Name;
        public ActionViewAttribute(string path, string name = null)
        {
            Path = path;
            Name = name;
        }
    }

    public class ActionView : VisualElement, IDebugUIItemView
    {
        private MethodInfo _MethodInfo;

        private string _Name;
        private List<DebugUIInputField> _InputElements;
        private object[] _Args;

        public string DebugUIItemName => _Name;

        public ActionView(MethodInfo method_info, string name)
        {
            _MethodInfo = method_info;
            _Name = name;
            if (_Name == null)
                _Name = _MethodInfo.Name;


            if (_InputElements == null)
                _InputElements = new List<DebugUIInputField>();

            bool isValid = true;
            ParameterInfo[] parameters = _MethodInfo.GetParameters();
            if (parameters.Length > 0)
            {
                Label methodInfoLabel = new Label(_Name);
                methodInfoLabel.style.fontSize = 16;
                methodInfoLabel.style.marginBottom = 10;
                methodInfoLabel.style.alignSelf = Align.Center;
                Add(methodInfoLabel);

                foreach (ParameterInfo param in parameters)
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
                button.text = _Name;
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
            if (_Args == null)
            {
                if (_InputElements.Count == 0)
                    _Args = Array.Empty<object>();
                else
                    _Args = new object[_InputElements.Count];
            }

            for (int i = 0; i < _Args.Length; i++)
            {
                _Args[i] = _InputElements[i].GetInputValue();
            }
            _MethodInfo.Invoke(null, _Args);
        }
    }


}
