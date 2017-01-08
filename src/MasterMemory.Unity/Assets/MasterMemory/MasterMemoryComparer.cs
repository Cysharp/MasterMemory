using MasterMemory.Internal;
using System.Collections.Generic;
using ZeroFormatter;

namespace MasterMemory
{
    /// <summary>
    /// Replacable Comaprer`T`.Default.
    /// </summary>
    public static class MasterMemoryComparer<T>
    {
        static IComparer<T> _default;

        public static IComparer<T> Default
        {
            get
            {
                return _default ?? (_default = Comparer<T>.Default);
            }
            set
            {
                _default = value;
            }
        }

        static IComparer<T>[] _array;

        public static IComparer<T>[] DefaultArray
        {
            get
            {
                return _array ?? (_array = new[] { Default });
            }
            set
            {
                _array = value;
            }
        }
    }

    public static class UnityTypeHint
    {
        public static void KeyTupleRegister<T1, T2>()
        {
            var comparers = MasterMemoryComparer<KeyTuple<T1, T2>>.DefaultArray;
            if (comparers.Length != 1) return;

            comparers = new IComparer<KeyTuple<T1, T2>>[]
            {
                new KeyTupleComparer<T1, T2>(1),
                new KeyTupleComparer<T1, T2>(2),
                new KeyTupleComparer<T1, T2>(-1),
            };

            MasterMemoryComparer<KeyTuple<T1, T2>>.DefaultArray = comparers;
        }
    }
}
