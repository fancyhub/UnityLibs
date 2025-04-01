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
    public interface IPool
    {
        /// <summary>
        /// 当前还有多少个是 free的
        /// </summary>
        int CountFree { get; }

        /// <summary>
        /// 还在使用的数量，可能不准确，使用 new - del
        /// </summary>
        int CountUsing { get; }

        /// <summary>
        /// 调用了多少次 new
        /// </summary>
        int CountNew { get; }

        /// <summary>
        /// 调用 del的次数
        /// </summary>
        int CountDel { get; }

        /// <summary>
        /// 同时使用的最大数量
        /// </summary>
        int CountUsingMax { get; }

        void Clear();

        bool Del(IPoolItem item);

        Type GetObjType();
    }

    public interface IPoolItem
    {
        IPool Pool { get; set; }
        bool InPool { get; set; }
    }


    public interface IPool<T> : IPool where T : class, IPoolItem
    {
        public T New();
    }
}
