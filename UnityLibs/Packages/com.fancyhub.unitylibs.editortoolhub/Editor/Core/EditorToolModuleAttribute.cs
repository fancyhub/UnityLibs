/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   :
 * Desc    :
*************************************************************************************/

using System;

namespace FH.EditorToolHub
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class EditorToolModuleAttribute : Attribute
    {
        public EditorToolModuleAttribute(string id, string title)
        {
            Id = id;
            Title = title;
        }

        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Category { get; set; }
        public int Order { get; set; }
        public bool IsDefault { get; set; }
        public Type ReplacesEditorWindowType { get; set; }
    }
}
