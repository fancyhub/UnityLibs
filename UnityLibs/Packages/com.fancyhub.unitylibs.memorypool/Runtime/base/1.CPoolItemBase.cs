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

        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        public void Destroy()
        {
            if (___pool == null || ___pool.Del(this))
            {
                ___obj_ver++;
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
