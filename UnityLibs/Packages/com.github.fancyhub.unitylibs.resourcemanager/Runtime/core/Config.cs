/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;

namespace FH
{
    [Serializable]
    public class GameObjectPreInstConfig
    {
        //默认所有的资源，最大的预加载量
        public int DefaultMaxCount = 5;

        [Serializable]
        public struct Item
        {
            public string Path;
            public int MaxCount;
        }
        public List<Item> Items = new List<Item>();


        //某些特殊的文件，最大的预加载量
        private Dictionary<string, int> _Dict;

        public int GetMaxCount(string path)
        {
            //1. 先从 spec_max 里面获取
            if (string.IsNullOrEmpty(path))
                return 0;

            if (_Dict == null)
            {
                _Dict = new Dictionary<string, int>(Items.Count);
                foreach (var item in Items)
                {
                    if (string.IsNullOrEmpty(item.Path))
                        continue;
                    _Dict[item.Path] = item.MaxCount;
                }
            }

            if (_Dict.TryGetValue(path, out var maxCount))
                return maxCount;
            return DefaultMaxCount;
        }
    }


    [Serializable]
    public class ResMgrConfig
    {
        public GameObjectPreInstConfig GameObjectPreInstConfig= new GameObjectPreInstConfig();
    }
}
