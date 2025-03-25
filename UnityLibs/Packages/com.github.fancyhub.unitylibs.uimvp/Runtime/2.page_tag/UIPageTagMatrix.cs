/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2024/6/2
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;


namespace FH.UI
{
    /// <summary>
    /// Tag 的矩阵,  
    /// </summary>
    public sealed class UIPageTagMatrix
    {
        public const int C_TAG_MAX = Bit64.LENGTH;
        private MyDict<int, (byte tag, CPtr<IUITagPage> page)> _dict_tag;
        private Dictionary<int, ulong> _dict_hide_mask;
        private Bit64 _hide_mask = Bit64.Zero;

        public UIPageTagMatrix()
        {
            _dict_tag = new MyDict<int, (byte, CPtr<IUITagPage>)>();
            _dict_hide_mask = new Dictionary<int, ulong>();
        }

        public void ClearTags()
        {
            _hide_mask = Bit64.Zero;
            _dict_tag.Clear();
            _dict_hide_mask.Clear();
        }

        public bool AddTag(IUITagPage page, byte page_tag)
        {
            if (page == null)
            {
                UILog._.Assert(false, "handler 不能为空");
                return false;
            }

            if (page_tag < 0 || page_tag >= C_TAG_MAX)
            {
                UILog._.Assert(false, "tag {0} 要在 [0,{1})", page_tag, C_TAG_MAX);                
                return false;
            }

            if (_dict_tag.ContainsKey(page.Id))
            {
                UILog._.Assert(false, "id {0} 对应的tag {1} 重复添加", page.Id, page_tag);
                return false;
            }

            _dict_tag.Add(page.Id, (page_tag, new CPtr<IUITagPage>(page)));
            page.SetPageTagVisible(!_hide_mask[page_tag]);
            return true;
        }

        public bool RemoveTag(int page_id)
        {
            return _dict_tag.Remove(page_id);
        }

        /// <summary>
        /// hide_mask  page.SetPageTagVisible(!hide_mask[page_tag])
        /// </summary>        
        public bool AddMask(int mask_id, Bit64 hide_mask)
        {
            //1. 检查是否存在
            if (_dict_hide_mask.ContainsKey(mask_id))
                return false;

            //2. 添加到dict里面
            _dict_hide_mask.Add(mask_id, hide_mask.Value);

            //3. 计算新的mask
            ulong new_mask = _hide_mask.Value | hide_mask.Value;
            if (new_mask == _hide_mask.Value) // new >= old
                return true;

            //4. 计算增加的位
            ulong diff_mask = new_mask & (~_hide_mask.Value);
            _hide_mask = new_mask;

            //5. 遍历 handler, 只处理增加的位
            Bit64 mask = diff_mask;
            foreach (var p in _dict_tag)
            {
                var page = p.Value.page.Val;
                if (page == null)
                {
                    _dict_tag.Remove(p.Key);
                    continue;
                }

                int tag = p.Value.Item1;
                if (mask[tag])
                {
                    page.SetPageTagVisible(false);
                }
            }
            return true;
        }

        public bool RemoveMask(int mask_id)
        {
            //1. 先移除
            if (!_dict_hide_mask.Remove(mask_id))
                return false;

            //2. 计算新的mask
            ulong new_mask = 0;
            foreach (var p in _dict_hide_mask)
                new_mask |= p.Value;

            //3. 判断是否一样
            if (new_mask == _hide_mask.Value) // new <= old
                return true;

            ulong diff_mask = _hide_mask.Value & (~new_mask);
            _hide_mask = new_mask;

            //5. 遍历 handler, 只处理增加的位
            Bit64 mask = diff_mask;
            foreach (var p in _dict_tag)
            {
                var page = p.Value.page.Val;
                if (page == null)
                {
                    _dict_tag.Remove(p.Key);
                    continue;
                }

                int tag = p.Value.Item1;
                if (mask[tag])
                {
                    page.SetPageTagVisible(true);
                }
            }
            return true;
        }
    }
}
