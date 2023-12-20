/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/13
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;

namespace FH
{
    public interface IClock
    {
        //默认，不缩放，千分位
        public const uint ScaleOne = 1000;

        void Scale(float scale);

        /// <summary>
        /// 千分位
        /// </summary>        
        void Scale(uint scale);

        /// <summary>
        /// 基于千分位, 如果返回1000 ,就是不缩放
        /// 对应 C_SCALE_ONE
        /// </summary>        
        uint GetScale();

        void Pause();
        void Resume();
        bool IsPaused();
        long GetTime();
    }     
}
