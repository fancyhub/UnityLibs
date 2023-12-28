/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/6/7 11:12:44
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;


namespace FH
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Idx : IEquatable<Idx>, IEqualityComparer<Idx>
    {
        static Idx()
        {
            MyEqualityComparer.Reg(new Idx());
        }

        public enum EType
        {
            none,
            i32,
            u32,
            i64,
            u64,
            str,
            max
        }

        public static Idx Empty = new Idx() { _type = EType.none };

        [FieldOffset(0)]
        public Str _str;
        [FieldOffset(0)]
        public long _i64;
        [FieldOffset(0)]
        public int _i32;
        [FieldOffset(0)]
        public ulong _u64;
        [FieldOffset(0)]
        public uint _u32;

        [FieldOffset(16)]
        public EType _type;

        public Idx(Str str)
        {
            _i32 = 0;
            _i64 = 0;
            _u32 = 0;
            _u64 = 0;

            _type = EType.str;
            _str = str;
        }

        public Idx(int v)
        {
            _i64 = 0;
            _u32 = 0;
            _u64 = 0;
            _str = new Str();

            _type = EType.i32;
            _i32 = v;
        }

        public Idx(long v)
        {
            _i32 = 0;
            _u32 = 0;
            _u64 = 0;
            _str = new Str();

            _type = EType.i64;
            _i64 = v;
        }

        public Idx(uint v)
        {
            _i32 = 0;
            _i64 = 0;
            _u64 = 0;
            _str = new Str();

            _type = EType.u32;
            _u32 = v;
        }

        public Idx(ulong v)
        {
            _i32 = 0;
            _u32 = 0;
            _i64 = 0;
            _str = new Str();

            _type = EType.u64;
            _u64 = v;
        }

        public void ToStr(System.Text.StringBuilder sb)
        {
            switch (_type)
            {
                case EType.i32:
                    sb.Append(_i32);
                    return;

                case EType.u32:
                    sb.Append(_u32);
                    return;

                case EType.i64:
                    sb.Append(_i64);
                    return;

                case EType.u64:
                    sb.Append(_u64);
                    return;

                case EType.str:
                    _str.ToStr(sb);
                    return;
                default:
                    return;
            }
        }

        public override string ToString()
        {
            switch (_type)
            {
                case EType.i32:
                    return _i32.ToString();

                case EType.u32:
                    return _u32.ToString();

                case EType.i64:
                    return _i64.ToString();

                case EType.u64:
                    return _u64.ToString();

                case EType.str:
                    return _str.ToString();
                default:
                    return "";
            }
        }

        public override int GetHashCode()
        {
            switch (_type)
            {
                case EType.i32:
                case EType.u32:
                    return _i32;

                case EType.i64:
                case EType.u64:
                    return (int)(_i64);

                case EType.str:
                    if (_str == null)
                        return 0;
                    return _str.GetHashCode();
                default:
                    return 0;
            }
        }

        public bool Equals(Idx other)
        {
            if (_type != other._type)
                return false;

            switch (other._type)
            {
                case EType.i32:
                case EType.u32:
                    return _i32 == other._i32;
                case EType.i64:
                case EType.u64:
                    return _i64 == other._i64;
                case EType.str:
                    return _str == other._str;
                default:
                    return true;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is Idx))
                return false;
            return Equals((Idx)obj);
        }

        public static bool operator ==(Idx self, Idx other)
        {
            return self.Equals(other);
        }

        public static bool operator !=(Idx self, Idx other)
        {
            return !(self == other);
        }

        public static implicit operator Idx(int v) { return new Idx(v); }
        public static implicit operator Idx(long v) { return new Idx(v); }
        public static implicit operator Idx(uint v) { return new Idx(v); }
        public static implicit operator Idx(ulong v) { return new Idx(v); }
        public static implicit operator Idx(Str v) { return new Idx(v); }
        public static implicit operator Idx(string v) { return new Idx(new Str(v)); }


        bool IEqualityComparer<Idx>.Equals(Idx x, Idx y)
        {
            return x.Equals(y);
        }

        int IEqualityComparer<Idx>.GetHashCode(Idx key)
        {
            return key.GetHashCode();
        }
    }
}
