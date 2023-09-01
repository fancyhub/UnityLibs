#if UNITY_EDITOR
using UnityEngine;

namespace FH.EventSet2Sample
{
    public class EventMgrUpdater : MonoBehaviour
    {
        public void Update()
        {
            EventMgr.Inst.ProcessAllEvents();
        }
    }
}

#endif