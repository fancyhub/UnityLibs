/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;

namespace FH.ResManagement
{
    //job的优先级队列
    internal class ResJobQueuePriority 
    {
        //按照优先级，从小到大
        public LinkedList<ulong> _job_queue = new LinkedList<ulong>();

        public void Add(int job_id, int prority)
        {
            //1. pack
            ulong id = Pack(job_id, prority);

            //2.找到要插入的节点,从前往后找，一般是 优先级一样，新加的比较小，在前面
            var node = _job_queue.First;
            if (node == null)
            {
                _job_queue.ExtAddFirst(id);
                return;
            }

            for (; ; )
            {
                if (node == null)
                    break;
                if (id < node.Value)
                    break;
                node = node.Next;
            }

            //3.  插入
            if (node == null)
                _job_queue.ExtAddLast(id);
            else
                _job_queue.ExtAddBefore(node, id);
        }

        public void Destroy()
        {
            _job_queue.ExtClear();
        }

        public int GetCount()
        {
            return _job_queue.Count;
        }

        public bool Peek(out int job_id)
        {
            job_id = 0;
            int count = _job_queue.Count;
            if (count == 0)
                return false;

            ulong last = _job_queue.Last.Value;
            Unpack(last, out job_id, out int _);
            return true;
        }

        public bool Pop()
        {
            int count = _job_queue.Count;
            if (count == 0)
                return false;
            _job_queue.ExtRemove(_job_queue.Last);
            return true;
        }

        public static ulong Pack(int job_id, int priority)
        {
            //1. 先把优先级 修正为正数
            long i_p = (long)priority - int.MinValue;

            //2. 把job id 求反
            job_id = int.MaxValue - job_id;

            //3. 拼接
            ulong ui_priority = (ulong)i_p;
            ulong ui_job_id = (uint)job_id;

            return ui_priority << 32 | ui_job_id;
        }

        public static void Unpack(ulong val, out int job_id, out int priority)
        {
            ulong ui_job_id = val & uint.MaxValue;
            job_id = (int)(uint)ui_job_id;

            //再次求反
            job_id = int.MaxValue - job_id;

            ulong ui_priority = val >> 32;
            long priority_64 = (long)ui_priority;
            priority_64 = priority_64 + int.MinValue;
            priority = (int)priority_64;
        }
    }
}
