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
    public class DebugUIInputFieldString : DebugUIInputField
    {
        private TextField _Input;
        private string _FieldName;
        public override string FieldName => _FieldName;
        public DebugUIInputFieldString(string name, object defaultValue = null)
        {
            this._FieldName = name;
            _Input = new TextField($"{name} (string):");
            if (defaultValue != null && defaultValue is string v)
            {
                _Input.value = v;
            }
            this.Add(_Input);
        }
        public override object GetInputValue()
        {
            return _Input.value;
        }
    }
}
