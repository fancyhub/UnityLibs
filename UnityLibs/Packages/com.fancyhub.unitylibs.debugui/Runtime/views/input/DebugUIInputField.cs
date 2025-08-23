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
    public abstract class DebugUIInputField : VisualElement
    {
        public abstract string FieldName { get; }
        public abstract object GetInputValue();
        public virtual bool IsInputValid() { return true; }
    }
}
