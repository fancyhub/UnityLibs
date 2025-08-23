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
    public interface IRenderDst
    {
        int RenderDstId { get; set; }
        void Apply(RenderDataSlotGroup data);
    }

    public struct RenderDataSrcAddHelper
    {
        public EWeather _weather;
        public RenderSlotKey _key;
        public int _time;
        public int _priority;

        public RenderDataSrcAddHelper SetPriority(int priority)
        {
            _priority = priority;
            return this;
        }

        public RenderDataSrcAddHelper SetWeather(EWeather weather)
        {
            _weather = weather;
            return this;
        }

        public RenderDataSrcAddHelper SetMainType(ERenderSlot type)
        {
            _key._type = type;
            return this;
        }

        public RenderDataSrcAddHelper SetSubType<T>(T sub_type) where T : Enum
        {
            _key._sub_type = sub_type.GetHashCode();
            return this;
        }

        public RenderDataSrcAddHelper SetTime(int time)
        {
            _time = RenderTimeUtil.Clamp(time);
            return this;
        }

        public RenderDataSrcAddHelper AddOverride<T>(T sub_type, IRenderDataSlot slot, HashSet<int> out_id_set) where T : Enum
        {
            _key._sub_type = sub_type.GetHashCode();
            int id = RenderDataMgr.Inst._src_data.AddOverride(_priority, _key, slot);
            if (id != 0)
                out_id_set.Add(id);
            return this;
        }

        public RenderDataSrcAddHelper AddScene(IRenderDataSlot slot, out int id)
        {
            id = RenderDataMgr.Inst._src_data.AddTimeWeather(_weather, _key, _time, slot);
            return this;
        }

        public RenderDataSrcAddHelper AddScene(IRenderDataSlot slot, HashSet<int> out_id_set)
        {
            int id = RenderDataMgr.Inst._src_data.AddTimeWeather(_weather, _key, _time, slot);
            if (id != 0)
                out_id_set.Add(id);
            return this;
        }

        public RenderDataSrcAddHelper AddScene<T>(T sub_type, IRenderDataSlot slot, out int id) where T : Enum
        {
            _key._sub_type = sub_type.GetHashCode();
            id = RenderDataMgr.Inst._src_data.AddTimeWeather(_weather, _key, _time, slot);
            return this;
        }

        public RenderDataSrcAddHelper AddScene<T>(T sub_type, IRenderDataSlot slot, HashSet<int> out_id_set) where T : Enum
        {
            _key._sub_type = sub_type.GetHashCode();
            int id = RenderDataMgr.Inst._src_data.AddTimeWeather(_weather, _key, _time, slot);
            if (id != 0)
                out_id_set.Add(id);
            return this;
        }
    }


    public struct RenderDataGetHelper
    {
        public RenderSlotKey _key;
        public bool _dirty_flag;
        public RenderDataGetHelper SetMainType(ERenderSlot main_type)
        {
            _key._type = main_type;
            return this;
        }

        public RenderDataGetHelper SetSubType<T>(T sub_type) where T : Enum
        {
            _key.SetSubType(sub_type);
            return this;
        }

        public RenderDataGetHelper SetDirtyFlag(bool dirty_flag)
        {
            _dirty_flag = dirty_flag;
            return this;
        }

        public RenderDataGetHelper Get<T>(out T v, out bool succ)
        {
            succ = RenderDataMgr.Inst.CurData.Get(_key, _dirty_flag, out v);
            return this;
        }

        public RenderDataGetHelper Get<TEnum, TVal>(TEnum sub_type, out TVal v, out bool succ) where TEnum : Enum
        {
            _key.SetSubType(sub_type);
            succ = RenderDataMgr.Inst.CurData.Get(_key, _dirty_flag, out v);
            return this;
        }
    }


    public sealed class RenderDataMgr
    {
        private static RenderDataMgr _;
        public int _id_render_dst_gen = 1;

        public RenderTimeWeather _time_weather;
        public RenderDataSrc _src_data; //场景的渲染
        public RenderDataSlotGroup _dst_data;
        public Dictionary<int, IRenderDst> _render_dst;

        public static RenderDataMgr Inst { get { if (_ == null) _ = new RenderDataMgr(); return _; } }

        private RenderDataMgr()
        {
            _time_weather = new RenderTimeWeather();
            _render_dst = new Dictionary<int, IRenderDst>();
            _src_data = new RenderDataSrc();
            _dst_data = new RenderDataSlotGroup();
        }

        public RenderDataSlotGroup CurData { get => _dst_data; }

        public int SetOverride<TEnum>(
            int priority,
            ERenderSlot slot_type,
            TEnum slot_sub_type,
            IRenderDataSlot data)
            where TEnum : Enum
        {
            var key = RenderSlotKey.Create(slot_type, slot_sub_type);
            return _src_data.AddOverride(priority, key, data);
        }

        public RenderTimeWeather TimeWeather { get => _time_weather; }

        /// <summary>
        /// 添加场景
        /// </summary>        
        public int AddDataSrc<TEnum>(
                    EWeather weather,
                    ERenderSlot slot_type,
                    TEnum slot_sub_type,
                    int time_of_day,
                    IRenderDataSlot data) where TEnum : Enum
        {
            time_of_day = RenderTimeUtil.Clamp(time_of_day);
            var key = RenderSlotKey.Create(slot_type, slot_sub_type);
            return _src_data.AddTimeWeather(weather, key, time_of_day, data);
        }

        public bool RemoveDataSrc(int src_id)
        {
            return _src_data.Remove(src_id);
        }

        public void AddRenderDst(IRenderDst dst)
        {
            if (dst == null)
                return;
            int id = dst.RenderDstId;
            if (id > 0)
                return;

            id = _id_render_dst_gen++;
            dst.RenderDstId = id;
            _render_dst.Add(id, dst);
        }

        public bool RemoveRenderDst(IRenderDst dst)
        {
            if (dst == null)
                return false;
            int id = dst.RenderDstId;
            if (id == 0)
                return false;

            _render_dst.TryGetValue(id, out var t);
            if (t != dst)
            {
                Log.Assert(false, "不要修改 I_RenderDst.RenderDstId");
                return false;
            }

            _render_dst.Remove(id);
            dst.RenderDstId = 0;
            return true;
        }

        public bool RemoveRenderDst(int dst_id)
        {
            _render_dst.TryGetValue(dst_id, out var t);
            if (t == null)
                return false;

            Log.Assert(t.RenderDstId == dst_id, "不要修改 I_RenderDst.RenderDstId");
            _render_dst.Remove(dst_id);
            t.RenderDstId = 0;
            return true;
        }
    }
}
