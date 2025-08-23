/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/11/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.DayNightWeather
{
    public class RenderDst_RenderSetting : MonoBehaviour, IRenderDst
    {
        public int RenderDstId { get; set; }

        public void OnEnable()
        {
            RenderDataMgr.Inst.AddRenderDst(this);            
            _apply(RenderDataMgr.Inst.CurData,false);
        }

        public void OnDisable()
        {
            RenderDataMgr.Inst.RemoveRenderDst(this);
        }

        public void Apply(RenderDataSlotGroup data)
        {
            if (data == null)
                return;
            _apply(data, true);
        }

        public static void _apply(RenderDataSlotGroup data, bool dirty_flag)
        {
            if (data == null)
                return;

            var key = RenderSlotKey.Create(ERenderSlot.env, ERenderSlotEnv.main);
            if (data.Get(key, dirty_flag, out RenderEnvCfg cfg))
            {
                cfg.WriteToEnv();                
            }

            key.SetSubType(ERenderSlotEnv.fog);
            if (data.Get(key, dirty_flag, out RenderFogCfg c))
            {
                c.WriteToEnv();
            }
        }
    }
}
