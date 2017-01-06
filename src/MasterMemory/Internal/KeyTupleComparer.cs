 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeroFormatter;
using ZeroFormatter.Formatters;
using ZeroFormatter.Internal;

namespace MasterMemory.Internal
{
    public static class KeyTupleComparer
    {
        internal static readonly Type[] Types = new[]
        {
            null, // 0
            null, // 1
            typeof(KeyTupleComparer<,>),
            typeof(KeyTupleComparer<,,>),
            typeof(KeyTupleComparer<,,,>),
            typeof(KeyTupleComparer<,,,,>),
            typeof(KeyTupleComparer<,,,,,>),
            typeof(KeyTupleComparer<,,,,,,>)
        };
    }


    public class KeyTupleComparer<T1, T2> : IComparer<KeyTuple<T1, T2>>
    {
        readonly IComparer<T1> comparer1 = MasterMemoryComparer<T1>.Default;
        readonly IComparer<T2> comparer2 = MasterMemoryComparer<T2>.Default;

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
        readonly IComparer<T1> comparer1 = MasterMemoryComparer<T1>.Default;
        readonly IComparer<T2> comparer2 = MasterMemoryComparer<T2>.Default;
        readonly IComparer<T3> comparer3 = MasterMemoryComparer<T3>.Default;

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

    public class KeyTupleComparer<T1, T2, T3, T4> : IComparer<KeyTuple<T1, T2, T3, T4>>
    {
        readonly IComparer<T1> comparer1 = MasterMemoryComparer<T1>.Default;
        readonly IComparer<T2> comparer2 = MasterMemoryComparer<T2>.Default;
        readonly IComparer<T3> comparer3 = MasterMemoryComparer<T3>.Default;
        readonly IComparer<T4> comparer4 = MasterMemoryComparer<T4>.Default;

        readonly Func<KeyTuple<T1, T2, T3, T4>, KeyTuple<T1, T2, T3, T4>, int> compare;

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
                case 4:
                    compare = Compare4;
                    break;
                default:
                    compare = CompareAll;
                    break;
            }
        }

        public int Compare(KeyTuple<T1, T2, T3, T4> x, KeyTuple<T1, T2, T3, T4> y)
        {
            return compare(x, y);
        }


        int Compare1(KeyTuple<T1, T2, T3, T4> x, KeyTuple<T1, T2, T3, T4> y)
        {
            return comparer1.Compare(x.Item1, y.Item1);
        }


        int Compare2(KeyTuple<T1, T2, T3, T4> x, KeyTuple<T1, T2, T3, T4> y)
        {
            return comparer2.Compare(x.Item2, y.Item2);
        }


        int Compare3(KeyTuple<T1, T2, T3, T4> x, KeyTuple<T1, T2, T3, T4> y)
        {
            return comparer3.Compare(x.Item3, y.Item3);
        }


        int Compare4(KeyTuple<T1, T2, T3, T4> x, KeyTuple<T1, T2, T3, T4> y)
        {
            return comparer4.Compare(x.Item4, y.Item4);
        }


