/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/24
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH.Omi
{
    public enum EGroupDir
    {
        Vertical,
        Horizontal,
    }

    public enum EGroupStyle
    {
        None = 0,
        Box,
        Folder,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property, AllowMultiple = true)]
    public class GroupAttribute : LayoutAttribute
    {
        public readonly string Path;
        public EGroupDir Dir { get; set; } = EGroupDir.Vertical;
        public EGroupStyle Style { get; set; } = EGroupStyle.None;
        public bool TabChild { get; set; }

        public GroupAttribute(string path)
        {
            Path = path;
        }
    }
}