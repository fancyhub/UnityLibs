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
    //主光源的旋转&颜色
    public class RenderDst_MainLight : MonoBehaviour, IRenderDst
    {
        public Light _light;
        public int RenderDstId { get; set; }

        public void OnEnable()
        {
            _light = GetComponent<Light>();
            Log.Assert(_light != null, "需要Light");
            if (_light == null)
                return;

            Log.Assert(_light.enabled, "主光源需要Enable");
            Log.Assert(_light.type == LightType.Directional, "必须是主光源");

            RenderDataMgr.Inst.AddRenderDst(this);
            _apply(RenderDataMgr.Inst.CurData, false, _light);
        }

        public void OnDisable()
        {
            RenderDataMgr.Inst.RemoveRenderDst(this);
        }

        public void Apply(RenderDataSlotGroup data)
        {
            _apply(data, true, _light);
        }

        public static void _apply(RenderDataSlotGroup data, bool dirty_flag, Light light)
        {
            if (light == null || data == null)
                return;

            var key = RenderSlotKey.Create(ERenderSlot.light, ERenderSlotLight.main_light_rot);
            if (data.Get(key, dirty_flag, out Quaternion rot))
                light.transform.localRotation = rot;

            key = RenderSlotKey.Create(ERenderSlot.light, ERenderSlotLight.main_light_color);
            if (data.Get(key, dirty_flag, out MainLightColor c))
            {
                light.color = c._color;
                light.intensity = c._intensity;
                light.bounceIntensity = c._indirect_multipiler;
            }
        }
    }
}
