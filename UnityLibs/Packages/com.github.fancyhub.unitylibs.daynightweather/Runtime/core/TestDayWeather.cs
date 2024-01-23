/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/11/18
 * Title   : 
 * Desc    : 
*************************************************************************************/

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.DayNightWeather
{
    public class TestDayWeather : MonoBehaviour
    {
        [System.Serializable]
        public struct InnerTimeWeather
        {
            [Range(0, 24)]
            public float _hour_of_day;
            public EWeather _weather_1;
            public EWeather _weather_2;
            public float _weather_t;
            public ClockManual _manual_clock;
        }

        public bool _manual;
        public InnerTimeWeather _time_weather;

        [System.Serializable]
        public struct InnerSlotDataMainLight
        {
            public Quaternion _rot;
            public MainLightColor _color;
        }

        [System.Serializable]
        public struct InnerData
        {
            public InnerSlotDataMainLight _main_light;
            public bool _point_light_active;
            public RenderEnvCfg _env;
            public RenderFogCfg _fog;
            public RenderBloomCfg _bloom;
        }
        public InnerData _snapshot;

        public void Update()
        {
            _update_day_weather(_manual, ref _time_weather);

            new RenderDataGetHelper()
                .SetDirtyFlag(false)
                .SetMainType(ERenderSlot.light)
                    .Get(ERenderSlotLight.main_light_rot, out _snapshot._main_light._rot, out var _)
                    .Get(ERenderSlotLight.main_light_color, out _snapshot._main_light._color, out var _)
                    .Get(ERenderSlotLight.point_light_active, out _snapshot._point_light_active, out var _)
                .SetMainType(ERenderSlot.env)
                    .Get(ERenderSlotEnv.main, out _snapshot._env, out var _)
                    .Get(ERenderSlotEnv.fog, out _snapshot._fog, out var _)
                .SetMainType(ERenderSlot.post)
                    .Get(ERenderSlotPP.bloom, out _snapshot._bloom, out var _);

            if (_editor_calc == null)
                _editor_calc = new RenderDataCalc();
            _editor_calc.Update();

        }
        private static RenderDataCalc _editor_calc;

        public static void _update_day_weather(bool manual, ref InnerTimeWeather time_weather)
        {
            RenderTimeWeather global_time_weather = RenderDataMgr.Inst.TimeWeather;

            if (!manual)
            {
                time_weather._weather_1 = global_time_weather.Weather1;
                time_weather._weather_2 = global_time_weather.Weather2;
                time_weather._weather_t = global_time_weather.WeatherT;
                time_weather._hour_of_day = global_time_weather.GetTimeOfDay() / 60.0f;

                if (time_weather._manual_clock != null)
                {
                    if (global_time_weather.OverrideClock == time_weather._manual_clock)
                    {
                        global_time_weather.OverrideClock = null;
                    }
                    time_weather._manual_clock = null;
                }
                return;
            }

            if (time_weather._manual_clock == null)
            {
                time_weather._manual_clock = new ClockManual(0);
                global_time_weather.OverrideClock = time_weather._manual_clock;
            }

            time_weather._manual_clock.Time = _hour_2_time(time_weather._hour_of_day);
            if (global_time_weather.OverrideClock == time_weather._manual_clock)
            {
                global_time_weather.SetWeather(time_weather._weather_1, time_weather._weather_2, time_weather._weather_t);
            }
        }

        public static long _hour_2_time(float hour)
        {
            DateTime svr_time_now = DateUtil.NowSvr;
            int min = RenderTimeUtil.CalcTimeFromHour(hour);
            TimeSpan dt = svr_time_now.TimeOfDay;
            DateTime new_time = svr_time_now - dt + new TimeSpan(0, min, 0);
            return new_time.ToUnixMilli();
        }
    }
}

#endif