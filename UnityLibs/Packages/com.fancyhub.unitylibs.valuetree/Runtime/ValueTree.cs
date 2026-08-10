/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/7 16:25:20
 * Title   : 
 * Desc    : 
*************************************************************************************/

namespace FH
{
    public sealed class ValueTree<T> : IPoolItem, ICPtr
    {
        private char _PathSeparator = '.';
        
        #region IPoolItem, ICPtr
        private IPool ___pool = null;
        private bool ___in_pool = false;
        IPool IPoolItem.Pool { get => ___pool; set => ___pool = value; }
        bool IPoolItem.InPool { get => ___in_pool; set => ___in_pool = value; }
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;
        #endregion
                

        public T Data { get; set; }
        //自己在父节点的名字
        private Idx _Key;
        private ValueTree<T> _Parent;
        private MyDict<Idx, ValueTree<T>> _Children;

        private ValueTree()
        {
            _Children = new MyDict<Idx, ValueTree<T>>();
        }
        public bool Contains(Idx key)
        {
            return _Children.ContainsKey(key);
        }

        public Idx Key { get { return _Key; } }

        public ValueTree<T> Parent { get { return _Parent; } }

        public ValueTree<T> GetByPath(Str path, bool auto_create)
        {
            if (path.IsEmpty())
                return null;

            ValueTree<T> temp = this;
            foreach (var sub in path.Split(_PathSeparator))
            {
                temp = temp.Get(sub, auto_create);
                if (temp == null)
                    return null;
            }
            return temp;
        }

        public ValueTree<T> Get(Idx key, bool auto_create)
        {
            _Children.TryGetValue(key, out ValueTree<T> c);
            if (c != null)
                return c;

            if (auto_create)
            {
                c = ValueTree<T>.Create(_PathSeparator);
                c._Key = key;
                c._Parent = this;
                _Children.Add(key, c);
            }
            return c;
        }

        public void ClearChildren()
        {
            foreach (var p in _Children)
            {
                p.Value._InnerDestroy();
            }
            _Children.Clear();
        }

        // 没有会添加
        // 如果v==null，是删除
        // 如果存在，就修改，旧的被destroy
        public void Set(Idx key, ValueTree<T> v)
        {
            if (v != null && v._Parent != null)
            {
                //报错
                return;
            }

            _Children.TryGetValue(key, out ValueTree<T> old_val);
            if (old_val != null)
            {
                old_val._InnerDestroy();
                _Children.Remove(key);
            }

            if (v == null)
                return;

            v._Parent = this;
            v._Key = key;
            _Children[key] = v;
        }

        public ValueTree<T> this[string path]
        {
            get
            {
                if (string.IsNullOrEmpty(path))
                    return null;

                if (path.IndexOf(_PathSeparator) < 0)
                    return Get(path, false);

                Str str = path;
                ValueTree<T> v = this;
                foreach (var p in str.Split(_PathSeparator))
                {
                    v = v.Get(p, false);
                    if (v == null)
                        return null;
                }
                return v;
            }
            set
            {
                if (string.IsNullOrEmpty(path))
                    return;

                if (path.IndexOf(_PathSeparator) < 0)
                {
                    Set(path, value);
                    return;
                }

                Str str = path;
                ValueTree<T> v = this;
                foreach (var p in str.Split(_PathSeparator))
                    v = v.Get(p, true);

                ValueTree<T> parent = v._Parent;
                parent.Set(path, value);
            }
        }

    
        public static implicit operator T(ValueTree<T> v)
        {
            return v.Data;
        }

        public int Count { get => _Children.Count; }

        public MyDict<Idx, ValueTree<T>> GetChildren()
        {
            return _Children;
        }

        public static ValueTree<T> Create(char pathSeparator)
        {
            var ret = GPool.New<ValueTree<T>>(() => new ValueTree<T>());

            if(ret!=null)
            {
                ret._PathSeparator = pathSeparator;
            }
            return ret;
        }

        public void Destroy()
        {
            if (_Parent != null)
            {
                _Parent.Set(_Key, null);
                return;
            }

            _InnerDestroy();
        }

        //只是销毁
        private void _InnerDestroy()
        {
            foreach (var p in _Children)
            {
                p.Value._InnerDestroy();
            }
            _Children.Clear();
            _Parent = null;
            Data = default;

            ___obj_ver++;
            ___pool.Del(this);
        }
    }


    public static class ValTreeExt
    {
        public static ValueTree<T> ExtGetByPath<T>(this ValueTree<T> self, Str path, bool auto_create = false)
        {
            if (self == null)
                return null;
            return self.GetByPath(path, auto_create);
        }

        public static ValueTree<T> ExtSetByPath<T>(this ValueTree<T> self, Str path, T v)
        {
            if (self == null)
                return null;
            ValueTree<T> node = self.GetByPath(path, true);
            node.Data = v;
            return node;
        }

        public static void ExtDelByPath<T>(this ValueTree<T> self, Str path)
        {
            if (self == null)
                return;           

            var node = self.GetByPath(path, false);
            if (node == null)
                return;
            node.Destroy();
        }

       
    }
}
