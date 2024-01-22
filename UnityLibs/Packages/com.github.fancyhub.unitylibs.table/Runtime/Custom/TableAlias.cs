using System;
using System.Collections.Generic;

namespace FH
{
    public static class TableAlias
    {
        public static void Create(ref PairItemIntInt64 v, bool hasV, (int, long) v2)
        {
            if (!hasV)
                v = null;
            else
                v = new PairItemIntInt64();
        }

        public static void Create(ref PairItemIntBool v, bool hasV, (int, bool) v2)
        {
            if (!hasV)
                v = null;

            v = new PairItemIntBool()
            {
                Key = v2.Item1,
                Value = v2.Item2
            };
        }

        public static void Create(ref PairItemIntBool v, bool hasV, (int, int) v2)
        {
            if (!hasV)
            {
                v = null;
            }

            v = new PairItemIntBool()
            {
                Key = v2.Item1,
            };
        }

        public static void Create(ref UnityEngine.Vector3 v, bool hasV, (float, float, float) v2)
        {
            if (!hasV)
            {
                return;
            }

            v = new UnityEngine.Vector3()
            {
                x = v2.Item1,
                y = v2.Item2,
                z = v2.Item3,
            };
        }
    }
}
