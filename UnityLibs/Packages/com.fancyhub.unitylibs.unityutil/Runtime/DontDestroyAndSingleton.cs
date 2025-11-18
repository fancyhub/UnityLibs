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
            }
            else
            {
                _Dict.TryGetValue(KeyName, out var oldV);
                if (oldV != null && oldV != this)
                {
                    Destroy(gameObject);
                }
                else
                {
                    _Dict[KeyName] = this;
                    GameObject.DontDestroyOnLoad(gameObject);
                }
            }
        }
    }
}
