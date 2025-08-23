/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/11/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FH.DayNightWeather
{
    public class RenderDst_VolumeProfile : MonoBehaviour, IRenderDst
    {
        public Volume _target;

        public int RenderDstId { get; set; }

        public void Apply(RenderDataSlotGroup data)
        {
            _apply(_target, data, true);
        }

        public void OnEnable()
        {
            _target = GetComponent<Volume>();
            Log.Assert(_target != null, "需要Volume");
            if (_target == null)
                return;


            RenderDataMgr.Inst.AddRenderDst(this);
            _apply(_target, RenderDataMgr.Inst.CurData, false);
        }

        public void OnDisable()
        {
            RenderDataMgr.Inst.RemoveRenderDst(this);
            //if (_target != null)
            //    _target.profile = null;
        }

        public static void _apply(Volume tar, RenderDataSlotGroup data, bool dirty_flag)
        {
            if (data == null || tar == null)
                return;

            var key = RenderSlotKey.Create(ERenderSlot.post, ERenderSlotPP.profiler);
            if (data.Get(key, dirty_flag, out VolumeProfile cfg))
            {
                tar.sharedProfile = cfg;
            }
        }
    }
}
