using System;
using System.Collections.Generic;

namespace FH
{
    public class ResHolder : IResHolder
    {
        //public HolderResMgr _mgr;
        public static uint _id_gen = 0;
        public uint Id;

        public ResHolderStat _stats;
        //public Dictionary<ResKey, ResVal> _Dict = new Dictionary<ResKey, ResVal>();

        public ResHolder()
        {
            _id_gen++;
            Id = _id_gen;
        }

        public void Destroy()
        {
        }

        public ResHolderStat GetResHolderStat()
        {
            return _stats;
        }

        public T Load<T>(string path) where T : UnityEngine.Object
        {
            throw new NotImplementedException();
        }

        public void Preload<T>(string path)
        {
            throw new NotImplementedException();
        }
    }
}
