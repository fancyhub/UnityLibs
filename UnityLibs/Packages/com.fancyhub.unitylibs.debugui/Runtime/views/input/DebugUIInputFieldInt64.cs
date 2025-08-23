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

    public class DebugUIInputFieldInt64 : DebugUIInputField
    {
        private LongField _Input;
        private string _FieldName;
        public override string FieldName => _FieldName;

        public DebugUIInputFieldInt64(string name, object defaultValue = null)
        {
            this._FieldName = name;
            _Input = new LongField($"{name} (int64):");
            if (defaultValue != null && defaultValue is long v)
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

    public class DebugUIInputFieldUInt64 : DebugUIInputField
    {
        private LongField _Input;
        private string _FieldName;
        public override string FieldName => _FieldName;

        public DebugUIInputFieldUInt64(string name, object defaultValue = null)
        {
            this._FieldName = name;
            _Input = new LongField($"{name} (uint64):");
            if (defaultValue != null && defaultValue is ulong v)
            {
                _Input.value = (long)v;
            }
            this.Add(_Input);
        }
        public override object GetInputValue()
        {
            ulong v= (ulong)_Input.value;
            return v;
        }
    }
}
