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
    /// 渲染数据的混合计算
    /// </summary>
    public sealed class RenderDataCalc
    {
        public RenderDataMgr _data_mgr;

        public RenderDataSrc _src_data;
        public RenderDataSlotGroup _scene_dst_group;
        public RenderDataSlotGroup _dst_data;
        public RenderTimeWeather _time_weather;

        public RenderDataCalc()
        {
            _data_mgr = RenderDataMgr.Inst;
            _scene_dst_group = new RenderDataSlotGroup();

            _time_weather = _data_mgr.TimeWeather;
            _src_data = _data_mgr._src_data;
            _dst_data = _data_mgr.CurData;
        }

        public void Update()
        {
            //1. 更新Time Weather 的数据
            int time = _time_weather.GetTimeOfDay();
            RenderData_Weather weather_group1 = _src_data.GetGroup(_time_weather.Weather1);
            RenderData_Weather weather_group2 = _src_data.GetGroup(_time_weather.Weather2);
            RenderDataSlotGroup group1 = weather_group1.Calc(time);
            if (weather_group1 != weather_group2)
            {
                RenderDataSlotGroup group2 = weather_group2.Calc(time);
                float weather_t = _time_weather.WeatherT;
                _scene_dst_group.Lerp(group1, group2, weather_t);
            }
            else
            {
                _scene_dst_group.Copy(group1);
            }

            //2. 更新Override 数据
            RenderDataSlotGroup override_group = _src_data.CalcOverride();

            //3. 合并, Override 的优先级比较高, 以 override 为主
            _dst_data.Lerp(_scene_dst_group, override_group, 1.0f);

            //2. 根据slots的脏位, 输出到目标
            if (!_data_mgr._dst_data.IsDirty())
                return;
            foreach (var p in _data_mgr._render_dst)
            {
                IRenderDst dst = p.Value;
                if (dst == null)
                    continue;
                dst.Apply(_data_mgr._dst_data);
            }
            _data_mgr._dst_data.ClearDirty();
        }
    }
}
