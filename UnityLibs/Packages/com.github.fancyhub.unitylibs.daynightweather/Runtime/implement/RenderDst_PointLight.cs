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
    /// <summary>
    /// 点光源的开关
    /// </summary>
    public class RenderDst_PointLight : MonoBehaviour, IRenderDst
    {
        public Light _light;
        public int RenderDstId { get; set; }

        public void OnEnable()
        {
            _light = GetComponent<Light>();
            Log.Assert(_light != null, "需要Light");
            if (_light == null)
                return;

            RenderDataMgr.Inst.AddRenderDst(this);
            _apply(RenderDataMgr.Inst.CurData, false, _light);
        }

        public void OnDisable()
        {
            RenderDataMgr.Inst.RemoveRenderDst(this);
            if (_light != null)
                _light.enabled = false;
        }

        public void Apply(RenderDataSlotGroup data)
        {
            _apply(data, true, _light);
        }

        public static void _apply(RenderDataSlotGroup data, bool dirty_flag, Light light)
        {
            if (light == null || data == null)
                return;

            var key = RenderSlotKey.Create(ERenderSlot.light, ERenderSlotLight.point_light_active);
            if (data.Get(key, dirty_flag, out bool enable))
                light.enabled = enable;
        }
    }
}
