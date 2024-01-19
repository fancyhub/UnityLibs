/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/1/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;

namespace FH
{
    [DisallowMultipleComponent]
    public abstract class LocComp : MonoBehaviour
    {
        [SerializeField] protected LocKey _LocKey;

        public void OnEnable()
        {
            DoLocalize();
        }

        public void DoLocalize()
        {
            OnLocalize(LocMgr.Lang);
        }

#if UNITY_EDITOR
        public abstract void EdDoLocalize(string lang);
#endif

        public abstract void OnLocalize(string lang);
    }
}