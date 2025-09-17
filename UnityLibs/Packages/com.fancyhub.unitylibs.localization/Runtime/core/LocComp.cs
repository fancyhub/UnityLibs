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
        public enum EMode
        {
            Str,
            Id,
        }
        [SerializeField] private LocKeyStr _LocKeyStr;
        protected LocKeyId _LocKeyId;
        private EMode _Mode = EMode.Str;

        public void SetKey(LocKeyStr key)
        {
            if (_Mode == EMode.Str && _LocKeyStr.Equals(key))
                return;
            _LocKeyStr = key;
            _Mode = EMode.Str;
            DoLocalize();
        }

        public void Setkey(LocKeyId id)
        {
            if (_Mode == EMode.Id && _LocKeyId.Equals(id))
                return;
            _LocKeyId = id;
            _Mode = EMode.Id;
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
            if (_Mode == EMode.Str)
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