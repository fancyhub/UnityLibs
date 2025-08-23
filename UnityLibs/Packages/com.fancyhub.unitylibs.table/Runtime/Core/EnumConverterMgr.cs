using System;
using System.Collections.Generic;

namespace FH
{

    public interface IEnumConverter { }
    public interface ITableEnumConverter<T> : IEnumConverter where T : Enum
    {
        public T Convert(int v);
        public int Convert(T v);
    }

    public class EnumConverterMgr
    {
        public static EnumConverterMgr Inst = new EnumConverterMgr();
        private Dictionary<Type, IEnumConverter> _dict = new Dictionary<Type, IEnumConverter>();

        public static void Reg<T>(ITableEnumConverter<T> convert) where T : Enum
        {
            if (convert == null)
                return;
            Inst._dict[typeof(T)] = convert;
        }
        public static void RegFunc<T>(Func<int, T> to, Func<T, int> rev) where T : Enum
        {
            if (to == null || rev == null)
                return;
            Inst._dict[typeof(T)] = new InnerConverter<T>(to, rev);
        }

        public static bool Convert<T>(int src, ref T dst) where T : Enum
        {
            dst = default(T);
            Inst._dict.TryGetValue(typeof(T), out var it);
            if (it == null)
                return false;
            var itt = it as ITableEnumConverter<T>;
            if (itt == null)
                return false;
            dst = itt.Convert(src);
            return true;
        }

        public static bool Convert<T>(T src, ref int dst) where T : Enum
        {
            dst = 0;
            Inst._dict.TryGetValue(typeof(T), out var it);
            if (it == null)
                return false;
            var itt = it as ITableEnumConverter<T>;
            if (itt == null)
                return false;
            dst = itt.Convert(src);
            return true;
        }

        private class InnerConverter<T> : ITableEnumConverter<T> where T : Enum
        {
            public Func<int, T> _to;
            public Func<T, int> _to2;
            public InnerConverter(Func<int, T> to, Func<T, int> to2)
            {
                _to = to;
                _to2 = to2;
            }
            public T Convert(int v) { return _to(v); }

            public int Convert(T v) { return _to2(v); }
        }
    }

}
