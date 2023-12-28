#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace FH.EventSet2Sample
{
    public class EventMgr : EventSet2<EventKey, object>
    {
        public static EventMgr Inst = new EventMgr();

        public static EventSet2Auto<EventKey, object> AutoReg()
        {
            return EventSet2Auto<EventKey, object>.Create(Inst);
        }
    }

    public static class EventKeyExt
    {
        public static void ExtFire(this EventKey self, object value)
        {
            EventMgr.Inst.Fire(self, value);
        }

        public static void ExtFireAsync(this EventKey self, object value)
        {
            EventMgr.Inst.FireAsync(self, value);
        }
    }
}
#endif