using System;
using System.Collections.Generic;

namespace FH
{
 

    public abstract class HolderFactory
    {
        protected IResLoader _ResLoader;
        protected static HolderFactory _;
        public static HolderFactory Inst { get { return _; } }

        public void SetResourceLoader(IResLoader loader)
        {
            _ResLoader = loader;
        } 
    }
}
