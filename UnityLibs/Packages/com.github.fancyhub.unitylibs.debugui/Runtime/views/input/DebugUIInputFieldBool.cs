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
    public class DebugUIInputFieldBool : DebugUIInputField
    {
        private Toggle _Input;
        private string _FieldName;
        public override string FieldName => _FieldName;

        public DebugUIInputFieldBool(string name, object defaultValue = null)
        {
            _FieldName = name;
            //_Input.value = defaultValue;
            _Input = new Toggle($"{name} (bool):");
            if (defaultValue != null && defaultValue is bool v)
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
