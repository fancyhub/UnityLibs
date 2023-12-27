/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/10 15:53:11
 * Title   : 
 * Desc    :  这个是多线程的操作
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;

namespace FH
{
    public static class ObjectChannelExt
    {
        public static ITask ConnectInStream<T>(this IObjectChannel<T> self, IObjectInStream<T> in_stream, Action task_end_cb = null)
        {
            if (self == null || in_stream == null)
                return null;

            Action act = () =>
            {
                for (; ; )
                {
                    bool read_succ = in_stream.Read(out T v);
                    if (!read_succ)
                    {
                        if (in_stream.IsClosed())
                            return;
                        Log.Assert(false, "从 {0} 读取失败 ", in_stream.GetType());
                        continue;
                    }

                    bool write_succ = self.Write(v);
                    if (!write_succ)
                    {
                        if (self.IsClosed())
                            return;
                        Log.Assert(false, "写入 {0} 失败 ", self.GetType());
                    }
                }
            };
            return TaskQueue.AddTask(act, task_end_cb);
        }

        public static ITask ConnectOutStream<T>(
           this IObjectChannel<T> self,
           IObjectOutStream<T> out_stream,
           Action task_end_cb = null)
        {
            if (self == null || out_stream == null)
                return null;

            Action act = () =>
            {
                for (; ; )
                {
                    bool read_succ = self.Read(out T v);
                    if (!read_succ)
                    {
                        if (self.IsClosed())
                            return;
                        Log.Assert(false, "从 {0} 读取失败 ", self.GetType());
                        continue;
                    }

                    bool write_succ = out_stream.Write(v);
                    if (!write_succ)
                    {
                        if (out_stream.IsClosed())
                            return;
                        Log.Assert(false, "写入 {0} 失败 ", out_stream.GetType());
                    }
                }
            };
            return TaskQueue.AddTask(act, task_end_cb);
        }
    }
}
