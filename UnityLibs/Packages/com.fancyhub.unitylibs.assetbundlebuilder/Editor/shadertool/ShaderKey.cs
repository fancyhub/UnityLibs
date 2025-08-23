/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;


namespace FH.AssetBundleBuilder.Ed
{
    public sealed class ShaderKey : IEqualityComparer<ShaderKey>, IEquatable<ShaderKey>
    {
        public static ShaderKey Empty = new ShaderKey(null, System.Array.Empty<string>(), MaterialGlobalIlluminationFlags.None);
        public readonly Shader Shader;
        public readonly string[] Keys;
        public readonly MaterialGlobalIlluminationFlags globalIlluminationFlags;
        private readonly int _HashCode;

        public static ShaderKey Create(Material mat)
        {
            if (mat == null)
                return null;
            if (mat.shader == null)
                return null;


            return new ShaderKey(mat.shader, _Sort(mat.enabledKeywords), mat.globalIlluminationFlags);
        }

        public static ShaderKey Create(ShaderKey other, Shader new_shader)
        {
            if (other == null || new_shader == null)
                return null;
            return new ShaderKey(new_shader, other.Keys, other.globalIlluminationFlags);
        }

        private ShaderKey(Shader shader, string[] keys, MaterialGlobalIlluminationFlags globalIlluminationFlags)
        {
            this.Shader = shader;
            this.Keys = keys;
            this.globalIlluminationFlags = globalIlluminationFlags;

            if (Shader == null)
                this._HashCode = 0;
            else
            {
                int hashCode = Shader.GetHashCode();
                hashCode = System.HashCode.Combine(hashCode, globalIlluminationFlags);

                foreach (var p in Keys)
                    hashCode = System.HashCode.Combine(hashCode, p.GetHashCode());
                this._HashCode = hashCode;
            }
        }

        private static string[] _Sort(UnityEngine.Rendering.LocalKeyword[] keys)
        {
            string[] ret = new string[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                ret[i] = keys[i].name;
            }

            for (int i = 0; i < ret.Length - 1; i++)
            {
                for (int j = i + 1; j < ret.Length; j++)
                {
                    if (ret[i].CompareTo(ret[j]) > 0)
                    {
                        var t = keys[i];
                        keys[i] = keys[j];
                        keys[j] = t;
                    }
                }
            }

            return ret;
        }

        bool IEqualityComparer<ShaderKey>.Equals(ShaderKey x, ShaderKey y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<ShaderKey>.GetHashCode(ShaderKey obj)
        {
            return obj._HashCode;
        }

        public override int GetHashCode()
        {
            return _HashCode;
        }

        public bool Equals(ShaderKey other)
        {
            if (other == this)
                return true;
            if (other == null)
                return false;
            if (other._HashCode != _HashCode)
                return false;

            if (Shader != other.Shader)
                return false;
            if (globalIlluminationFlags != other.globalIlluminationFlags)
                return false;

            if (Keys.Length != other.Keys.Length)
                return false;

            for (int i = 0; i < Keys.Length; i++)
            {
                if (Keys[i] != other.Keys[i])
                    return false;
            }
            return true;
        }
    }
}
