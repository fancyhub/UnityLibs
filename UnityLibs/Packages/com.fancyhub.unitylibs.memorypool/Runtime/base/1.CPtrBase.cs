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
        private int ___obj_ver = 0;
        int IVersionObj.ObjVersion => ___obj_ver;

        public void Destroy()
        {
            ___obj_ver++;
            OnRelease();
        }

        public void Dispose()
        {
            Destroy();
        }

        protected abstract void OnRelease();
    }
}
