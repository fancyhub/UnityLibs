/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/10 15:53:11
 * Title   : 
 * Desc    :  这个是多线程的操作
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    public interface IObjectChannel<T> : IDestroyable
    {
        /// <summary>
        /// ObjChannel 不会阻塞
        /// BoundedObjChannel 会阻塞
        /// </summary>
        bool Write(T v);

        /// <summary>
        /// 阻塞
        /// </summary>
        bool Read(out T v);

        /// <summary>
        /// 非阻塞的获取
        /// </summary>
        bool Select(out T v);

        /// <summary>
        /// 非阻塞模式的获取指定数量的对象 <para/>
        /// 如果 被销毁了, 返回0 ,没有获取到也返回0 <para/>
        /// count &lt;= 0, 就是有多少取多少
        /// </summary>
        int Select(ICollection<T> list, int count = 0);


        /// <summary>
        /// 非阻塞模式的获取指定数量的对象 <para/>
        /// 如果 被销毁了, 返回0 ,没有获取到也返回0 <para/>
        /// count &lt;= 0, 就是有多少取多少
        /// </summary>
        int Select(Queue<T> list, int count = 0);

        void Close();

        bool IsClosed();
    }     
}
