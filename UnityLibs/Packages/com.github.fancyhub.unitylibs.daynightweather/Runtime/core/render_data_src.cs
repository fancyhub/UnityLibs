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
    /// <summary>
    /// 数据源
    /// </summary>
    public class RenderDataSrc
    {
        public int _id_gen = 1;
        public struct InnerData
        {
            public RenderSlotKey _key;
            public LinkedListNode<DataSlotNodeVal> _node;
        }

        public RenderData_Weather[] _groups;
        public RenderData_Priority _override_groups;

        public Dictionary<int, InnerData> _dict;

        public RenderDataSrc()
        {
            _groups = new RenderData_Weather[(int)EWeather.max];
            for (int i = 0; i < _groups.Length; i++)
                _groups[i] = new RenderData_Weather();
            _override_groups = new RenderData_Priority();
            _dict = new Dictionary<int, InnerData>(1000);
        }

        public int AddOverride(int priority, RenderSlotKey key, IRenderDataSlot data)
        {
            if (data == null)
                return 0;

            int id = _id_gen++;
            DataSlotNodeVal node_val = new DataSlotNodeVal()
            {
                _id = id,
                _val = priority,
                _data = data,
            };

            LinkedListNode<DataSlotNodeVal> node = _override_groups.Add(key, node_val);

            _dict.Add(id, new InnerData()
            {
                _node = node,
                _key = key,
            });
            return id;
        }

        public int AddTimeWeather(
            EWeather weather,
            RenderSlotKey key,
            int time_of_day,
            IRenderDataSlot data)
        {
            if (data == null)
                return 0;

            RenderData_Weather weather_group = GetGroup(weather);
            if (weather_group == null)
                return 0;

            int id = _id_gen++;
            DataSlotNodeVal node_val = new DataSlotNodeVal()
            {
                _id = id,
                _val = time_of_day,
                _data = data,
            };

            LinkedListNode<DataSlotNodeVal> node = weather_group.Add(key, node_val);

            _dict.Add(id, new InnerData()
            {
                _node = node,
                _key = key,
            });
            return id;
        }

        public bool Remove(int src_id)
        {
            if (!_dict.TryGetValue(src_id, out InnerData data))
                return false;
            _dict.Remove(src_id);

            LinkedListNode<DataSlotNodeVal> node = data._node;
            if (node == null)
                return true;

            LinkedList<DataSlotNodeVal> list = data._node.List;
            if (list != null)
                list.ExtRemove(data._node);
            return true;
        }

        public RenderDataSlotGroup CalcOverride()
        {
            return _override_groups.Calc();
        }

        public RenderData_Weather GetGroup(EWeather weather)
        {
            if (weather < 0 || (int)weather >= _groups.Length)
                return null;
            return _groups[(int)weather];
        }
    }
}
