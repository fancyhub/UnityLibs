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
    public class RenderSrc_VolumeProfile : MonoBehaviour
    {
        public EWeather _weather;
        [System.Serializable]
        public struct InnerData
        {
            public string _name;

            [Range(0, 24)]
            public float _hour;

            public Volume _volume;
            public IRenderDataSlot<VolumeProfile> _volume_slot;             
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

            if (data._volume_slot == null)
                data._volume_slot = new RDSObj<VolumeProfile>();
            data._volume_slot.Val = data._volume.sharedProfile;             

            new RenderDataSrcAddHelper()
                .SetWeather(weather).SetTime(time)
                .SetMainType(ERenderSlot.post)
                    .AddScene(ERenderSlotPP.profiler, data._volume_slot, out_list);

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
