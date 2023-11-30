/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{

    public abstract class CPoolItemBase : IPoolItem, ICPtr, IDisposable
    {
        private IPool ___pool = null;
        private bool ___in_pool = false;
        IPool IPoolItem.Pool { get => ___pool; set => ___pool = value; }
        bool IPoolItem.InPool { get => ___in_pool; set => ___in_pool = value; }

        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        public void Destroy()
        {
            if (___pool == null || ___pool.Del(this))
            {
                ___ptr_ver++;
                OnPoolRelease();
                return;
            }
        }

        public void Dispose()
        {
            Destroy();
        }

        protected abstract void OnPoolRelease();
    }

    public abstract class SPoolItemBase : IPoolItem, ISPtr, IDisposable
    {
        private IPool ___pool = null;
        private bool ___in_pool = false;
        IPool IPoolItem.Pool { get => ___pool; set => ___pool = value; }
        bool IPoolItem.InPool { get => ___in_pool; set => ___in_pool = value; }

        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        private int __ref_count = 0;
        int ISPtr.IncRef()
        {
            __ref_count++;
            return __ref_count;
        }

        public void Destroy()
        {
            __ref_count--;
            if (__ref_count > 0)
                return;

            if (___pool == null || ___pool.Del(this))
            {
                __ref_count = 0;
                ___ptr_ver++;
                OnPoolRelease();
                return;
            }
        }

        public void Dispose()
        {
            Destroy();
        }

        protected abstract void OnPoolRelease();
    }
}
