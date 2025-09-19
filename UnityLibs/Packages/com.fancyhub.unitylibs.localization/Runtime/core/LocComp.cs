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
       
        [SerializeField] private LocKeyStr _LocKeyStr;
        protected LocKeyInt _LocKeyId;
        private ELocKeyMode _Mode = ELocKeyMode.Str;

        public void SetKey(LocKeyStr key)
        {
            if (_Mode == ELocKeyMode.Str && _LocKeyStr.Equals(key))
                return;
            _LocKeyStr = key;
            _Mode = ELocKeyMode.Str;
            DoLocalize();
        }

        public void Setkey(LocKeyInt id)
        {
            if (_Mode == ELocKeyMode.Int && _LocKeyId.Equals(id))
                return;
            _LocKeyId = id;
            _Mode = ELocKeyMode.Int;
            DoLocalize();
        }

        public void OnEnable()
        {
            DoLocalize();
        }

        public void DoLocalize()
        {
            if(this.enabled)
                OnLocalize(LocMgr.CurrentLang);
        }

        protected bool TryGetTran(out string tran)
        {
            if (_Mode == ELocKeyMode.Str)
                return LocMgr.TryGet(this._LocKeyStr, out tran, this);
            else
                return LocMgr.TryGet(this._LocKeyId, out tran, this);
        }

#if UNITY_EDITOR
        public abstract void EdDoLocalize(string lang);
        protected bool EdTryGetTran(string lang, out string tran)
        {
            return LocMgr.EdTryGet(this._LocKeyStr, lang, out tran);
        }
#endif

        public abstract void OnLocalize(string lang);
    }
}