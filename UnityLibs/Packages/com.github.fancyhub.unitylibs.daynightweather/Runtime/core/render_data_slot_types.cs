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
    /// <summary>
    /// 引用类型
    /// </summary>
    public class RDSObj<T> : RenderDataSlotBase<T> where T : class
    {
        public override IRenderDataSlot Clone()
        {
            return new RDSObj<T>();
        }

        public override T LerpData(T from, T to, float t)
        {
            return t < 0.5f ? from : to;
        }

        public override bool IsDataEuqal(ref T x, ref T y)
        {
            return object.ReferenceEquals(x, y);
        }
    }

    public interface IRDSStruct<T>
    {
        bool IsDataEuqal(ref T other);
        void LerpData(ref T from, ref T to, float t);
    }

    public class RDSStruct<T> : RenderDataSlotBase<T> where T : struct, IRDSStruct<T>
    {
        public override IRenderDataSlot Clone()
        {
            return new RDSStruct<T>();
        }

        public override T LerpData(T from, T to, float t)
        {
            T ret = new T();
            ret.LerpData(ref from, ref to, t);
            return ret;
        }

        public override bool IsDataEuqal(ref T x, ref T y)
        {
            return x.IsDataEuqal(ref y);
        }
    }

    public class RDSQuaternion : RenderDataSlotBase<Quaternion>
    {
        public override IRenderDataSlot Clone()
        {
            return new RDSQuaternion();
        }

        public override Quaternion LerpData(Quaternion from, Quaternion to, float t)
        {
            return Quaternion.Lerp(from, to, t);
        }

        public override bool IsDataEuqal(ref Quaternion x, ref Quaternion y)
        {
            return x == y;
        }
    }

    public class RDSBoolean : RenderDataSlotBase<bool>
    {
        public override IRenderDataSlot Clone()
        {
            return new RDSBoolean();
        }

        public override bool LerpData(bool from, bool to, float t)
        {
            return t < 0.5f ? from : to;
        }

        public override bool IsDataEuqal(ref bool x, ref bool y)
        {
            return x == y;
        }
    }

    public class RDSFloat : RenderDataSlotBase<float>
    {
        public override IRenderDataSlot Clone()
        {
            return new RDSFloat();
        }

        public override float LerpData(float from, float to, float t)
        {
            return Mathf.Lerp(from, to, t);
        }

        public override bool IsDataEuqal(ref float x, ref float y)
        {
            return Mathf.Abs(x - y) < 0.0001f;
        }
    }

    public class RDSVector2 : RenderDataSlotBase<Vector2>
    {
        public override IRenderDataSlot Clone()
        {
            return new RDSVector2();
        }

        public override Vector2 LerpData(Vector2 from, Vector2 to, float t)
        {
            return Vector2.Lerp(from, to, t);
        }

        public override bool IsDataEuqal(ref Vector2 x, ref Vector2 y)
        {
            return x == y;
        }
    }

    public class RDSVector3 : RenderDataSlotBase<Vector3>
    {
        public override IRenderDataSlot Clone()
        {
            return new RDSVector3();
        }

        public override Vector3 LerpData(Vector3 from, Vector3 to, float t)
        {
            return Vector3.Lerp(from, to, t);
        }

        public override bool IsDataEuqal(ref Vector3 x, ref Vector3 y)
        {
            return x == y;
        }
    }

    public class RDSVector4 : RenderDataSlotBase<Vector4>
    {
        public override IRenderDataSlot Clone()
        {
            return new RDSVector4();
        }

        public override Vector4 LerpData(Vector4 from, Vector4 to, float t)
        {
            return Vector4.Lerp(from, to, t);
        }

        public override bool IsDataEuqal(ref Vector4 x, ref Vector4 y)
        {
            return x == y;
        }
    }

    public class RDSInt : RenderDataSlotBase<int>
    {
        public override IRenderDataSlot Clone()
        {
            return new RDSInt();
        }

        public override int LerpData(int from, int to, float t)
        {
            return (int)Mathf.Lerp(from, to, t);
        }

        public override bool IsDataEuqal(ref int x, ref int y)
        {
            return x == y;
        }
    }

    public class RDSColor : RenderDataSlotBase<Color>
    {
        public override IRenderDataSlot Clone()
        {
            return new RDSColor();
        }

        public override Color LerpData(Color from, Color to, float t)
        {
            return Color.Lerp(from, to, t);
        }

        public override bool IsDataEuqal(ref Color x, ref Color y)
        {
            return x == y;
        }
    }

}
