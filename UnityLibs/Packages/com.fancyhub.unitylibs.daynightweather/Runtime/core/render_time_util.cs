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
    public static class RenderTimeUtil
    {
        public const int C_TIME_OF_DAY_MAX = 60 * 24;//一天这么多 分钟

        /// <summary>
        /// 转换成当天的分钟
        /// </summary>        
        public static int CalcTimeFromHour(float hour)
        {
            int min = (int)(hour * 60);
            return Clamp(min);
        }

        public static int Clamp(int time_of_day_min)
        {
            int ret = time_of_day_min % C_TIME_OF_DAY_MAX;
            if (ret < 0)
                ret += C_TIME_OF_DAY_MAX;
            return ret;
        }

        //计算时间 time_of_day 的在from 和to 之间的范围
        public static float CalcPercent(int from_time, int to_time, int time_of_day)
        {
            for (; ; )
            {
                if (from_time <= time_of_day)
                    break;
                from_time = from_time - C_TIME_OF_DAY_MAX;
            }

            for (; ; )
            {
                if (to_time >= time_of_day)
                    break;
                to_time = to_time + C_TIME_OF_DAY_MAX;
            }

            int duration = to_time - from_time;
            if (duration == 0)
                return 0;

            int dt = time_of_day - from_time;
            return ((dt * 1000) / duration) * 0.001f;
        }
    }
}
