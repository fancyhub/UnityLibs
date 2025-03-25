/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/09/01
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    internal struct EventSet1NodeData
    {
        private static int _id_gen = 0;
        public Delegate Action;
        public int Id;

        public EventSet1NodeData(Delegate action)
        {
            this.Action = action;
            _id_gen++;
            this.Id = _id_gen;
        }
    }

    /// <summary>
    /// Support <br/>
    /// 1.  Action&lt;TValue&gt;, Action&lt;TKey,TValue&gt; <br/>
    /// </summary>
    public class EventSet1<TKey> : ICPtr
    {
        public struct Handle
        {
            internal readonly CPtr<EventSet1<TKey>> Set;
            internal readonly int Id;
            internal readonly TKey Key;
            internal Handle(EventSet1<TKey> set, int id, TKey key)
            {
                this.Set = new CPtr<EventSet1<TKey>>(set);
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
                EventSetLog._.D("Unreg Handler {0}", Key);
                node.Value = default;
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

        private sealed class ActionList : CPoolItemBase
        {
            private TKey _key;
            private Type _value_type;
            private LinkedList<EventSet1NodeData> _list = new LinkedList<EventSet1NodeData>();
            private int _stack_count = 0;
            private bool _has_invalid_nodes = false;

            internal static ActionList Create(TKey key, Type value_type)
            {
                var ret = GPool.New<ActionList>();
                ret._key = key;
                ret._value_type = value_type;
                return ret;
            }

            public bool Fire<TValue>(ref TValue v)
            {
                if (typeof(TValue) != _value_type)
                {
                    EventSetLog._.Assert(false, "value type Is Different Key:{0}, Need ValueType: {1}, MsgType: {2}", _key, _value_type, typeof(TValue));
                    return false;
                }

                if (_stack_count > 3)
                {
                    EventSetLog._.Assert(false, "action in stack more than {0} times", _stack_count + 1);
                    return false;
                }
                _stack_count++;
                bool retval = true;

                try
                {
                    int list_count = _list.Count;
                    int total_count = Math.Max(list_count, 5) * 2;
                    int count = 0;
                    var node = _list.First;
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

                        EventSet1NodeData cur_node_data = node.Value;
                        node = node.Next;
                        if (cur_node_data.Action == null)
                        {
                            _has_invalid_nodes = true;
                            continue;
                        }

                        if (cur_node_data.Action is Action<TValue> action0)
                            action0(v);
                        else if (cur_node_data.Action is Action<TKey, TValue> action1)
                            action1(_key, v);
                        else
                            EventSetLog._.Assert(false, "Action Convert Fail, ActionType: {0}, ParamType: {1}", cur_node_data.GetType(), typeof(TValue));
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

            public LinkedListNode<EventSet1NodeData> Add(Type value_type, Delegate action)
            {
                if (action == null)
                {
                    EventSetLog._.Assert(false, "Can't reg null action");
                    return null;
                }

                if (value_type != _value_type)
                {
                    EventSetLog._.Assert(false, "can't reg, value type Is Different Key:{0}, Need ValueType: {1}, NewValueType: {2}", _key, _value_type, value_type);
                    return null;
                }

                // Cant Add Twice
                if (_Find(action) != null)
                {
                    EventSetLog._.Assert(false, "Can't reg twice {0} {1}", _key, action);
                    return null;
                }

                _RemoveInvalidNodes();
                EventSetLog._.D("Reg Handler {0}", _key);
                return _list.ExtAddLast(new EventSet1NodeData(action));
            }

            public bool IsEmpty() { return _list.Count == 0; }

            protected override void OnPoolRelease()
            {
                _list.ExtClear();
            }


            private LinkedListNode<EventSet1NodeData> _Find(Delegate action)
            {
                var node = _list.First;
                for (; ; )
                {
                    if (node == null)
                        return null;
                    if (node.Value.Action == null)
                    {
                        _has_invalid_nodes = true;
                        continue;
                    }
                    else if (node.Value.Action == action)
                        return node;
                    node = node.Next;
                }
            }

            private void _RemoveInvalidNodes()
            {
                if (_stack_count > 0 || !_has_invalid_nodes)
                    return;
                _has_invalid_nodes = false;
                var node = _list.First;
                for (; ; )
                {
                    if (node == null)
                        break;

                    var t = node;
                    node = node.Next;

                    if (t.Value.Action == null)
                        t.ExtRemoveFromList();
                }
            }
        }

        private Dictionary<TKey, ActionList> _key_map;
        private Dictionary<int, LinkedListNode<EventSet1NodeData>> _id_map;

        private int __ptr_ver = 0;
        int ICPtr.PtrVer { get => __ptr_ver; }

        public EventSet1(IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = EqualityComparer<TKey>.Default;
            _key_map = new Dictionary<TKey, ActionList>(equality_comparer);
            _id_map = new Dictionary<int, LinkedListNode<EventSet1NodeData>>();
        }

        public EventSet1(int cap, IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = EqualityComparer<TKey>.Default;
            _key_map = new Dictionary<TKey, ActionList>(cap, equality_comparer);
            _id_map = new Dictionary<int, LinkedListNode<EventSet1NodeData>>(cap);
        }

        public Handle Reg<TValue>(TKey key, Action<TValue> action) { return _RegDelegate(key, typeof(TValue), action); }
        public Handle Reg<TValue>(TKey key, Action<TKey, TValue> action) { return _RegDelegate(key, typeof(TValue), action); }

        private Handle _RegDelegate(TKey key, Type value_type, Delegate action)
        {
            if (action == null)
            {
                EventSetLog._.Assert(false, "Can't reg null action {0}", key);
                return default;
            }

            if (!_key_map.TryGetValue(key, out var action_list))
            {
                action_list = ActionList.Create(key, value_type);
                _key_map.Add(key, action_list);
            }
            var node = action_list.Add(value_type, action);
            if (node == null)
                return default;
            _id_map[node.Value.Id] = node;
            return new Handle(this, node.Value.Id, key);
        }

        //立即发消息
        public bool Fire<TValue>(TKey key, TValue val)
        {
            if (!_key_map.TryGetValue(key, out var action_list))
                return false;
            return action_list.Fire(ref val);
        }

        public void Clear()
        {
            _key_map.Clear();
        }

        public void Destroy()
        {
            Clear();
            __ptr_ver++;
        }
    }

    public sealed class EventSet1Auto<TKey> : CPoolItemBase
    {
        private CPtr<EventSet1<TKey>> _set;
        private EventSet1<TKey>.HandleList _list;

        public static EventSet1Auto<TKey> Create(EventSet1<TKey> set)
        {
            if (set == null)
                return null;
            var ret = GPool.New<EventSet1Auto<TKey>>();
            ret._set = set;
            return ret;
        }

        public EventSet1Auto<TKey> Reg<TValue>(TKey key, Action<TValue> action)
        {
            EventSet1<TKey> set = _set;
            if (set == null)
                return this;
            _list += set.Reg(key, action);
            return this;
        }

        public EventSet1Auto<TKey> Reg<TValue>(TKey key, Action<TKey, TValue> action)
        {
            EventSet1<TKey> set = _set;
            if (set == null)
                return this;
            _list += set.Reg(key, action);
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