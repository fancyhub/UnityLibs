/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/03/01
 * Title   : 
 * Desc    : 
*************************************************************************************/
using UnityEngine;

namespace FH
{
    public static class FixedMathUnity
    {
        public static Vector2 ToVector2(this FixedVector2 v) => new Vector2(v.X.ToFloat(), v.Y.ToFloat());

        public static FixedVector2 ToFixed(this Vector2 v) =>
            FixedVector2.FromFloat(v.x, v.y);

        public static Vector3 ToVector3(this FixedVector3 v) =>
            new Vector3(v.X.ToFloat(), v.Y.ToFloat(), v.Z.ToFloat());

        public static FixedVector3 ToFixed(this Vector3 v) =>
            FixedVector3.FromFloat(v.x, v.y, v.z);

        public static Vector4 ToVector4(this FixedVector4 v) =>
            new Vector4(v.X.ToFloat(), v.Y.ToFloat(), v.Z.ToFloat(), v.W.ToFloat());

        public static FixedVector4 ToFixed(this Vector4 v) =>
            new FixedVector4(
                Fixed32.FromFloat(v.x),
                Fixed32.FromFloat(v.y),
                Fixed32.FromFloat(v.z),
                Fixed32.FromFloat(v.w));

        public static Quaternion ToQuaternion(this FixedQuaternion q) =>
            new Quaternion(q.X.ToFloat(), q.Y.ToFloat(), q.Z.ToFloat(), q.W.ToFloat());

        public static FixedQuaternion ToFixed(this Quaternion q) =>
            new FixedQuaternion(
                Fixed32.FromFloat(q.x),
                Fixed32.FromFloat(q.y),
                Fixed32.FromFloat(q.z),
                Fixed32.FromFloat(q.w));
    }
}
