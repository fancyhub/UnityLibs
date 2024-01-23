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
    public class RenderSrc_MainLightRot : MonoBehaviour
    {
        [System.Serializable]
        public struct InnerData
        {
            [Range(0, 24)]
            public float _hour;
            public Vector3 _rot;
            public IRenderDataSlot<Quaternion> _slot;
        }

        public InnerData[] _rot_list;
        public HashSet<int> _src_ids = new HashSet<int>();
        public void OnEnable()
        {
            UpdateParams();
        }

        public void UpdateParams()
        {
            if (_rot_list == null)
                return;

            var mgr = RenderDataMgr.Inst;
            for (int i = 0; i < _rot_list.Length; i++)
            {
                if (_rot_list[i]._slot == null)
                {
                    _rot_list[i]._slot = new RDSQuaternion();
                }
                _rot_list[i]._slot.Val = Quaternion.Euler(_rot_list[i]._rot);

                int time = RenderTimeUtil.CalcTimeFromHour(_rot_list[i]._hour);

                for (int j = 0; j < (int)EWeather.max; j++)
                {
                    int id = mgr.AddDataSrc((EWeather)j, ERenderSlot.light, ERenderSlotLight.main_light_rot, time, _rot_list[i]._slot);
                    _src_ids.Add(id);
                }
            }
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
