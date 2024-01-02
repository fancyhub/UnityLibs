/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;


namespace FH.UI.ViewGenerate.Ed
{
    public class EdUIFieldType : IEquatable<EdUIFieldType>
    {
        public enum EType
        {
            Component,
            SubView,
        }

        public EType Type;

        public Type CompType;
        public EdUIViewDesc ViewType;

        public EdUIFieldType Clone()
        {
            return new EdUIFieldType()
            {
                Type = Type,
                CompType = CompType,
                ViewType = ViewType
            };
        }

        public static EdUIFieldType CreateComponent(Type comp_type)
        {
            return new EdUIFieldType()
            {
                Type = EType.Component,
                CompType = comp_type,
            };
        }

        public static EdUIFieldType CreateSubView(EdUIViewDesc view_type)
        {
            return new EdUIFieldType()
            {
                Type = EType.SubView,
                ViewType = view_type
            };
        }

        public bool Equals(EdUIFieldType other)
        {
            return Type == other.Type && ViewType == other.ViewType && CompType == other.CompType;
        }
    }

    /// <summary>
    /// 一个Field 对应一个 节点
    /// </summary>
    public class EdUIField
    {
        public string Path;
        public string Fieldname;

        public EdUIFieldType FieldType;
    }
}
