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
    public interface IObjectStreamIn<T>
    {
        bool Read(out T data);
        int Read(T[] data, int offset, int count);
        int Read(Span<T> data);

        bool IsClosedIn();
        void CloseIn();
    }

    public interface IObjectStreamOut<T>
    {
        bool Write(T data);
        int Write(T[] data, int offset, int count);
        int Write(ReadOnlySpan<T> data);
        bool IsClosedOut();
        void CloseOut();
    }

    public interface IObjectStream<T> : IObjectStreamIn<T>, IObjectStreamOut<T>
    {
    }
}
