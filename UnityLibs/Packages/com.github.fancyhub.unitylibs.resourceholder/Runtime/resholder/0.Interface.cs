using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{
 
    public struct ResHolderStat
    {
        public int TotalCount;
        public int LoadingCount;
        public int FailedCount;
        public int SuccCount;
    }

    //只能加载, 不能卸载, 只能一起卸载
    public interface IResHolder : IDestroyable
    {
        public void Preload<T>(string path);

        public T Load<T>(string path) where T : UnityEngine.Object;

        public ResHolderStat GetResHolderStat();
    }
}