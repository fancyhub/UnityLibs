/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/7/22 
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;

namespace FH
{
    public sealed class PtrList : CPoolItemBase
    {
        /// <summary>
        /// 把 IDestroy 的接口变成 ICPtr 接口,包一层
        /// </summary>
        public sealed class CPtrWrapper : CPoolItemBase
        {
            private IDestroyable _tar;
            public static CPtrWrapper Create(IDestroyable tar)
            {
                if (tar == null)
                {
                    Log.Assert(false, "Tar is null");
                    return null;
                }
                if (tar is ICPtr)
                {
                    Log.Assert(false, "Tar {0} 实现了 ICPtr 接口", tar.GetType());
                    return null;
                }
                var ret = GPool.New<CPtrWrapper>();
                ret._tar = tar;
                return ret;
            }

            protected override void OnPoolRelease()
            {
                var t = _tar;
                _tar = null;
                t?.Destroy();
            }
        }

        private const int C_COUNT_THRESHOLD = 10; //每添加多少个之后,检查一下是否有invalid,并删除        

        private int _count_threshold = C_COUNT_THRESHOLD;
        private LinkedList<CPtr<ICPtr>> _list = new LinkedList<CPtr<ICPtr>>();
        private int _last_count = 0;

        public static PtrList operator +(PtrList ptr_list, IDestroyable ptr)
        {
            //1. 检查是否为空
            if (ptr == null)
                return ptr_list;

            //2. 如果ptr list 为空,创建一个
            if (ptr_list == null)
                ptr_list = GPool.New<PtrList>();

            //3. 获取 I_Cptr 的接口
            ICPtr c_ptr = ptr as ICPtr;
            if (c_ptr == null)
            {
                c_ptr = CPtrWrapper.Create(ptr);
            }

            //4.  把Ptr 添加到 List里面
            ptr_list._list.ExtAddLast(new CPtr<ICPtr>(c_ptr));

            //5. 检查是否触发了删除非法的ptr
            if (ptr_list._list.Count >= (ptr_list._last_count + ptr_list._count_threshold))
                ptr_list.RemoveInvalid();

            return ptr_list;
        }

        public int RemoveInvalid()
        {
            for (var node = _list.First; node != null;)
            {
                var t = node;
                node = node.Next;
                if (t.Value.Null)
                {
                    _list.ExtRemove(t);
                }
            }
            _last_count = _list.Count;
            return _list.Count;
        }

        public int Count { get { return _list.Count; } }

        public int CountThreshold { get => _count_threshold; set => _count_threshold = value; }

        public void Clear()
        {
            //从后向前销毁
            for (var node = _list.Last; node != null; node = node.Previous)
            {
                node.Value.Destroy();
            }
            _list.ExtClear();
            _last_count = 0;
        }

        protected override void OnPoolRelease()
        {
            Clear();
            _count_threshold = C_COUNT_THRESHOLD;
        }
    }
}
