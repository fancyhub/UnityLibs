/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2025/3/26
 * Title   : 
 * Desc    : 
*************************************************************************************/

using UnityEngine;
using System.Collections.Generic;

namespace FH
{
    public sealed class DontDestroyAndSingleton : UnityEngine.MonoBehaviour
    {
        private static Dictionary<string, DontDestroyAndSingleton> _Dict = new();
        public string KeyName;
        public void Awake()
        {
            if (string.IsNullOrEmpty(KeyName))
            {
                GameObject.DontDestroyOnLoad(gameObject);
                return;
            }

            _Dict.TryGetValue(KeyName, out var oldV);
            if (oldV != null && oldV != this) // 重复了, 销毁当前对象
            {
                Destroy(gameObject);                
            }
            else
            {
                //把自己添加进去
                _Dict[KeyName] = this;
                GameObject.DontDestroyOnLoad(gameObject);
            }
        }

        public void OnDestroy()
        {
            if (string.IsNullOrEmpty(KeyName))
                return;
            _Dict.TryGetValue(KeyName, out var oldV);
            if (oldV == this) // 自己销毁自己
                _Dict.Remove(KeyName);
        }
    }
}
