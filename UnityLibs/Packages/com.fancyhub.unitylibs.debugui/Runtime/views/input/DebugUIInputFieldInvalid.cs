/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/07/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine.UIElements;

namespace FH.DebugUI
{
    public class DebugUIInputFieldInvalid : DebugUIInputField
    {
        private Label _Input;
        private string _FieldName;
        public override string FieldName => _FieldName;
        public DebugUIInputFieldInvalid(string name, Type type)
        {
            this._FieldName = name;
            _Input = new Label($"{name} ({type.Name}) Unsupport");
            this.Add(_Input);
        }

        public DebugUIInputFieldInvalid(string name)
        {
            //_Input.value = defaultValue;
            _Input = new Label($"{name} Unsupport");
            this.Add(_Input);
        }

        public override object GetInputValue()
        {
            return "";
        }

        public override bool IsInputValid() { return false; }
    }
}
