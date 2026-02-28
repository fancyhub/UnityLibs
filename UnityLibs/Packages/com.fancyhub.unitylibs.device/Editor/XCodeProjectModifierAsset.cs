/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/02/03
 * Title   : 
 * Desc    : 
*************************************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System;

namespace FH.DI.Ed
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "XCodeProject", menuName = "fancyhub/XCode Project Asset")]
    public class XCodeProjectModifierAsset : ScriptableObject
    {
        [Serializable]
        public class Framework
        {
            public string FrameworkName;
            public bool Weak;

            [UnityEngine.Multiline]
            public string Comment;
        }

        public List<Framework> FrameworksToAdd = new ();
    }
}
