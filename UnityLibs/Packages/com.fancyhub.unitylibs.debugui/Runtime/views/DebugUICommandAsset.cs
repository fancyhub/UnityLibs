/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/07/30
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace FH.DebugUI
{
    [System.Serializable]
    public class DebugUICommand
    {
        public enum EParamType
        {
            Int32,
            Int64,
            Float,
            String,
        }

        [System.Serializable]
        public struct Param
        {
            public string Name;
            public EParamType Type;
        }

        public string Path;
        public string Name;
        public Param[] Params;
        [UnityEngine.Multiline]
        public string Script;
    }

    //[UnityEngine.CreateAssetMenu()]
    public class DebugUICommandAsset : ScriptableObject
    {
        public List<DebugUICommand> Commands;
    }
}
