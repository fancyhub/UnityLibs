#if UNITY_EDITOR
using UnityEngine;

namespace FH.EventSetType1Sample
{
    public class Sub : MonoBehaviour
    {
        public PtrList _ptrlist;
        public void Start()
        {

        }

        public void OnEnable()
        {
            _ptrlist += EventMgr.AutoReg()
               .Reg<string>(Pub.EventEnable, _OnMsgUpdate1)
               .Reg<string>(Pub.EventEnable, _OnMsgUpdate2);
        }

        public void OnDisable()
        {
            _ptrlist?.Destroy();
            _ptrlist = null;
        }

        private void _OnMsgUpdate0()
        {
            Debug.Log("OnMsgUpdate0");
        }

        private void _OnMsgUpdate1(string v)
        {
            Debug.Log($"OnMsgUpdate1 Value:{v}");
        }

        private void _OnMsgUpdate2(EventKey key, string v)
        {
            Debug.Log($"OnMsgUpdate2 key: {key} Value: {v}");
        }
    }
}
#endif