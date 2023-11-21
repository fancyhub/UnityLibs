using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI
{
    [CreateAssetMenu(menuName = "UIView/UI View Gen Config", fileName = "NewViewGeneratorConfig")]
    public class UIViewGenConfig: ScriptableObject
    {        
        public List<Type> TypeList = new List<Type>();

    }
}
