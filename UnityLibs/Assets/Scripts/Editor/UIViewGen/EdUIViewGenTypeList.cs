using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdUIViewGenTypeList : FH.UI.View.Gen.ED.EdUIViewGenTypeList
{
    public override List<System.Type> GetCompOrderList()
    {
            return new List<System.Type>()
            {
                typeof(UnityEngine.UI.Button),
                typeof(UnityEngine.UI.Toggle),
                typeof(UnityEngine.UI.Slider),
                typeof(UnityEngine.UI.InputField),
                typeof(UnityEngine.UI.ScrollRect),
                typeof(UnityEngine.UI.Text),
                typeof(UnityEngine.UI.Scrollbar),
                typeof(UnityEngine.UI.RawImage),
                typeof(UnityEngine.UI.Image),
                typeof(UnityEngine.RectTransform),
                typeof(UnityEngine.Transform),
            };
    }
}
