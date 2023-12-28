using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.UI.Ed
{
    public class EdUITran
    {
        public Vector2 pos;
        public Vector2 scale;
        public Vector2 rot;
        public Vector2 size;
    }

    public class EdUIContext
    {
        public EdUIEventSystem EventSystem = new EdUIEventSystem();
        public EdUIContext()
        {

        }
    }

    public abstract class EdUIComp
    {
        public List<EdUIComp> _children;
        public EdUITran _tran = new EdUITran();
        public EdUIComp _parent;
        public EdUIContext _context;
        public bool _visible = true;

        public string Name { get; set; }
        public EdUITran Tran { get { return _tran; } }

        public virtual EdUIComp Parent
        {
            get => _parent;
            set
            {
                if (_parent == value)
                    return;

                if (value == null)
                    _parent.RemoveChild(this);
                else
                    value.AddChild(this);
            }
        }

        public virtual int ChildCount
        {
            get
            {
                return _children == null ? 0 : _children.Count;
            }
        }

        public virtual EdUIComp GetChild(int index)
        {
            if (_children == null)
                return null;
            if (index < 0 || index >= _children.Count)
                return null;
            return _children[index];
        }

        public virtual bool AddChild(EdUIComp child)
        {
            if (child == null || child == this)
                return false;

            //如果自己是 child的 child, 就不能添加, 要不然循环了
            if (child.IsChildRecursive(this))
                return false;

            if (child._parent != null)
                child._parent.RemoveChild(child);

            if (_children == null)
                _children = new List<EdUIComp>();
            _children.Add(child);
            child._parent = this;
            child._set_context(_context);
            return true;
        }

        public virtual bool RemoveChild(EdUIComp child)
        {
            if (child == null)
                return false;
            if (child._parent != this)
                return false;

            bool ret = _children.Remove(child);
            child._parent = null;
            child._set_context(null);
            return ret;
        }

        /// <summary>
        /// 这个会不断获取 对象的parent.parent , 如果指向自己,就是true
        /// </summary>        
        public bool IsChildRecursive(EdUIComp comp)
        {
            if (comp == null)
                return false;
            if (comp == this)
                return false;

            EdUIComp p = comp.Parent;
            for (; ; )
            {
                if (p == null)
                    return false;
                if (p == this)
                    return true;
                p = p.Parent;
            }
        }

        public virtual void Draw(Vector2 size)
        {
            if (!Visible)
                return;
            OnDraw(size);
            _draw_children(size);
        }

        public void _draw_children(Vector2 size)
        {
            if (_children == null)
                return;

            foreach (var p in _children)
            {
                p?.Draw(size);
            }
        }

        public virtual void OnDraw(Vector2 size)
        {
        }

        public virtual bool Visible { get => _visible; set => _visible = value; }

        public void _set_context(EdUIContext context)
        {
            if (_context == context)
                return;
            _context = context;
            if (_children == null)
                return;

            foreach (var p in _children)
                p?._set_context(context);
        }
    }
}
