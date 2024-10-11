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


    public class Event2Mgr1 : EventSet2<int, string>
    {
    }

    public class Event1Mgr1 : EventSet1<int>
    {
    }


    public struct Event2Msg1
    {
        public string msg;
    }

    public class Event1Mgr2 : EventSet1<Type>
    {
        public EventHandler Reg<TValue>(Action<TValue> action)
        {
            return Reg<TValue>(typeof(TValue), action);
        }

        public bool Fire<TValue>(TValue value)
        {
            return Fire<TValue>(typeof(TValue), value);
        }
    }

    public class TestEventSet
    {
        public static void Test()
        {
            {
                Log.I("Begin set1 Test==================");
                Event1Mgr1 set1 = new Event1Mgr1();
                Event1Mgr1.EventHandlerList handlerList = null;

                handlerList += set1.Reg<string>(1, _set1_action1);
                Log.Assert(!set1.Reg<string>(1, _set1_action1).Valid, "未通过: 重复添加的测试");
                Log.Assert(!set1.Reg<int>(1, _set1_action2).Valid, "未通过: 类型添加测试");
                Log.Assert(!set1.Fire(1, 1), "未通过: 类型Fire测试");


                set1.Fire(1, "set1 hello1");
                handlerList.Destroy();
                set1.Fire(1, "未通过: set1 hello2 ");
            }

            {
                Log.I("Begin set2 Test==================");
                Event1Mgr2 set2 = new Event1Mgr2();
                Event1Mgr2.EventHandlerList handlerList = null;

                handlerList += set2.Reg<Event2Msg1>(_set2_action1);
                handlerList += set2.Reg<Event2Msg1>(_set2_action1);

                set2.Fire(new Event2Msg1()
                {
                    msg = "set2 Hello1"
                });

                handlerList.Destroy();

                set2.Fire(new Event2Msg1()
                {
                    msg = "未通过: set2 Hello2"
                });
            }

            {
                Log.I("Begin set3 Test==================");
                Event2Mgr1 set3 = new Event2Mgr1();
                Event2Mgr1.EventHandlerList handlerList1 = null;
                handlerList1 += set3.Reg(1, _set3_action1);
                Log.Assert(!set3.Reg(1, _set3_action1).Valid, "未通过: 重复添加的测试");
                handlerList1 += set3.Reg(1, _set3_action2);
                handlerList1 += set3.Reg(1, _set3_action3);
                set3.Fire(1, "set3 Hello");
                handlerList1.Destroy();

                set3.Fire(1, "未通过: set3 Hello2");
            }

        }


        private static void _set1_action1(string msg)
        {
            UnityEngine.Debug.Log($"_set1_action1 {msg}");
        }

        private static void _set1_action2(int msg)
        {
            UnityEngine.Debug.Log($"_set1_action2 {msg}");
        }


        private static void _set2_action1(Event2Msg1 msg)
        {
            UnityEngine.Debug.Log($"_set2_action1 {msg.msg}");
        }

        private static void _set3_action1()
        {
            UnityEngine.Debug.Log($"_set3_action1 ");
        }

        private static void _set3_action2(string v)
        {
            UnityEngine.Debug.Log($"_set3_action2 {v}");
        }

        private static void _set3_action3(int key, string v)
        {
            UnityEngine.Debug.Log($"_set3_action3 {key} {v}");
        }

    }



}
#endif