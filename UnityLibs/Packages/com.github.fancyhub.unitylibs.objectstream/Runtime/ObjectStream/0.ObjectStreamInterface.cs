/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/10 15:24:44
 * Title   : 
 * Desc    :  一个对象流的接口
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    public interface IObjectStream
    {
        bool IsClosed();
        void Close();
    }

    public interface IObjectInStream<T> : IObjectStream
    {
        bool Read(out T out_v);
    }

    public interface IObjectArraryInStream<T> : IObjectStream
    {
        int Read(T[] buff, int offset, int count);
    }

    public interface IObjectOutStream<T> : IObjectStream
    {
        bool Write(T target);
    }

    public interface IObjectArrayOutStream<T> : IObjectStream
    {
        int Write(T[] buff,int offset,int count);
    }

    public static class ArrayExt
    {
        
    }
}
