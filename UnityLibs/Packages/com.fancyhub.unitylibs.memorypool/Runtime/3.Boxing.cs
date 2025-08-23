using System;

namespace FH
{
    public interface IBoxing : ICPtr
    {
        public System.Object GetObjValue();
        public System.Type GetValueType();
    }

    public sealed class Boxing<T> : IBoxing, IPoolItem, IEquatable<Boxing<T>>, IEquatable<T> where T : struct, IEquatable<T>
    {
        public T Value;

        private IPool ___pool = null;
        private bool ___in_pool = false;
        IPool IPoolItem.Pool { get => ___pool; set => ___pool = value; }
        bool IPoolItem.InPool { get => ___in_pool; set => ___in_pool = value; }
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        public void Destroy()
        {
            if (___pool == null || ___pool.Del(this))
            {
                ___obj_ver++;
                return;
            }
        }

        public void Dispose()
        {
            Destroy();
        }

        public bool Equals(Boxing<T> other)
        {
            return Value.Equals(other.Value);
        }

        public bool Equals(T other)
        {
            return Value.Equals(other);
        }

        public object GetObjValue()
        {
            return Value;
        }

        public Type GetValueType()
        {
            return typeof(T);
        }

        public static implicit operator T(Boxing<T> v) { return v.Value; }
    }

    public static class Boxing
    {
        public static Boxing<T> Create<T>(T v) where T : struct, IEquatable<T>
        {
            var ret = GPool.New<Boxing<T>>();
            ret.Value = v;
            return ret;
        }
    }
}
