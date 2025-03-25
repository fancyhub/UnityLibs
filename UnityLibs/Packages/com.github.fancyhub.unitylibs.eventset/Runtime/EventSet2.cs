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
        private struct NodeData
        {
            private static int _id_gen = 0;
            public readonly int Type; //1:IHandler, 2: Action, 3: Action<TValue> 4:Action<TKey,TValue>
            public readonly IHandler Handler;
            public readonly Action Action;
            public readonly Action<TValue> Action1;
            public readonly Action<TKey, TValue> Action2;
            public readonly int Id;

            public NodeData(IHandler handler)
            {
                this.Type = 1;
                this.Handler = handler;
                this.Action = null;
                this.Action1 = null;
                this.Action2 = null;
                this.Id = 0;

                if (handler != null)
                {
                    _id_gen++;
                    this.Id = _id_gen;
                }
            }

            public NodeData(Action action)
            {
                this.Type = 2;
                this.Handler = null;
                this.Action = action;
                this.Action1 = null;
                this.Action2 = null;
                this.Id = 0;

                if (action != null)
                {
                    _id_gen++;
                    this.Id = _id_gen;
                }
            }

            public NodeData(Action<TValue> action)
            {
                this.Type = 3;
                this.Handler = null;
                this.Action = null;
                this.Action1 = action;
                this.Action2 = null;
                this.Id = 0;

                if (action != null)
                {
                    _id_gen++;
                    this.Id = _id_gen;
                }
            }

            public NodeData(Action<TKey, TValue> action)
            {
                this.Type = 4;
                this.Handler = null;
                this.Action = null;
                this.Action1 = null;
                this.Action2 = action;
                this.Id = 0;

                if (action != null)
                {
                    _id_gen++;
                    this.Id = _id_gen;
                }
            }

            public bool IsValid()
            {
                return Id != 0;
            }

            public bool IsEqual(NodeData node)
            {
                if (Id == 0)
                    return false;
                if (node.Type != Type)
                    return false;

                switch (Type)
                {
                    case 1:
                        return Handler == node.Handler;
                    case 2:
                        return Action == node.Action;
                    case 3:
                        return Action1 == node.Action1;
                    case 4:
                        return Action2 == node.Action2;
                    default:
                        return true;
                }
            }
        }

        public struct Handle
        {
            internal readonly CPtr<EventSet2<TKey, TValue>> Set;
            internal readonly int Id;
            internal readonly TKey Key;
            internal Handle(EventSet2<TKey, TValue> set, int id, TKey key)
            {
                this.Set = new CPtr<EventSet2<TKey, TValue>>(set);
                this.Id = id;
                this.Key = key;
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
                node.Value = default;

                EventSetLog._.D("Unreg Handler {0}", Key);
            }
        }

        public sealed class HandleList : CPoolItemBase
        {
            private LinkedList<Handle> _list = new LinkedList<Handle>();
            protected override void OnPoolRelease()
            {
                foreach (var p in _list)
                    p.Destroy();
                _list.ExtClear();
            }

            public static HandleList operator +(HandleList list, Handle handle)
            {
                if (!handle.Valid)
                    return list;

                if (list == null)
                    list = GPool.New<HandleList>();
                list._list.ExtAddLast(handle);
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
            private LinkedList<NodeData> _list = new LinkedList<NodeData>();
            private int _stack_count = 0;
            private bool _dirty = false;

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
                    EventSetLog._.Assert(false, "action in stack more than {0} times", _stack_count + 1);
                    return false;
                }

                _stack_count++;
                bool retval = true;

                try
                {
                    var node = _list.First;
                    var list_count = _list.Count;
                    int total_count = Math.Max(list_count, 5) * 2;
                    int count = 0;
                    for (; ; )
                    {
                        if (node == null)
                            break;
                        if (node.List != _list) //Next Node is Removed when call current action
                            break;
                        count++;
                        if (count > total_count)
                        {
                            EventSetLog._.Assert(false, "dead loop, {0}, {1}/{2}", _key, count, list_count);
                            break;
                        }
                        NodeData cur = node.Value;
                        node = node.Next;

                        if (!cur.IsValid())
                        {
                            _dirty = true;
                            continue;
                        }                       
                        switch (cur.Type)
                        {
                            case 1:
                                cur.Handler?.HandleEvent(_key, v);
                                break;

                            case 2:
                                cur.Action?.Invoke();
                                break;

                            case 3:
                                cur.Action1.Invoke(v);
                                break;

                            case 4:
                                cur.Action2?.Invoke(_key, v);
                                break;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    EventSetLog._.E(e);
                    retval = false;
                }

                _stack_count--;

                _RemoveInvalidNodes();
                return retval;
            }

            private void _RemoveInvalidNodes()
            {
                if (_stack_count > 0 || !_dirty)
                    return;
                _dirty = false;

                var node = _list.First;
                for (; ; )
                {
                    if (node == null)
                        break;

                    var t = node;
                    node = node.Next;

                    if (!t.Value.IsValid())
                    {
                        t.ExtRemoveFromList();
                    }
                }
            }

            public LinkedListNode<NodeData> Add(NodeData data)
            {
                if (!data.IsValid())
                {
                    EventSetLog._.Assert(false, "Can't reg null action");
                    return null;
                }

                // Cant Add Twice
                if (_Find(data) != null)
                {
                    EventSetLog._.Assert(false, "Can't reg twice key:{0} action:{1}", _key, data);
                    return null;
                }
                EventSetLog._.D("Reg Handler {0}", _key);
                _RemoveInvalidNodes();
                return _list.ExtAddLast(data);
            }

            private LinkedListNode<NodeData> _Find(NodeData action)
            {
                if (!action.IsValid())
                    return null;

                var node = _list.First;
                for (; ; )
                {
                    if (node == null)
                        return null;
                    if (!node.Value.IsValid())
                    {
                        _dirty = true;
                    }
                    else if (action.IsEqual(node.Value))
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

        private Dictionary<int, LinkedListNode<NodeData>> _id_map;
        private Dictionary<TKey, ActionList> _key_map;
        private Queue<(TKey key, TValue value)> _event_queue;

        private int __ptr_ver = 0;
        int ICPtr.PtrVer { get => __ptr_ver; }

        public EventSet2(IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = MyEqualityComparer<TKey>.Default;
            _key_map = new Dictionary<TKey, ActionList>(equality_comparer);
            _id_map = new Dictionary<int, LinkedListNode<NodeData>>();
        }

        public EventSet2(int cap, IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = MyEqualityComparer<TKey>.Default;
            _key_map = new Dictionary<TKey, ActionList>(cap, equality_comparer);
            _id_map = new Dictionary<int, LinkedListNode<NodeData>>(cap);
        }

        public Handle Reg(TKey key, Action action) { return _Reg(key, new NodeData(action)); }

        public Handle Reg(TKey key, Action<TValue> action) { return _Reg(key, new NodeData(action)); }

        public Handle Reg(TKey key, Action<TKey, TValue> action) { return _Reg(key, new NodeData(action)); }

        public Handle Reg(TKey key, IHandler handler) { return _Reg(key, new NodeData(handler)); }

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


        private Handle _Reg(TKey key, NodeData data)
        {
            if (!data.IsValid())
            {
                EventSetLog._.Assert(false, "Can't reg null action {0}", key);
                return default;
            }

            if (!_key_map.TryGetValue(key, out var action_list))
            {
                action_list = ActionList.Create(key);
                _key_map.Add(key, action_list);
            }
            var node = action_list.Add(data);
            if (node == null)
                return default;

            _id_map[node.Value.Id] = node;
            return new Handle(this, node.Value.Id, key);
        }
    }

    public sealed class EventSet2Auto<TKey, TValue> : CPoolItemBase
    {
        private CPtr<EventSet2<TKey, TValue>> _set;
        private EventSet2<TKey, TValue>.HandleList _list;

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