/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/08/10
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.ViewGenerate
{
    public class UIItemVariable : MonoBehaviour
    {
        public string ExportName;
        public UnityEngine.Object ExportObject;

        public string GetExportedName()
        {
            if (string.IsNullOrEmpty(ExportName))
            {
                return this.gameObject.name;
            }

            return ExportName;
        }

        public System.Type GetExportObjectType()
        {
            if (ExportObject == null)
                return null;
            return ExportObject.GetType();
        }
    }
}
