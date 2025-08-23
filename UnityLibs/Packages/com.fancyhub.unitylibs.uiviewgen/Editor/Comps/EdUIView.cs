/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/14
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FH.UI.ViewGenerate.Ed
{
    public class EdUIView
    {   
        public EdUIViewDesc ParentDesc;
        public EdUIViewDesc Desc;
        public GameObject Prefab;

        public EdUIView ParentView; //继承的类

        public List<EdUIField> Fields = new List<EdUIField>();
        public List<EdUIViewListField> ListFields = new List<EdUIViewListField>();

         
    }
}