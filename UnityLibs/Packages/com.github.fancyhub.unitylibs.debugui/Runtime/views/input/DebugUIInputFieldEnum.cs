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
    public class DebugUIInputFieldEnum : DebugUIInputField
    {
        private EnumField _Input;
        private string _FieldName;
        public override string FieldName => _FieldName;

        public DebugUIInputFieldEnum(string name, Type paramType, object defaultValue = null)
        {
            this._FieldName = name;
            Enum defaultEnumVal = defaultValue as Enum;
            if (defaultEnumVal == null)
            {
                defaultEnumVal = Activator.CreateInstance(paramType) as Enum;
            }
            _Input = new EnumField($"{name} (Enum):", defaultEnumVal);
            this.Add(_Input);
        }
        public override object GetInputValue()
        {
            return _Input.value;
        }
    }
}
