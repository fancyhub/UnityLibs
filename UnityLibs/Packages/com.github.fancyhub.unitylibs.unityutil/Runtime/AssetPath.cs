/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2023/11/22
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using UnityEngine;

namespace FH
{
    [Serializable]
    public struct AssetPath<T> : IEquatable<AssetPath<T>> where T : UnityEngine.Object
    {
        public string Path;

        public bool Equals(AssetPath<T> other)
        {
            return Path == other.Path;
        }

        //public static implicit operator string(ResPath<T> a) { return a.Path; }

#if UNITY_EDITOR
        public bool EdSet(T obj)
        {
            if (obj == null)
            {
                Path = string.Empty;
                return true;
            }

            Path = UnityEditor.AssetDatabase.GetAssetPath(obj);
            return string.IsNullOrEmpty(Path);
        }

        public bool EdIsMissing()
        {
            if (string.IsNullOrEmpty(Path))
                return false;
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(Path) == null;
        }

        public T EdLoad()
        {
            if (string.IsNullOrEmpty(Path))
                return null;
            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(Path);
        }
#endif
    }
}