/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/8 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    public interface IDestroyable
    {
        void Destroy();
    }

    public interface IVersionObj
    {
        int ObjVersion { get; }
    }


    public struct VersionObj<T> where T : class, IVersionObj
    {
        //接口类型的T,不能隐式转换
        public static implicit operator T(VersionObj<T> ptr) { return ptr.Val; }
        public static implicit operator VersionObj<T>(T target) { return new VersionObj<T>(target); }

        private readonly int _ver;
        private T _target;
        public VersionObj(T target)
        {
            _target = target;
            _ver = -1;
            if (_target == null)
                return;
            _ver = target.ObjVersion;
        }

        public T Val
        {
            get
            {
                if (_target == null)
                    return null;
                if (_target.ObjVersion == _ver)
                    return _target;
                _target = null;
                return null;
            }
        }

        public bool Null { get { return Val == null; } }
    }



    /// <summary>
    /// C风格的指针，继承了才能用 CPtr
    /// </summary>
    public interface ICPtr : IDestroyable, IVersionObj
    {
    }

    /// <summary>
    /// C 风格的指针，一旦一个地方销毁，其他指向该指针的对象就失效 <br/>
    /// 不会出现c的野指针问题 <br/>
    /// 最好配合 PoolItem 来使用<br/>
    /// CPtr&lt;T&gt; 不要出现在函数参数, 只能出现在成员变量里面<br/>
    /// </summary>
    public struct CPtr<T> : IDisposable where T : class, ICPtr
    {
        //接口类型的T,不能隐式转换
        public static implicit operator T(CPtr<T> ptr) { return ptr.Val; }
        public static implicit operator CPtr<T>(T target) { return new CPtr<T>(target); }

        private readonly int _ver;
        private T _target;
        public CPtr(T target)
        {
            _target = target;
            _ver = -1;
            if (_target == null)
                return;
            _ver = target.ObjVersion;
        }

        public T Val
        {
            get
            {
                if (_target == null)
                    return null;
                if (_target.ObjVersion == _ver)
                    return _target;
                _target = null;
                return null;
            }
        }

        public bool Null { get { return Val == null; } }

        public void Destroy()
        {
            var t = Val;
            if (t == null)
                return;
            _target = default;
            t.Destroy();
        }

        public void Dispose()
        {
            Destroy();
        }
    }


    

    //接口类型的T,不能隐式转换,  所以增加两个扩展方法
    //就不用new 了,因为有些类的名字比较长
    public  static partial class PtrExt
    {
        public static CPtr<T> CPtr<T>(this T self) where T : class, ICPtr { return new CPtr<T>(self); }
    }
}
