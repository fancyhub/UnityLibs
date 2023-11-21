/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FH.UI.View.Gen.ED
{
    public class EdUIFieldType : IEquatable<EdUIFieldType>
    {
        public enum EType
        {
            Component,
            SubView,
        }

        public EType Type;
        public string Name;
        
        public EdUIFieldType Clone()
        {
            return new EdUIFieldType()
            {
                Type = Type,
                Name = Name
            };
        }

        public static EdUIFieldType CreateComponent(Type comp_type)
        {
            return new EdUIFieldType()
            {

                Type = EType.Component,
                Name = comp_type.FullName,
            };
        }

        public static EdUIFieldType CreateSubView(string class_name)
        {
            return new EdUIFieldType()
            {

                Type = EType.SubView,
                Name = class_name
            };
        }

        public bool Equals(EdUIFieldType other)
        {
            return Type == other.Type && Name == other.Name;
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
