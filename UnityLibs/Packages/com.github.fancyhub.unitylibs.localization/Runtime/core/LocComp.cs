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
            Key,
            Id,
        }
        [SerializeField] private LocKey _LocKey;
        protected LocId _LocId;
        private EMode _Mode = EMode.Key;

        public void SetKey(LocKey key)
        {
            if (_Mode == EMode.Key && _LocKey.Equals(key))
                return;
            _LocKey = key;
            _Mode = EMode.Key;
            DoLocalize();
        }

        public void SetId(LocId id)
        {
            if (_Mode == EMode.Id && _LocId.Equals(id))
                return;
            _LocId = id;
            _Mode = EMode.Id;
            DoLocalize();
        }

        public void OnEnable()
        {
            DoLocalize();
        }

        public void DoLocalize()
        {
            OnLocalize(LocMgr.Lang);
        }

        protected bool TryGetTran(out string tran)
        {
            if (_Mode == EMode.Key)
                return LocMgr.TryGet(this._LocKey, out tran);
            else
                return LocMgr.TryGet(this._LocId, out tran);
        }

#if UNITY_EDITOR
        public abstract void EdDoLocalize(string lang);
        protected bool EdTryGetTran(string lang, out string tran)
        {
            return LocMgr.EdTryGet(this._LocKey, lang, out tran);
        }
#endif

        public abstract void OnLocalize(string lang);
    }
}