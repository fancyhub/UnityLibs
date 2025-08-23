/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/11/18
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FH.DayNightWeather
{
    public interface IRenderDataSlot
    {
        /// <summary>
        /// 是否有数据, 如果没有数据,就相当于 null操作, copy和 lerp的时候, 忽视该数据
        /// </summary>
        bool HasData { get; }
        void Clear();
        Type DataType { get; }
        bool Dirty { get; set; }

        /// <summary>
        ///  从 from 和 to 做一次lerp, 并且填充到自己的数据里面
        ///  如果 from 和 to 都为空(包括HasData==false), 返回false, 如果有一个不为空(包括HasData==false), 就直接赋值
        /// </summary>
        bool Lerp(IRenderDataSlot from, IRenderDataSlot to, float t);

        bool CopyFrom(IRenderDataSlot src);

        /// <summary>
        /// 复制出来,不带有数据
        /// </summary>
        IRenderDataSlot Clone();
    }

    public interface IRenderDataSlot<T> : IRenderDataSlot
    {
        T Val { get; set; }
    }

    public static class RDSUtil
    {
        public static bool GetExt<T>(this IRenderDataSlot self, out T v)
        {
            if (self == null || !self.HasData)
            {
                v = default(T);
                return false;
            }

            var t = self as IRenderDataSlot<T>;
            if (t == null)
            {
                v = default(T);
                return false;
            }

            v = t.Val;
            return true;
        }

        public static bool LerpSlot(IRenderDataSlot from, IRenderDataSlot to, float t, ref IRenderDataSlot inout_dst)
        {
            //1. 情况1: 如果两个都为空, dst 直接clear
            if (from == null && to == null)
            {
                return false;
            }

            //2. 情况2: 有一个是空的, 直接赋值
            if (from == null || to == null)
            {
                IRenderDataSlot src = from == null ? to : from;

                if (inout_dst == null)
                    inout_dst = src.Clone();

                return inout_dst.CopyFrom(src);
            }

            //3. 情况3: 两个不为空 
            if (inout_dst == null)
                inout_dst = from.Clone();
            return inout_dst.Lerp(from, to, t);
        }

        public static int Lerp(int from, int to, float t)
        {
            return (int)Mathf.Lerp(from, to, t);
        }

        public static Color Lerp(Color from, Color to, float t)
        {
            return Color.Lerp(from, to, t);
        }

        public static bool Lerp(bool from, bool to, float t)
        {
            return t < 0.5f ? from : to;
        }

        public static Quaternion Lerp(Quaternion from, Quaternion to, float t)
        {
            return Quaternion.Lerp(from, to, t);
        }

        public static float Lerp(float from, float to, float t)
        {
            return Mathf.Lerp(from, to, t);
        }

        public static Vector2 Lerp(Vector2 from, Vector2 to, float t)
        {
            return Vector2.Lerp(from, to, t);
        }

        public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
        {
            return Vector3.Lerp(from, to, t);
        }

        public static Vector4 Lerp(Vector4 from, Vector4 to, float t)
        {
            return Vector4.Lerp(from, to, t);
        }

        public static T LerpHalf<T>(T from, T to, float t)
        {
            return t < 0.5f ? from : to;
        }   

        public static T Lerp<T>(T from, T to, float t) where T : class
        {
            return t < 0.5f ? from : to;
        }
        public static bool IsEuqal(bool x, bool y)
        {
            return x == y;
        }

        public static bool IsEuqal(int x, int y)
        {
            return x == y;
        }
        public static bool IsEuqal(float x, float y)
        {
            return Math.Abs(x - y) < 0.001f;
        }
        public static bool IsEuqal(Vector2 x, Vector2 y)
        {
            if (!IsEuqal(x.x, y.x))
                return false;
            if (!IsEuqal(x.y, y.y))
                return false;
            return true;
        }

        public static bool IsEuqal(Vector3 x, Vector3 y)
        {
            if (!IsEuqal(x.x, y.x))
                return false;
            if (!IsEuqal(x.y, y.y))
                return false;
            if (!IsEuqal(x.z, y.z))
                return false;
            return true;
        }

        public static bool IsEuqal(Vector4 x, Vector4 y)
        {
            if (!IsEuqal(x.x, y.x))
                return false;
            if (!IsEuqal(x.y, y.y))
                return false;
            if (!IsEuqal(x.z, y.z))
                return false;
            if (!IsEuqal(x.w, y.w))
                return false;
            return true;
        }

        public static bool IsEuqal(Color x, Color y)
        {
            if (!IsEuqal(x.r, y.r))
                return false;
            if (!IsEuqal(x.g, y.g))
                return false;
            if (!IsEuqal(x.b, y.b))
                return false;
            if (!IsEuqal(x.a, y.a))
                return false;
            return true;
        }
    }
}
