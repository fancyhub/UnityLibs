/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace FH.UI.ViewGenerate.Ed
{
    public class EdUIViewListField
    {
        public struct FieldRef
        {
            public int Index;
            public EdUIField Field;

            public FieldRef(EdUIField field, int index)
            {
                Field = field;
                this.Index = index;
            }

            

            public EdUIFieldType FieldType
            {
                get
                {
                    return Field.FieldType;
                }
            }
        }
        /// <summary>
        /// list 对象的名字
        /// </summary>
        public string FieldName;
        public List<FieldRef> FieldList = new List<FieldRef>();


        /// <summary>
        /// 如果 该 父结构里面也有相同的 field，该对象就不会是空的
        /// </summary>
        public EdUIViewListField Parent;
        public bool HasChild; //是否有子结构       
        public EdUIFieldType FieldType;

        public EdUIViewListField(string name)
        {
            FieldName = name;
        }

        public void Sort()
        {
            FieldList.Sort((a, b) =>
            {
                return a.Index - b.Index;
            });
        }
    }
}
