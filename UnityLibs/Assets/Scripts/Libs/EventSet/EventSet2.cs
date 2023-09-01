/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/09/01
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;

namespace FH
{
    public interface IEventSet2Handler<Tkey, TValue>
    {
        void HandleEvent(Tkey key, TValue value);
    }

    /// <summary>
    /// Support <br/>
    /// 1. Delay Msg  <br/>
    /// 2. IEventSet2Handler, Action, Action&lt;TValue&gt;, Action&lt;TKey,TValue&gt; <br/>
    /// </summary>
    public class EventSet2<TKey, TValue> : ICPtr
    {
        private sealed class ActionList : CPoolItemBase
        {
            private TKey _key;
            private LinkedList<object> _list = new LinkedList<object>();

            internal static ActionList Create(TKey key)
            {
                var ret = GPool.New<ActionList>();
                ret._key = key;
                return ret;
            }

            public bool Fire(ref TValue v)
            {
                try
                {
                    var node = _list.First;
                    for (; ; )
                    {
                        if (node == null)
                            break;
                        if (node.List != _list) //Next Node is Removed when call current action
                            break;

                        var cur = node.Value;
                        node = node.Next;
                        if (cur == null)
                            continue;
                        if (cur is IEventSet2Handler<TKey, TValue> handler)
                            handler.HandleEvent(_key, v);
                        else if (cur is Action a)
                            a();
                        else if (cur is Action<TValue> a_value)
                            a_value(v);
                        else if (cur is Action<TKey, TValue> a_pair)
                            a_pair(_key, v);
                    }
                }
                catch (System.Exception e)
                {
                    Log.E(e);
                    return false;
                }
                return true;
            }

            public bool Add(object action)
            {
                if (action == null)
                {
                    Log.Assert(false, "Can't reg null action");
                    return false;
                }

                // Cant Add Twice
                if (_list.Find(action) != null)
                {
                    Log.Assert(false, "Can't reg twice {0} {1}", _key, action);
                    return false;
                }

                _list.ExtAddLast(action);
                return true;
            }

            public bool Remove(object action) { return _list.ExtRemove(action); }

            public bool IsEmpty() { return _list.Count == 0; }

            protected override void OnPoolRelease()
            {
                _list.ExtClear();
            }
        }

        private Dictionary<TKey, ActionList> _dict;
        private Queue<(TKey key, TValue value)> _event_queue;

        private int __ptr_ver = 0;
        int ICPtr.PtrVer { get => __ptr_ver; }

        public EventSet2(IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = EqualityComparer<TKey>.Default;
            _dict = new Dictionary<TKey, ActionList>(equality_comparer);
        }

        public EventSet2(int cap, IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = EqualityComparer<TKey>.Default;
            _dict = new Dictionary<TKey, ActionList>(cap, equality_comparer);
        }

        public bool Reg(TKey key, Action action) { return _RegDelegate(key, action); }

        // In Case TKey == TValue
        //public bool Reg(TKey key, Action<TKey> action) { return RegDelegate(key, action); }

        public bool Reg(TKey key, Action<TValue> action) { return _RegDelegate(key, action); }

        public bool Reg(TKey key, Action<TKey, TValue> action) { return _RegDelegate(key, action); }

        public bool Reg(TKey key, IEventSet2Handler<TKey, TValue> handler)
        {
            if (handler == null)
            {
                Log.Assert(false, "Can't reg null action {0}", key);
                return false;
            }

            if (!_dict.TryGetValue(key, out var action_list))
            {
                action_list = ActionList.Create(key);
                _dict.Add(key, action_list);
            }
            return action_list.Add(handler);
        }

        public bool Unreg(TKey key, Action action) { return UnregCallBack(key, action); }

        public bool Unreg(TKey key, Action<TValue> action) { return UnregCallBack(key, action); }

        public bool Unreg(TKey key, Action<TKey, TValue> action) { return UnregCallBack(key, action); }

        public bool Unreg(TKey key, IEventSet2Handler<TKey, TValue> handler) { return UnregCallBack(key, handler); }

