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


    /// <summary>
    /// Support <br/>
    /// 1. Delay Msg  <br/>
    /// 2. IHandler, Action, Action&lt;TValue&gt;, Action&lt;TKey,TValue&gt; <br/>
    /// </summary>
    public class EventSet2<TKey, TValue> : ICPtr
    {
        internal struct EventSet2EventNode
        {
            public readonly IHandler Handler;
            public readonly Delegate Delegate;
            public readonly int Id;
            public static int _id_gen = 0;

            public EventSet2EventNode(IHandler handler)
            {
                this.Handler = handler;
                this.Delegate = null;
                _id_gen++;
                this.Id = _id_gen;
            }

            public EventSet2EventNode(Delegate action)
            {
                this.Handler = null;
                this.Delegate = action;
                _id_gen++;
                this.Id = _id_gen;
            }
        }

        public struct EventHandler
        {
            internal readonly CPtr<EventSet2<TKey, TValue>> Set;
            internal readonly int Id;
            internal EventHandler(EventSet2<TKey, TValue> set, int id)
            {
                this.Set = new CPtr<EventSet2<TKey, TValue>>(set);
                this.Id = id;
            }

            public bool Valid => !Set.Null && Id != 0;

            public void Destroy()
            {
                var set = Set.Val;
                if (set == null)
                    return;

                if (!set._id_map.TryGetValue(this.Id, out var node))
                    return;

                set._id_map.Remove(this.Id);
                node.ExtRemoveFromList();
            }
        }

        public sealed class EventHandlerList : CPoolItemBase
        {
            private LinkedList<EventHandler> _list = new LinkedList<EventHandler>();
            protected override void OnPoolRelease()
            {
                foreach (var p in _list)
                    p.Destroy();
                _list.ExtClear();
            }

            public static EventHandlerList operator +(EventHandlerList list, EventHandler handler)
            {
                if (!handler.Valid)
                    return list;

                if (list == null)
                    list = GPool.New<EventHandlerList>();
                list._list.ExtAddLast(handler);
                return list;
            }
        }

        public interface IHandler
        {
            void HandleEvent(TKey key, TValue value);
        }


        private sealed class ActionList : CPoolItemBase
        {
            private TKey _key;
            private LinkedList<EventSet2EventNode> _list = new LinkedList<EventSet2EventNode>();
            private int _stack_count = 0;

            internal static ActionList Create(TKey key)
            {
                var ret = GPool.New<ActionList>();
                ret._key = key;
                return ret;
            }

            public bool Fire(ref TValue v)
            {
                if (_stack_count > 3)
                {
                    Log.Assert(false, "action in stack more than {0} times", _stack_count + 1);
                    return false;
                }

                _stack_count++;
                bool retval = true;

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
                        if (cur.Handler != null)                        
                            cur.Handler.HandleEvent(_key, v);                                                
                        else if (cur.Delegate is Action action)
                            action();
                        else if (cur.Delegate is Action<TValue> action_value)
                            action_value(v);
                        else if (cur.Delegate is Action<TKey, TValue> action_pair)
                            action_pair(_key, v);
                    }
                }
                catch (System.Exception e)
                {
                    Log.E(e);
                    retval = false;
                }

                _stack_count--;
                return retval;
            }

            public LinkedListNode<EventSet2EventNode> AddHandler(IHandler action)
            {
                if (action == null)
                {
                    Log.Assert(false, "Can't reg null action");
                    return null;
                }

                // Cant Add Twice
                if (_FindHandler(action) != null)
                {
                    Log.Assert(false, "Can't reg twice key:{0} action:{1}", _key, action);
                    return null;
                }

                return _list.ExtAddLast(new EventSet2EventNode(action));
            }

            public LinkedListNode<EventSet2EventNode> AddDelegate(Delegate action)
            {
                if (action == null)
                {
                    Log.Assert(false, "Can't reg null action");
                    return null;
                }

                // Cant Add Twice
                if (_FindDelegate(action) != null)
                {
                    Log.Assert(false, "Can't reg twice key:{0} action:{1}", _key, action);
                    return null;
                }

                return _list.ExtAddLast(new EventSet2EventNode(action));
            }

            private LinkedListNode<EventSet2EventNode> _FindDelegate(Delegate action)
            {
                var node = _list.First;
                for (; ; )
                {
                    if (node == null)
                        return null;
                    if (node.Value.Delegate == action)
                        return node;

                    node = node.Next;
                }
            }

            private LinkedListNode<EventSet2EventNode> _FindHandler(IHandler action)
            {
                var node = _list.First;
                for (; ; )
                {
                    if (node == null)
                        return null;
                    if (node.Value.Handler == action)
                        return node;

                    node = node.Next;
                }
            }

            public bool IsEmpty() { return _list.Count == 0; }

            protected override void OnPoolRelease()
            {
                _list.ExtClear();
            }
        }

        private Dictionary<int, LinkedListNode<EventSet2EventNode>> _id_map;
        private Dictionary<TKey, ActionList> _key_map;
        private Queue<(TKey key, TValue value)> _event_queue;

        private int __ptr_ver = 0;
        int ICPtr.PtrVer { get => __ptr_ver; }

        public EventSet2(IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = MyEqualityComparer<TKey>.Default;
            _key_map = new Dictionary<TKey, ActionList>(equality_comparer);
            _id_map = new Dictionary<int, LinkedListNode<EventSet2EventNode>>();
        }

        public EventSet2(int cap, IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = MyEqualityComparer<TKey>.Default;
            _key_map = new Dictionary<TKey, ActionList>(cap, equality_comparer);
            _id_map = new Dictionary<int, LinkedListNode<EventSet2EventNode>>(cap);
        }

        public EventHandler Reg(TKey key, Action action) { return _RegDelegate(key, action); }

        public EventHandler Reg(TKey key, Action<TValue> action) { return _RegDelegate(key, action); }

        public EventHandler Reg(TKey key, Action<TKey, TValue> action) { return _RegDelegate(key, action); }

        public EventHandler Reg(TKey key, IHandler handler)
        {
            if (handler == null)
            {
                Log.Assert(false, "Can't reg null action {0}", key);
                return default;
            }

            if (!_key_map.TryGetValue(key, out var action_list))
            {
                action_list = ActionList.Create(key);
                _key_map.Add(key, action_list);
            }
            var node = action_list.AddHandler(handler);
            if (node == null)
                return default;

            _id_map[node.Value.Id] = node;
            return new EventHandler(this, node.Value.Id);
        }

        //立即发消息
        public bool Fire(TKey key, TValue val)
        {
            if (!_key_map.TryGetValue(key, out var action_list))
                return false;
            return action_list.Fire(ref val);
        }

        public bool FireAsync(TKey key, TValue val)
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
            _key_map.Clear();
            _event_queue?.Clear();
        }

        public void Destroy()
        {
            Clear();
            __ptr_ver++;
        }


        private EventHandler _RegDelegate(TKey key, Delegate action)
        {
            if (action == null)
            {
                Log.Assert(false, "Can't reg null action {0}", key);
                return default;
            }

            if (!_key_map.TryGetValue(key, out var action_list))
            {
                action_list = ActionList.Create(key);
                _key_map.Add(key, action_list);
            }
            var node = action_list.AddDelegate(action);
            if (node == null)
                return default;

            _id_map[node.Value.Id] = node;
            return new EventHandler(this, node.Value.Id);
        }
    }

    public sealed class EventSet2Auto<TKey, TValue> : CPoolItemBase
    {
        private CPtr<EventSet2<TKey, TValue>> _set;
        private EventSet2<TKey, TValue>.EventHandlerList _list;

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
            if (set == null)
                return this;
            _list += set.Reg(key, action);
            return this;
        }

        public EventSet2Auto<TKey, TValue> Reg(TKey key, Action<TValue> action)
        {
            EventSet2<TKey, TValue> set = _set;
            if (set == null)
                return this;
            _list += set.Reg(key, action);
            return this;
        }

        public EventSet2Auto<TKey, TValue> Reg(TKey key, Action<TKey, TValue> action)
        {
            EventSet2<TKey, TValue> set = _set;
            if (set == null)
                return this;
            _list += set.Reg(key, action);
            return this;
        }

        public EventSet2Auto<TKey, TValue> Reg(TKey key, EventSet2<TKey, TValue>.IHandler handler)
        {
            EventSet2<TKey, TValue> set = _set;
            if (set == null)
                return this;
            _list += set.Reg(key, handler);
            return this;
        }

        protected override void OnPoolRelease()
        {
            _set = null;
            _list?.Destroy();
            _list = null;
        }
    }
}