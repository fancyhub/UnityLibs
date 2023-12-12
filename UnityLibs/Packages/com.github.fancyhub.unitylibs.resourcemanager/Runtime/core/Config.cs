/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/11 
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH
{


    [Serializable]
    public class ResMgrConfig
    {
        public ELogLvl LogLevel = ELogLvl.Debug;

        /// <summary>
        /// 同时加载资源的数量
        /// </summary>
        [Range(1, 10)]
        public int MaxAsyncResLoaderSlot = 5;

        /// <summary>
        /// 一个GameObject 实例化, 分为2个步骤
        /// 第一步: 实例化到 Disable的节点下面
        /// 第二步: 先移动到 Enable的节点下面, 再移回Disable的节点下面
        /// </summary>
        [Range(1, 10)]
        public int MaxAsyncGameObjectStep = 1;

        public GameObjectPreInstConfig PreInst = new GameObjectPreInstConfig();

        public GCConfig GC = new GCConfig();

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
        public class GCConfig
        {
            /// <summary>
            /// 一帧最多处理多少个Res
            /// </summary>
            [Min(1)]
            public int MaxResCountProcess = 20;

            /// <summary>
            /// 一个资源, 从没有任何人使用->销毁, 等待的帧数            
            /// </summary>
            [Min(1)]
            public int ResWaitFrameCount = 5;

            /// <summary>
            /// 一帧最多处理多少个Res
            /// </summary>
            [Min(1)]
            public int MaxInstCountProcess = 40;

            /// <summary>
            /// 一个GameObject, 从没有任何人使用->销毁, 等待的帧数, 但是受到预实例化数量的限制, 比如该对象需要预实例化10个, 当前使用的和未使用的数量 大于该数量的时候才会触发
            /// </summary>
            [Min(1)]
            public int InstWaitFrameCount = 10;
        }
    }
}
