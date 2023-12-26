/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/12/25
 * Title   : 
 * Desc    : 
*************************************************************************************/


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using FH.Ed;
namespace FH.FileManagement.Ed
{
   
    public sealed class BuildContext
    {
        public BuildTarget BuildTarget;         

        public string Target2Name()
        {
            return BuildTarget.Ext2Name();
        }         
    }     
}
