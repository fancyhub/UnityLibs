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
    public class RenderSrc_HeroShowEnv : MonoBehaviour
    {
        public int _priority = 1;

        [System.Serializable]
        public struct InnerData
        {
            public RenderEnvCfg _env;
            public IRenderDataSlot<RenderEnvCfg> _env_slot;

            public RenderFogCfg _fog;
            public IRenderDataSlot<RenderFogCfg> _fog_slot;


            //[Sirenix.OdinInspector.Button]
            public void ReadFromEnv()
            {
                _env.ReadFromEnv();
                _fog.ReadFromEnv();
            }
        }

        public InnerData _data;
        public HashSet<int> _src_ids = new HashSet<int>();

        public void OnEnable()
        {
            UpdateParams();
        }

        public void UpdateParams()
        {
            if (_data._env_slot == null)
                _data._env_slot = new RDSStruct<RenderEnvCfg>();
            _data._env_slot.Val = _data._env;

            if (_data._fog_slot == null)
                _data._fog_slot = new RDSStruct<RenderFogCfg>();
            _data._fog_slot.Val = _data._fog;

            new RenderDataSrcAddHelper()
                .SetPriority(_priority)
                .SetMainType(ERenderSlot.env)
                    .AddOverride(ERenderSlotEnv.main, _data._env_slot, _src_ids)
                    .AddOverride(ERenderSlotEnv.fog, _data._fog_slot, _src_ids);
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
