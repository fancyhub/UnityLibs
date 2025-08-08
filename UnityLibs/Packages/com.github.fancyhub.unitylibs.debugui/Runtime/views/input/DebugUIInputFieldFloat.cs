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
    public class DebugUIInputFieldFloat : DebugUIInputField
    {
        private FloatField _Input;
        private string _FieldName;
        public override string FieldName => _FieldName;

        public DebugUIInputFieldFloat(string name, object defaultValue = null)
        {
            this._FieldName = name;
            _Input = new FloatField($"{name} (float):");
            if (defaultValue != null && defaultValue is float v)
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
