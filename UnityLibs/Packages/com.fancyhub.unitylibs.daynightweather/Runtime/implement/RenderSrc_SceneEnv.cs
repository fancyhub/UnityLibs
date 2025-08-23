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

namespace FH.DayNightWeather
{
    public class RenderSrc_SceneEnv : MonoBehaviour
    {
        public EWeather _weather;

        [System.Serializable]
        public struct InnerData
        {
            public string _name;

            [Range(0, 24)]
            public float _hour;

            public MainLightColor _main_light;
            public IRenderDataSlot<MainLightColor> _main_light_slot;

            public bool _point_light;
            public IRenderDataSlot<bool> _point_light_slot;

            public RenderEnvCfg _env;
            public IRenderDataSlot<RenderEnvCfg> _env_slot;

            public RenderFogCfg _fog;
            public IRenderDataSlot<RenderFogCfg> _fog_slot;

            //public RenderBloomCfg _bloom;
            //public I_RenderDataSlot<RenderBloomCfg> _bloom_slot;


            //[Sirenix.OdinInspector.Button]
            public void ReadFromEnv()
            {
                _main_light.ReadFromRealLight();
                _env.ReadFromEnv();
                _fog.ReadFromEnv();
                //_bloom.ReadFromEnv();
            }
        }

        public InnerData[] _data;
        public HashSet<int> _src_ids = new HashSet<int>();

        public void OnEnable()
        {
            UpdateParams();
        }

        public void UpdateParams()
        {
            if (_data == null)
                return;

            for (int i = 0; i < _data.Length; i++)
            {
                _data[i] = _update_single_param(_data[i], _weather, _src_ids);
            }
        }

        public static InnerData _update_single_param(
            InnerData data,
            EWeather weather,
            HashSet<int> out_list)
        {
            int time = RenderTimeUtil.CalcTimeFromHour(data._hour);

            if (data._main_light_slot == null)
                data._main_light_slot = new RDSStruct<MainLightColor>();
            data._main_light_slot.Val = data._main_light;

            if (data._env_slot == null)
                data._env_slot = new RDSStruct<RenderEnvCfg>();
            data._env_slot.Val = data._env;

            if (data._fog_slot == null)
                data._fog_slot = new RDSStruct<RenderFogCfg>();
            data._fog_slot.Val = data._fog;

            //if (data._bloom_slot == null)
            //    data._bloom_slot = new RDSStruct<RenderBloomCfg>();
            //data._bloom_slot.Val = data._bloom;

            if (data._point_light_slot == null)
                data._point_light_slot = new RDSBoolean();
            data._point_light_slot.Val = data._point_light;

            new RenderDataSrcAddHelper()
                .SetWeather(weather).SetTime(time)
                .SetMainType(ERenderSlot.light)
                    .AddScene(ERenderSlotLight.main_light_color, data._main_light_slot, out_list)
                    .AddScene(ERenderSlotLight.point_light_active, data._point_light_slot, out_list)
                .SetMainType(ERenderSlot.env)
                    .AddScene(ERenderSlotEnv.main, data._env_slot, out_list)
                    .AddScene(ERenderSlotEnv.fog, data._fog_slot, out_list);
                //.SetMainType(E_RENDER_SLOT.post)
                    //.AddScene(E_RENDER_SLOT_PP.bloom, data._bloom_slot, out_list);

            return data;
        }

#if UNITY_EDITOR
        public void Update()
        {
            UnregAll();
            UpdateParams();
        }
#endif

        public void UnregAll()
        {
            foreach (var id in _src_ids)
            {
                RenderDataMgr.Inst.RemoveDataSrc(id);
            }
            _src_ids.Clear();
        }

        public void OnDisable()
        {
            UnregAll();
        }
    }

}
