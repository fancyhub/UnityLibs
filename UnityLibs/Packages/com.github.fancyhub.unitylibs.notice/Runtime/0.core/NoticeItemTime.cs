/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/23 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using UnityEngine;

namespace FH
{
    public enum ENoticeItemPhase
    {
        Wait,
        ShowIn,   //出现动画
        Showing,  //展示中  
        HideOut,  //Hide 动画
        End
    }

    public struct NoticeItemTime
    {
        private long _start_time; // 开始的时间戳
        private long _show_time;  // 完全显示的时间戳  start 到 show 为 fade_in
        private long _hide_time;  // 开始隐藏的时间戳,  show 到 hide 为 普通显示状态 
        private long _end_time;   // 结束的时间戳, hide 到 end 为 fade_out

        private ENoticeItemPhase _phase;
        private long _phase_elapsed;//毫秒
        private long _phase_total; //毫秒

        public NoticeItemTime(long show_duration_ms, int show_up_duration_ms, int hide_out_duration_ms)
        {
            _start_time = 0;
            _show_time = _start_time + Math.Max(0, show_up_duration_ms);            
            _hide_time = _show_time + show_duration_ms;
            _end_time = _hide_time + Math.Max(0, hide_out_duration_ms);            
            _phase = ENoticeItemPhase.Wait;
            _phase_elapsed = 0;
            _phase_total = 0;
        }

        public long GetFadeInDuration()
        {
            return _show_time - _start_time;
        }

        public void Delay(long delay)
        {
            _start_time += delay;
            _show_time += delay;
            _hide_time += delay;
            _end_time += delay;
        }

        public bool IsEnd(long time_now)
        {
            return time_now >= _end_time;
        }

        public ENoticeItemPhase Phase { get { return _phase; } }

        public void SetTimeNow(long time_now)
        {
            if (time_now >= _end_time)
            {
                _phase = ENoticeItemPhase.End;
            }
            else if (time_now >= _hide_time)
            {
                _phase = ENoticeItemPhase.HideOut;
                _phase_total = _end_time - _hide_time;
                _phase_elapsed = time_now - _hide_time;
            }
            else if (time_now >= _show_time)
            {
                _phase = ENoticeItemPhase.Showing;
                _phase_total = _hide_time - _show_time;
                _phase_elapsed = time_now - _show_time;
            }
            else if (time_now >= _start_time)
            {
                _phase = ENoticeItemPhase.ShowIn;
                _phase_total = _show_time - _start_time;
                _phase_elapsed = time_now - _start_time;
            }
            else
            {
                _phase = ENoticeItemPhase.Wait;
            }
        }

        // 获取当前 阶段的百分比
        public float GetCurPhaseProgress()
        {
            switch (_phase)
            {
                case ENoticeItemPhase.Wait:
                    return 0.0f;
                case ENoticeItemPhase.ShowIn:
                case ENoticeItemPhase.Showing:
                case ENoticeItemPhase.HideOut:
                    return Range64.GetClampPercent(0, _phase_total, _phase_elapsed);
                case ENoticeItemPhase.End:
                    return 1.0f;
                default:
                    NoticeLog.Assert(false, "未知的类型 {0}", _phase);
                    return 0.0f;
            }
        }
    }

}
