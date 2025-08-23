/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/11/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;

namespace FH.DayNightWeather
{
    public class RenderTimeWeather
    {
        //public const float C_SCALE = RenderTimeUtil.C_TIME_OF_DAY_MAX / 1.0f; //1分钟一天
        public const float C_SCALE = 0;
        public static DateTimeOffset CStartTime = new DateTimeOffset(2021, 1, 1, 12, 0, 0, DateTimeOffset.Now.Offset);

        public IClock _clock;
        public EWeather _weather1 = EWeather.sunny;
        public EWeather _weather2 = EWeather.sunny;
        public float _weather_t = 0.0f;

        public RenderTimeWeather()
        {
            _clock = new ClockDecorator(new ClockLocal(CStartTime.ToUnixMilli()));
            _clock.ScaleFloat = C_SCALE;
        }

        public EWeather Weather1 { get { return _weather1; } }
        public EWeather Weather2 { get { return _weather2; } }
        public float WeatherT { get { return _weather_t; } }

        /// <summary>
        /// 返回从一天的0:00 到现在的总 Minutes
        /// </summary>
        public int GetTimeOfDay()
        {
            long now = GetClock().Time;
            int ret = (int)DateUtil.ToDateTimeLocal(now).TimeOfDay.TotalMinutes;
            return RenderTimeUtil.Clamp(ret);
        }

        public void SetWeather(EWeather weather)
        {
            _weather1 = weather;
            _weather2 = weather;
            _weather_t = 0;
        }

        public void SetWeather(EWeather weather1, EWeather weather2, float t)
        {
            if (t >= 0.99f)
            {
                _weather1 = weather2;
                _weather2 = weather2;
                _weather_t = 0;
            }
            else if (t < 0.01f)
            {
                _weather1 = weather1;
                _weather2 = weather1;
                _weather_t = 0;
            }
            else
            {
                _weather1 = weather1;
                _weather2 = weather2;
                _weather_t = t;
            }
        }

        public IClock OverrideClock { get; set; }

        public IClock GetClock()
        {
            if (OverrideClock != null)
                return OverrideClock;
            return _clock;
        }
    }
}
