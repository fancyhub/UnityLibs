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
    public class RenderSrc_VolumeProfileOverride : MonoBehaviour
    {
        public int _priority = 1;
   
        public Volume _volume;
        public IRenderDataSlot<VolumeProfile> _volume_slot;
        public HashSet<int> _src_ids = new HashSet<int>();

        public void OnEnable()
        {
            UpdateParams();
        }

        public void UpdateParams()
        {
            if (_volume == null)
                return;

            if (_volume_slot == null)
                _volume_slot = new RDSObj<VolumeProfile>();
            _volume_slot.Val = _volume.sharedProfile;

            new RenderDataSrcAddHelper()
                .SetPriority(_priority)
                .SetMainType(ERenderSlot.post)
                    .AddOverride(ERenderSlotPP.profiler, _volume_slot, _src_ids);

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
