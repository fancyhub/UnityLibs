#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace FH.EventSetType1Sample
{
    public class EventMgr : EventSetType1<EventKey>
    {
        public static EventMgr Inst = new EventMgr();

        public static EventSetType1Auto<EventKey> AutoReg()
        {
            return EventSetType1Auto<EventKey>.Create(Inst);
        }
    }

    public static class EventKeyExt
    {
        public static void ExtFire<TValue>(this EventKey self, TValue value)
        {
            EventMgr.Inst.Fire<TValue>(self, value);
        }
    }
}
#endif