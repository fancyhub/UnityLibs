#if UNITY_EDITOR
using UnityEngine;

namespace FH.EventSet2Sample
{
    public class Sub : MonoBehaviour, EventMgr.IHandler
    {
        public PtrList _ptrlist;
        public void Start()
        {
            LogRecorderMgr.Init();
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
            Log.D("OnMsgUpdate0");
        }

        private void _OnMsgUpdate1(object v)
        {
            Log.I($"OnMsgUpdate1 Value:{v}");
        }

        private void _OnMsgUpdate2(EventKey key, object v)
        {
            Log.E($"OnMsgUpdate2 key: {key} Value: {v}");
        }

        public void HandleEvent(EventKey key, object value)
        {
            Log.D($"HandleEvent key: {key} Value: {value}");
        }
    }
}
#endif