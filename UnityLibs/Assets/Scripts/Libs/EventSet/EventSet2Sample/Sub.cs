#if UNITY_EDITOR
using UnityEngine;

namespace FH.EventSet2Sample
{
    public class Sub : MonoBehaviour, IEventSet2Handler<EventKey, object>
    {
        public PtrList _ptrlist;
        public void Start()
        {

        }

        public void OnEnable()
        {
            _ptrlist += EventMgr.AutoReg()
                               .Reg(Pub.EventEnable, _OnMsgUpdate0)
                               .Reg(Pub.EventEnable, _OnMsgUpdate1)
                               .Reg(Pub.EventEnable, _OnMsgUpdate2)
                               .Reg(Pub.EventEnable, this);
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

        private void _OnMsgUpdate1(object v)
        {
            Debug.Log($"OnMsgUpdate1 Value:{v}");
        }

        private void _OnMsgUpdate2(EventKey key, object v)
        {
            Debug.Log($"OnMsgUpdate2 key: {key} Value: {v}");
        }

        public void HandleEvent(EventKey key, object value)
        { 
            Debug.Log($"HandleEvent key: {key} Value: {value}");
        }
    }
}
#endif