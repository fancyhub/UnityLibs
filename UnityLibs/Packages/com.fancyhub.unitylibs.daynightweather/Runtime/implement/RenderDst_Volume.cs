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
    public class RenderDst_Volume : MonoBehaviour, IRenderDst
    {
        public Volume _target;
        [NonSerialized]
        public VolumeProfile _inst_profile;
        [NonSerialized]
        public VolumeProfile _shared_profile;

        public int RenderDstId { get; set; }

        public void Apply(RenderDataSlotGroup data)
        {
            _apply(data, true, _inst_profile);
        }

        public void OnEnable()
        {
            _target = GetComponent<Volume>();
            Log.Assert(_target != null, "需要Volume");
            if (_target == null)
                return;
            _shared_profile = _target.sharedProfile;
            if (_inst_profile == null)
                _inst_profile = _target.profile;
            else
                _target.profile = _inst_profile;

            RenderDataMgr.Inst.AddRenderDst(this);
            _apply(RenderDataMgr.Inst.CurData, false, _inst_profile);
        }

        public void OnDisable()
        {
            RenderDataMgr.Inst.RemoveRenderDst(this);
            if (_target != null)
                _target.profile = null;
        }

        public static void _apply(RenderDataSlotGroup data, bool dirty_flag, VolumeProfile inst_p)
        {
            if (inst_p == null || data == null)
                return;

            var key = RenderSlotKey.Create(ERenderSlot.post, ERenderSlotPP.bloom);
            if (data.Get(key, dirty_flag, out RenderBloomCfg bloom_cfg))
            {
                inst_p.TryGet(out Bloom inst);
                if (inst != null)
                    bloom_cfg.ApplyTo(inst);
            }
        }
    }
}