        public bool UnregCallBack(TKey key, object call_back)
        {
            if (call_back == null)
                return false;

            if (!_dict.TryGetValue(key, out var action_list))
                return false;

            if (!action_list.Remove(call_back))
                return false;

            if (action_list.IsEmpty())
            {
                action_list.Destroy();
                _dict.Remove(key);
            }
            return true;
        }

        //立即发消息
        public bool Fire(TKey key, TValue val)
        {
            if (!_dict.TryGetValue(key, out var action_list))
                return false;
            return action_list.Fire(ref val);
        }

        public bool FireDelay(TKey key, TValue val)
        {
            if (_event_queue == null)
                _event_queue = new();
            _event_queue.Enqueue((key, val));
            return true;
        }

        /// <summary>
        /// 只处理当前队列里面的消息，如果在这个过程中，又有新的消息来了，不处理
        /// </summary>        
        public int ProcessEvents(List<(TKey, TValue)> processed_events = null)
        {
            if (_event_queue == null)
                return 0;
            int ret = 0;
            int count = _event_queue.Count;
            for (int i = 0; i < count; ++i)
            {
                if (_event_queue.Count == 0)
                    break;

                var (key, value) = _event_queue.Dequeue();
                processed_events?.Add((key, value));
                Fire(key, value);
                ret++;
            }
            return ret;
        }

        public int ProcessAllEvents(List<(TKey, TValue)> processed_events = null)
        {
            if (_event_queue == null)
                return 0;

            int ret = 0;
            for (; ; )
            {
                if (_event_queue.Count == 0)
                    break;
                var (key, value) = _event_queue.Dequeue();
                processed_events?.Add((key, value));

                Fire(key, value);
                ret++;
            }
            return ret;
        }

        public void ClearEventQueue()
        {
            _event_queue?.Clear();
        }

        public void Clear()
        {
            _dict.Clear();
            _event_queue?.Clear();
        }

        public void Destroy()
        {
            Clear();
            __ptr_ver++;
        }

        private bool _RegDelegate(TKey key, Delegate action)
        {
            if (action == null)
            {
                Log.Assert(false, "Can't reg null action {0}", key);
                return false;
            }

            if (!_dict.TryGetValue(key, out var action_list))
            {
                action_list = ActionList.Create(key);
                _dict.Add(key, action_list);
            }
            return action_list.Add(action);
        }
    }

    public sealed class EventSet2Auto<TKey, TValue> : CPoolItemBase
    {
        private CPtr<EventSet2<TKey, TValue>> _set;
        private LinkedList<(TKey key, object callback)> _list = new();

        public static EventSet2Auto<TKey, TValue> Create(EventSet2<TKey, TValue> set)
        {
            if (set == null)
                return null;

            var ret = GPool.New<EventSet2Auto<TKey, TValue>>();
            ret._set = set;
            return ret;
        }

        public EventSet2Auto<TKey, TValue> Reg(TKey key, Action action)
        {
            EventSet2<TKey, TValue> set = _set;
            if (set != null && set.Reg(key, action))
                _list.ExtAddLast((key, action));
            return this;
        }

        public EventSet2Auto<TKey, TValue> Reg(TKey key, Action<TValue> action)
        {
            EventSet2<TKey, TValue> set = _set;
            if (set != null && set.Reg(key, action))
                _list.ExtAddLast((key, action));
            return this;
        }

        public EventSet2Auto<TKey, TValue> Reg(TKey key, Action<TKey, TValue> action)
        {
            EventSet2<TKey, TValue> set = _set;
            if (set != null && set.Reg(key, action))
                _list.ExtAddLast((key, action));
            return this;
        }

        public EventSet2Auto<TKey, TValue> Reg(TKey key, IEventSet2Handler<TKey, TValue> handler)
        {
            EventSet2<TKey, TValue> set = _set;
            if (set != null && set.Reg(key, handler))
                _list.ExtAddLast((key, handler));
            return this;
        }

        protected override void OnPoolRelease()
        {
            EventSet2<TKey, TValue> set = _set;
            if (set != null)
            {
                foreach (var p in _list)
                    set.UnregCallBack(p.key, p.callback);
            }
            _list.ExtClear();
            _set = null;
        }
    }

}