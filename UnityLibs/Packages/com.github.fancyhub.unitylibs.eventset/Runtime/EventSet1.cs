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
    internal struct EventSet1EventNode
    {
        public Delegate Action;
        public int Id;
        public static int _id_gen = 0;

        public EventSet1EventNode(Delegate action)
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
        public struct EventHandler
        {
            internal readonly CPtr<EventSet1<TKey>> Set;
            internal readonly int Id;
            internal EventHandler(EventSet1<TKey> set, int id)
            {
                this.Set = new CPtr<EventSet1<TKey>>(set);
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

        internal sealed class ActionList : CPoolItemBase
        {
            private TKey _key;
            private Type _value_type;
            private LinkedList<EventSet1EventNode> _list = new LinkedList<EventSet1EventNode>();

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
                    Log.Assert(false, "value type Is Different Key:{0}, Need ValueType: {1}, MsgType: {2}", _key, _value_type, typeof(TValue));
                    return false;
                }

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
                        if (cur.Action == null)
                            continue;

                        if (cur.Action is Action<TValue> a_value)
                            a_value(v);
                        else if (cur.Action is Action<TKey, TValue> a_pair)
                            a_pair(_key, v);
                        else
                            Log.Assert(false, "Action Convert Fail, ActionType: {0}, ParamType: {1}", cur.GetType(), typeof(TValue));
                    }
                }
                catch (System.Exception e)
                {
                    Log.E(e);
                    return false;
                }
                return true;
            }

            public LinkedListNode<EventSet1EventNode> Add(Type value_type, Delegate action)
            {
                if (action == null)
                {
                    Log.Assert(false, "Can't reg null action");
                    return null;
                }

                if (value_type != _value_type)
                {
                    Log.Assert(false, "can't reg, value type Is Different Key:{0}, Need ValueType: {1}, NewValueType: {2}", _key, _value_type, value_type);
                    return null;
                }

                // Cant Add Twice
                if (_Find(action) != null)
                {
                    Log.Assert(false, "Can't reg twice {0} {1}", _key, action);
                    return null;
                }

                return _list.ExtAddLast(new EventSet1EventNode(action));
            }

            public bool IsEmpty() { return _list.Count == 0; }

            protected override void OnPoolRelease()
            {
                _list.ExtClear();
            }


            private LinkedListNode<EventSet1EventNode> _Find(Delegate action)
            {
                var node = _list.First;
                for (; ; )
                {
                    if (node == null)
                        return null;
                    if (node.Value.Action == action)
                        return node;

                    node = node.Next;
                }
            }

        }

        private Dictionary<TKey, ActionList> _key_map;
        private Dictionary<int, LinkedListNode<EventSet1EventNode>> _id_map;

        private int __ptr_ver = 0;
        int ICPtr.PtrVer { get => __ptr_ver; }

        public EventSet1(IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = EqualityComparer<TKey>.Default;
            _key_map = new Dictionary<TKey, ActionList>(equality_comparer);
            _id_map = new Dictionary<int, LinkedListNode<EventSet1EventNode>>();
        }

        public EventSet1(int cap, IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = EqualityComparer<TKey>.Default;
            _key_map = new Dictionary<TKey, ActionList>(cap, equality_comparer);
            _id_map = new Dictionary<int, LinkedListNode<EventSet1EventNode>>(cap);
        }

        public EventHandler Reg<TValue>(TKey key, Action<TValue> action) { return _RegDelegate(key, typeof(TValue), action); }
        public EventHandler Reg<TValue>(TKey key, Action<TKey, TValue> action) { return _RegDelegate(key, typeof(TValue), action); }


        private EventHandler _RegDelegate(TKey key, Type value_type, Delegate action)
        {
            if (action == null)
            {
                Log.Assert(false, "Can't reg null action {0}", key);
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
            return new EventHandler(this, node.Value.Id);
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
        private EventSet1<TKey>.EventHandlerList _list;

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