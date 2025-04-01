/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    /// <summary>
    /// 带有引用计数的指针，需要对象自己来实现
    /// </summary>
    public interface ISPtr : IVersionObj
    {
        public int IncRef();
        public int DecRef();
        public int RefCount { get; }
    }


    public interface ICSPtr<T> : ICPtr where T : ISPtr
    {
        public T Val { get; }
    }

    internal sealed class CSPtr<T> : CPoolItemBase, ICSPtr<T> where T : class, ISPtr
    {
        private VersionObj<T> _target;
        public T Val => _target;

        public static CSPtr<T> Create(T target)
        {
            if (target == null)
                return null;
            var ret = GPool.New<CSPtr<T>>();
            ret._target = new VersionObj<T>(target);
            target.IncRef();
            return ret;
        }

        protected override void OnPoolRelease()
        {
            _target.Val?.DecRef();
            _target = default;
        }
    }

    /// <summary>
    /// SharedPtr 指针, 需要有引用计数 <br/>
    /// SPtr&lt;T&gt; 不要出现在函数参数, 只能出现在成员变量里面<br/>
    /// </summary>
    public struct SPtr<T> where T : class, ISPtr
    {
        //接口类型的T,不能隐式转换
        public static implicit operator T(SPtr<T> ptr) { return ptr.Val; }
        public static implicit operator SPtr<T>(T target) { return new SPtr<T>(target); }

        private CPtr<ICSPtr<T>> _target;

        public SPtr(T target)
        {
            if (target == null)
                _target = default;
            else
            {
                _target = new CPtr<ICSPtr<T>>(CSPtr<T>.Create(target));
            }
        }

        public void Destroy()
        {
            _target.Destroy();
        }

        public bool Null => _target.Null;

        public T Val => _target.Val?.Val;
    }

    public static partial class PtrExt
    {
        public static SPtr<T> SPtr<T>(this T self) where T : class, ISPtr { return new SPtr<T>(self); }
        public static ICSPtr<T> ExtCreateCSPtr<T>(this T self) where T:class ,ISPtr
        {
            return CSPtr<T>.Create(self);
        }
    }

    public abstract class SPoolItemBase : IPoolItem, ISPtr, IDisposable
    {
        private IPool ___pool = null;
        private bool ___in_pool = false;
        IPool IPoolItem.Pool { get => ___pool; set => ___pool = value; }
        bool IPoolItem.InPool { get => ___in_pool; set => ___in_pool = value; }

        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        private int __ref_count = 0;
        public int IncRef()
        {
            __ref_count++;
            return __ref_count;
        }

        public int DecRef()
        {
            __ref_count--;
            if (__ref_count > 0)
                return __ref_count;

            if (___pool == null || ___pool.Del(this))
            {
                __ref_count = 0;
                ___obj_ver++;
                OnPoolRelease();
                return 0;
            }
            return 0;
        }

        public int RefCount
        {
            get
            {
                return __ref_count;
            }
        }

        public void Dispose()
        {
            this.DecRef();
        }

        protected abstract void OnPoolRelease();
    }
}
