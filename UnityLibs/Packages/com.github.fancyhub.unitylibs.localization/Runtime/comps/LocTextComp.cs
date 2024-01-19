/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace FH
{
    [RequireComponent(typeof(Text))]
    public sealed class LocTextComp : LocComp
    {
        public override void OnLocalize(string lang)
        {
            if (!LocMgr.TryGet(this._LocKey, out var tran))
                return;
            Text text = GetComponent<Text>();

            if (text.text != tran)
                text.text = tran;
        }

#if UNITY_EDITOR
        public override void EdDoLocalize(string lang)
        {
            if (!LocMgr.EdTryGet(this._LocKey, lang, out var tran))
                return;

            Text text = GetComponent<Text>();
            bool changed = false;
            if (text.text != tran)
            {
                text.text = tran;
                changed = true;
            }


            if (changed)
            {
                UnityEditor.EditorUtility.SetDirty(text);
            }
        }
#endif
    }
}