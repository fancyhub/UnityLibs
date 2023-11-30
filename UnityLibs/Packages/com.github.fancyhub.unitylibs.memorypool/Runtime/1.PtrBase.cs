/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    public abstract class CPtrBase : ICPtr, IDisposable
    {
        private int ___ptr_ver = 0;
        int ICPtr.PtrVer => ___ptr_ver;

        public void Destroy()
        {
            ___ptr_ver++;
            OnRelease();
        }

        public void Dispose()
        {
            Destroy();
        }

        protected abstract void OnRelease();
    }
}