        int CompareAll(KeyTuple<T1, T2, T3, T4> x, KeyTuple<T1, T2, T3, T4> y)
        {
            int res;
            res = comparer1.Compare(x.Item1, y.Item1);
            if (res != 0) return res;
            res = comparer2.Compare(x.Item2, y.Item2);
            if (res != 0) return res;
            res = comparer3.Compare(x.Item3, y.Item3);
            if (res != 0) return res;
            res = comparer4.Compare(x.Item4, y.Item4);
            if (res != 0) return res;
            return res;
        }
    }

    public class KeyTupleComparer<T1, T2, T3, T4, T5> : IComparer<KeyTuple<T1, T2, T3, T4, T5>>
    {
        readonly IComparer<T1> comparer1 = MasterMemoryComparer<T1>.Default;
        readonly IComparer<T2> comparer2 = MasterMemoryComparer<T2>.Default;
        readonly IComparer<T3> comparer3 = MasterMemoryComparer<T3>.Default;
        readonly IComparer<T4> comparer4 = MasterMemoryComparer<T4>.Default;
        readonly IComparer<T5> comparer5 = MasterMemoryComparer<T5>.Default;

        readonly Func<KeyTuple<T1, T2, T3, T4, T5>, KeyTuple<T1, T2, T3, T4, T5>, int> compare;

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
                case 4:
                    compare = Compare4;
                    break;
                case 5:
                    compare = Compare5;
                    break;
                default:
                    compare = CompareAll;
                    break;
            }
        }

        public int Compare(KeyTuple<T1, T2, T3, T4, T5> x, KeyTuple<T1, T2, T3, T4, T5> y)
        {
            return compare(x, y);
        }


        int Compare1(KeyTuple<T1, T2, T3, T4, T5> x, KeyTuple<T1, T2, T3, T4, T5> y)
        {
            return comparer1.Compare(x.Item1, y.Item1);
        }


        int Compare2(KeyTuple<T1, T2, T3, T4, T5> x, KeyTuple<T1, T2, T3, T4, T5> y)
        {
            return comparer2.Compare(x.Item2, y.Item2);
        }


        int Compare3(KeyTuple<T1, T2, T3, T4, T5> x, KeyTuple<T1, T2, T3, T4, T5> y)
        {
            return comparer3.Compare(x.Item3, y.Item3);
        }


        int Compare4(KeyTuple<T1, T2, T3, T4, T5> x, KeyTuple<T1, T2, T3, T4, T5> y)
        {
            return comparer4.Compare(x.Item4, y.Item4);
        }


        int Compare5(KeyTuple<T1, T2, T3, T4, T5> x, KeyTuple<T1, T2, T3, T4, T5> y)
        {
            return comparer5.Compare(x.Item5, y.Item5);
        }


        int CompareAll(KeyTuple<T1, T2, T3, T4, T5> x, KeyTuple<T1, T2, T3, T4, T5> y)
        {
            int res;
            res = comparer1.Compare(x.Item1, y.Item1);
            if (res != 0) return res;
            res = comparer2.Compare(x.Item2, y.Item2);
            if (res != 0) return res;
            res = comparer3.Compare(x.Item3, y.Item3);
            if (res != 0) return res;
            res = comparer4.Compare(x.Item4, y.Item4);
            if (res != 0) return res;
            res = comparer5.Compare(x.Item5, y.Item5);
            if (res != 0) return res;
            return res;
        }
    }

    public class KeyTupleComparer<T1, T2, T3, T4, T5, T6> : IComparer<KeyTuple<T1, T2, T3, T4, T5, T6>>
    {
        readonly IComparer<T1> comparer1 = MasterMemoryComparer<T1>.Default;
        readonly IComparer<T2> comparer2 = MasterMemoryComparer<T2>.Default;
        readonly IComparer<T3> comparer3 = MasterMemoryComparer<T3>.Default;
        readonly IComparer<T4> comparer4 = MasterMemoryComparer<T4>.Default;
        readonly IComparer<T5> comparer5 = MasterMemoryComparer<T5>.Default;
        readonly IComparer<T6> comparer6 = MasterMemoryComparer<T6>.Default;

        readonly Func<KeyTuple<T1, T2, T3, T4, T5, T6>, KeyTuple<T1, T2, T3, T4, T5, T6>, int> compare;

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
                case 4:
                    compare = Compare4;
                    break;
                case 5:
                    compare = Compare5;
                    break;
                case 6:
                    compare = Compare6;
                    break;
                default:
                    compare = CompareAll;
                    break;
            }
        }

        public int Compare(KeyTuple<T1, T2, T3, T4, T5, T6> x, KeyTuple<T1, T2, T3, T4, T5, T6> y)
        {
            return compare(x, y);
        }


        int Compare1(KeyTuple<T1, T2, T3, T4, T5, T6> x, KeyTuple<T1, T2, T3, T4, T5, T6> y)
        {
            return comparer1.Compare(x.Item1, y.Item1);
        }


        int Compare2(KeyTuple<T1, T2, T3, T4, T5, T6> x, KeyTuple<T1, T2, T3, T4, T5, T6> y)
        {
            return comparer2.Compare(x.Item2, y.Item2);
        }


        int Compare3(KeyTuple<T1, T2, T3, T4, T5, T6> x, KeyTuple<T1, T2, T3, T4, T5, T6> y)
        {
            return comparer3.Compare(x.Item3, y.Item3);
        }


        int Compare4(KeyTuple<T1, T2, T3, T4, T5, T6> x, KeyTuple<T1, T2, T3, T4, T5, T6> y)
        {
            return comparer4.Compare(x.Item4, y.Item4);
        }


        int Compare5(KeyTuple<T1, T2, T3, T4, T5, T6> x, KeyTuple<T1, T2, T3, T4, T5, T6> y)
        {
            return comparer5.Compare(x.Item5, y.Item5);
        }


        int Compare6(KeyTuple<T1, T2, T3, T4, T5, T6> x, KeyTuple<T1, T2, T3, T4, T5, T6> y)
        {
            return comparer6.Compare(x.Item6, y.Item6);
        }


        int CompareAll(KeyTuple<T1, T2, T3, T4, T5, T6> x, KeyTuple<T1, T2, T3, T4, T5, T6> y)
        {
            int res;
            res = comparer1.Compare(x.Item1, y.Item1);
            if (res != 0) return res;
            res = comparer2.Compare(x.Item2, y.Item2);
            if (res != 0) return res;
            res = comparer3.Compare(x.Item3, y.Item3);
            if (res != 0) return res;
            res = comparer4.Compare(x.Item4, y.Item4);
            if (res != 0) return res;
            res = comparer5.Compare(x.Item5, y.Item5);
            if (res != 0) return res;
            res = comparer6.Compare(x.Item6, y.Item6);
            if (res != 0) return res;
            return res;
        }
    }

    public class KeyTupleComparer<T1, T2, T3, T4, T5, T6, T7> : IComparer<KeyTuple<T1, T2, T3, T4, T5, T6, T7>>
    {
        readonly IComparer<T1> comparer1 = MasterMemoryComparer<T1>.Default;
        readonly IComparer<T2> comparer2 = MasterMemoryComparer<T2>.Default;
        readonly IComparer<T3> comparer3 = MasterMemoryComparer<T3>.Default;
        readonly IComparer<T4> comparer4 = MasterMemoryComparer<T4>.Default;
        readonly IComparer<T5> comparer5 = MasterMemoryComparer<T5>.Default;
        readonly IComparer<T6> comparer6 = MasterMemoryComparer<T6>.Default;
        readonly IComparer<T7> comparer7 = MasterMemoryComparer<T7>.Default;

        readonly Func<KeyTuple<T1, T2, T3, T4, T5, T6, T7>, KeyTuple<T1, T2, T3, T4, T5, T6, T7>, int> compare;

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
                case 4:
                    compare = Compare4;
                    break;
                case 5:
                    compare = Compare5;
                    break;
                case 6:
                    compare = Compare6;
                    break;
                case 7:
                    compare = Compare7;
                    break;
                default:
                    compare = CompareAll;
                    break;
            }
        }

        public int Compare(KeyTuple<T1, T2, T3, T4, T5, T6, T7> x, KeyTuple<T1, T2, T3, T4, T5, T6, T7> y)
        {
            return compare(x, y);
        }


        int Compare1(KeyTuple<T1, T2, T3, T4, T5, T6, T7> x, KeyTuple<T1, T2, T3, T4, T5, T6, T7> y)
        {
            return comparer1.Compare(x.Item1, y.Item1);
        }


        int Compare2(KeyTuple<T1, T2, T3, T4, T5, T6, T7> x, KeyTuple<T1, T2, T3, T4, T5, T6, T7> y)
        {
            return comparer2.Compare(x.Item2, y.Item2);
        }


        int Compare3(KeyTuple<T1, T2, T3, T4, T5, T6, T7> x, KeyTuple<T1, T2, T3, T4, T5, T6, T7> y)
        {
            return comparer3.Compare(x.Item3, y.Item3);
        }


        int Compare4(KeyTuple<T1, T2, T3, T4, T5, T6, T7> x, KeyTuple<T1, T2, T3, T4, T5, T6, T7> y)
        {
            return comparer4.Compare(x.Item4, y.Item4);
        }


        int Compare5(KeyTuple<T1, T2, T3, T4, T5, T6, T7> x, KeyTuple<T1, T2, T3, T4, T5, T6, T7> y)
        {
            return comparer5.Compare(x.Item5, y.Item5);
        }


        int Compare6(KeyTuple<T1, T2, T3, T4, T5, T6, T7> x, KeyTuple<T1, T2, T3, T4, T5, T6, T7> y)
        {
            return comparer6.Compare(x.Item6, y.Item6);
        }


        int Compare7(KeyTuple<T1, T2, T3, T4, T5, T6, T7> x, KeyTuple<T1, T2, T3, T4, T5, T6, T7> y)
        {
            return comparer7.Compare(x.Item7, y.Item7);
        }


        int CompareAll(KeyTuple<T1, T2, T3, T4, T5, T6, T7> x, KeyTuple<T1, T2, T3, T4, T5, T6, T7> y)
        {
            int res;
            res = comparer1.Compare(x.Item1, y.Item1);
            if (res != 0) return res;
            res = comparer2.Compare(x.Item2, y.Item2);
            if (res != 0) return res;
            res = comparer3.Compare(x.Item3, y.Item3);
            if (res != 0) return res;
            res = comparer4.Compare(x.Item4, y.Item4);
            if (res != 0) return res;
            res = comparer5.Compare(x.Item5, y.Item5);
            if (res != 0) return res;
            res = comparer6.Compare(x.Item6, y.Item6);
            if (res != 0) return res;
            res = comparer7.Compare(x.Item7, y.Item7);
            if (res != 0) return res;
            return res;
        }
    }
}