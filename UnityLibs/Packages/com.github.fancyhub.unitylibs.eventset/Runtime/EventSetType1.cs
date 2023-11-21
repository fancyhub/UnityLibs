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
    /// <summary>
    /// Support <br/>
    /// 1.  Action&lt;TValue&gt;, Action&lt;TKey,TValue&gt; <br/>
    /// </summary>
    public class EventSetType1<TKey> : ICPtr
    {
        internal sealed class ActionList : CPoolItemBase
        {
            private TKey _key;
            private Type _value_type;
            private LinkedList<Delegate> _list = new LinkedList<Delegate>();

            internal static ActionList Create(TKey key, Type value_type)
            {
                var ret = GPool.New<ActionList>();
                ret._key = key;
                ret._value_type = value_type;
                return ret;
            }

            public bool Fire<TValue>(ref TValue v)
            {
                Log.Assert(typeof(TValue) == _value_type, "类型不一致 Key:{0}, Need ValueType: {1}, MsgType: {2}", _key, _value_type, typeof(TValue));

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

                        if (cur is Action<TValue> a_value)
                            a_value(v);
                        else if (cur is Action<TKey, TValue> a_pair)
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

            public bool Add(Type value_type, Delegate action)
            {
                if (action == null)
                {
                    Log.Assert(false, "Can't reg null action");
                    return false;
                }

                if (value_type != _value_type)
                {
                    Log.Assert(false, "类型不一致");
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

            public bool Remove(Delegate action) { return _list.ExtRemove(action); }

            public bool IsEmpty() { return _list.Count == 0; }

            protected override void OnPoolRelease()
            {
                _list.ExtClear();
            }
        }

        private Dictionary<TKey, ActionList> _dict;

        private int __ptr_ver = 0;
        int ICPtr.PtrVer { get => __ptr_ver; }

        public EventSetType1(IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = EqualityComparer<TKey>.Default;
            _dict = new Dictionary<TKey, ActionList>(equality_comparer);
        }

        public EventSetType1(int cap, IEqualityComparer<TKey> equality_comparer = null)
        {
            if (equality_comparer == null)
                equality_comparer = EqualityComparer<TKey>.Default;
            _dict = new Dictionary<TKey, ActionList>(cap, equality_comparer);
        }

        public bool Reg<TValue>(TKey key, Action<TValue> action) { return RegDelegate(key, typeof(TValue), action); }
        public bool Reg<TValue>(TKey key, Action<TKey, TValue> action) { return RegDelegate(key, typeof(TValue), action); }

        public bool Unreg<TValue>(TKey key, Action<TValue> action) { return UnregDelegate(key, action); }
        public bool Unreg<TValue>(TKey key, Action<TKey, TValue> action) { return UnregDelegate(key, action); }


        public bool RegDelegate(TKey key, Type value_type, Delegate action)
        {
            if (action == null)
            {
                Log.Assert(false, "Can't reg null action {0}", key);
                return false;
            }

            if (!_dict.TryGetValue(key, out var action_list))
            {
                action_list = ActionList.Create(key, value_type);
                _dict.Add(key, action_list);
            }
            return action_list.Add(value_type, action);
        }

        public bool UnregDelegate(TKey key, Delegate action)
        {
            if (action == null)
                return false;

            if (!_dict.TryGetValue(key, out var action_list))
                return false;

            if (!action_list.Remove(action))
                return false;

            if (action_list.IsEmpty())
            {
                action_list.Destroy();
                _dict.Remove(key);
            }
            return true;
        }

        //立即发消息
        public bool Fire<TValue>(TKey key, TValue val)
        {
            if (!_dict.TryGetValue(key, out var action_list))
                return false;
            return action_list.Fire(ref val);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public void Destroy()
        {
            Clear();
            __ptr_ver++;
        }
    }

    public sealed class EventSetType1Auto<TKey> : CPoolItemBase
    {
        private CPtr<EventSetType1<TKey>> _set;
        private LinkedList<(TKey, Delegate)> _list = new LinkedList<(TKey, Delegate)>();

        public static EventSetType1Auto<TKey> Create(EventSetType1<TKey> set)
        {
            if (set == null)
                return null;
            var ret = GPool.New<EventSetType1Auto<TKey>>();
            ret._set = set;
            return ret;
        }

        public EventSetType1Auto<TKey> Reg<TValue>(TKey key, Action<TValue> action)
        {
            EventSetType1<TKey> set = _set;
            if (set != null && set.Reg<TValue>(key, action))
                _list.ExtAddLast((key, action));
            return this;
        }

        public EventSetType1Auto<TKey> Reg<TValue>(TKey key, Action<TKey, TValue> action)
        {
            EventSetType1<TKey> set = _set;
            if (set != null && set.Reg<TValue>(key, action))
                _list.ExtAddLast((key, action));
            return this;
        }

        protected override void OnPoolRelease()
        {
            EventSetType1<TKey> set = _set;
            if (set != null)
            {
                foreach (var p in _list)
                    set.UnregDelegate(p.Item1, p.Item2);
            }
            _list.ExtClear();
            _set = null;
        }
    }
}