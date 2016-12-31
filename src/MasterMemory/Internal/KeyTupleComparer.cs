using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeroFormatter;

namespace MasterMemory.Internal
{
    // TODO:Test, make all keytuples by T4.
    public static class KeyTupleComparer
    {
        internal static readonly Type[] Types = new[]
        {
            //typeof(KeyTupleComparer<
            null, // 0
            null, // 1
            typeof(KeyTupleComparer<,>),
            typeof(KeyTupleComparer<,,>),
        };
    }


    public class KeyTupleComparer<T1, T2> : IComparer<KeyTuple<T1, T2>>
    {
        static readonly Comparer<T1> comparer1 = Comparer<T1>.Default;
        static readonly Comparer<T2> comparer2 = Comparer<T2>.Default;

        readonly Func<KeyTuple<T1, T2>, KeyTuple<T1, T2>, int> compare;

        public KeyTupleComparer(int compareKeyNumber) // -1 is all.
        {
            if (compareKeyNumber == -1) compare = CompareAll;
            switch (compareKeyNumber)
            {
                case 1:
                    compare = Compare1;
                    break;
                case 2:
                    compare = Compare2;
                    break;
                default:
                    compare = CompareAll;
                    break;
            }
        }

        public int Compare(KeyTuple<T1, T2> x, KeyTuple<T1, T2> y)
        {
            return compare(x, y);
        }

        int Compare1(KeyTuple<T1, T2> x, KeyTuple<T1, T2> y)
        {
            return comparer1.Compare(x.Item1, y.Item1);
        }

        int Compare2(KeyTuple<T1, T2> x, KeyTuple<T1, T2> y)
        {
            return comparer2.Compare(x.Item2, y.Item2);
        }


        int CompareAll(KeyTuple<T1, T2> x, KeyTuple<T1, T2> y)
        {
            int res;
            res = comparer1.Compare(x.Item1, y.Item1);
            if (res != 0) return res;
            res = comparer2.Compare(x.Item2, y.Item2);
            if (res != 0) return res;

            return res;
        }
    }

    public class KeyTupleComparer<T1, T2, T3> : IComparer<KeyTuple<T1, T2, T3>>
    {
        static readonly Comparer<T1> comparer1 = Comparer<T1>.Default;
        static readonly Comparer<T2> comparer2 = Comparer<T2>.Default;
        static readonly Comparer<T3> comparer3 = Comparer<T3>.Default;

        readonly Func<KeyTuple<T1, T2, T3>, KeyTuple<T1, T2, T3>, int> compare;

        public KeyTupleComparer(int compareKeyNumber) // -1 is all.
        {
            if (compareKeyNumber == -1) compare = CompareAll;
            switch (compareKeyNumber)
            {
                case 1:
                    compare = Compare1;
                    break;
                case 2:
                    compare = Compare2;
                    break;
                case 3:
                    compare = Compare3;
                    break;
                default:
                    compare = CompareAll;
                    break;
            }
        }

        public int Compare(KeyTuple<T1, T2, T3> x, KeyTuple<T1, T2, T3> y)
        {
            return compare(x, y);
        }

        int Compare1(KeyTuple<T1, T2, T3> x, KeyTuple<T1, T2, T3> y)
        {
            return comparer1.Compare(x.Item1, y.Item1);
        }

        int Compare2(KeyTuple<T1, T2, T3> x, KeyTuple<T1, T2, T3> y)
        {
            return comparer2.Compare(x.Item2, y.Item2);
        }

        int Compare3(KeyTuple<T1, T2, T3> x, KeyTuple<T1, T2, T3> y)
        {
            return comparer3.Compare(x.Item3, y.Item3);
        }

        int CompareAll(KeyTuple<T1, T2, T3> x, KeyTuple<T1, T2, T3> y)
        {
            int res;
            res = comparer1.Compare(x.Item1, y.Item1);
            if (res != 0) return res;
            res = comparer2.Compare(x.Item2, y.Item2);
            if (res != 0) return res;
            res = comparer3.Compare(x.Item3, y.Item3);
            if (res != 0) return res;

            return res;
        }
    }
}
