/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH.Omi
{    
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ButtonAttribute : BaseAttribute
    {
        public string Text;
        public ButtonAttribute(string text = null)
        {
            Text = text;
        }
    }
}