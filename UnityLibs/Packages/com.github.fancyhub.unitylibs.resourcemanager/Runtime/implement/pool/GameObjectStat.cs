/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FH.ResManagement
{
    //统计模块
    internal class GameObjectStat 
    {
        public const int C_CAP = 500;
        //Key：是实例的path， Value ： 实例的数量
        public Dictionary<string, int> _dict;

        public GameObjectStat()
        {
            _dict = new Dictionary<string, int>(C_CAP);
        }

        public int AddOne(string path)
        {
            int count = 0;
            _dict.TryGetValue(path, out count);
            count = count + 1;
            _dict[path] = count;
            return count;
        }

        public int RemoveOne(string path)
        {
            int count = 0;
            bool succ = _dict.TryGetValue(path, out count);
            if (!succ)
                return 0;

            count--;
            if (count == 0)
                _dict.Remove(path);
            else
                _dict[path] = count;
            return count;
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public int GetCount(string path)
        {
            int count = 0;
            _dict.TryGetValue(path, out count);
            return count;
        }
    }

}
