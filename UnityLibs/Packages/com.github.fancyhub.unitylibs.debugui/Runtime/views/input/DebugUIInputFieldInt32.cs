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
    public class DebugUIInputFieldInt32 : DebugUIInputField
    {
        private IntegerField _Input;
        private string _FieldName;
        public override string FieldName => _FieldName;
        public DebugUIInputFieldInt32(string name, object defaultValue = null)
        {
            this._FieldName = name;
            _Input = new IntegerField($"{name} (int32):");
            if (defaultValue != null && defaultValue is int v)
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

    public class DebugUIInputFieldUInt32 : DebugUIInputField
    {
        private IntegerField _Input;
        private string _FieldName;
        public override string FieldName => _FieldName;
        public DebugUIInputFieldUInt32(string name, object defaultValue = null)
        {
            this._FieldName = name;
            _Input = new IntegerField($"{name} (uint32):");
            if (defaultValue != null && defaultValue is uint v)
            {
                _Input.value = (int)v;
            }
            this.Add(_Input);
        }
        public override object GetInputValue()
        {
            uint v = (uint)_Input.value;
            return v;
        }
    }
}
